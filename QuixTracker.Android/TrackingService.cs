using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Threading;
using Android.Hardware;
using System.Collections.Concurrent;
using QuixTracker.Services;
using System.Text.Json;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;
using Android.Content.PM;
using QuixTracker.Models;
using Android.Bluetooth;

namespace QuixTracker.Droid
{

    [Service(ForegroundServiceType = ForegroundService.TypeLocation, Exported = true)]
	public class TrackingService : Service, ISensorEventListener
	{
		private BlockingCollection<ParameterDataDTO> locationQueue = new BlockingCollection<ParameterDataDTO>(new ConcurrentQueue<ParameterDataDTO>());

		private string streamId;
		private long lastTimeStamp;

		private bool isRunning;
		private Task task;
		private NotificationService notificationService;
		private CancellationTokenSource cancellationTokenSource;
		private HeartRateDiscovery heartRateDiscovery;
		private SensorManager sensorManager;
		private ConcurrentDictionary<long, Tuple<double, double, double>> gforces = new ConcurrentDictionary<long, Tuple<double, double, double>>();
		private ConcurrentDictionary<long, double> temperatures = new ConcurrentDictionary<long, double>();
		private ConnectionService connectionService;
		private QuixService quixService;
		private DateTime lastErrorMessage = DateTime.Now;
		private Sensor gyroSensor;
		private Sensor tempSensor;
		private PackageInfo packageInfo;
		private CurrentData currentData;
        private LoggingService loggingService;
        private Task queueConsumer;
        private BluetoothAdapter btAdapter;

        public TrackingService()
		{
			this.connectionService = ConnectionService.Instance;
			this.quixService = new QuixService(this.connectionService);

			var context = Android.App.Application.Context;

			this.packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);

			this.currentData = new CurrentData();
			this.loggingService = LoggingService.Instance;
		}

		#region overrides

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public void Start()
		{
			Logger.Instance.Log("Starting tracking service");
		}

		public override void OnCreate()
		{
			isRunning = false;

            this.notificationService = new NotificationService(GetSystemService(Context.NotificationService) as NotificationManager, this);

            task = new Task(DoWork);
			this.notificationService.SendForegroundNotification("Quix Tracker", "Initializing services...");
            this.cancellationTokenSource = new CancellationTokenSource();
		}

		public override async void OnDestroy()
		{
			try
			{
				this.cancellationTokenSource.Cancel();

				this.connectionService.OnOutputConnectionChanged(ConnectionState.Draining);

				try
				{

					await this.queueConsumer;
				}
				catch (System.OperationCanceledException)
				{

				}

				try
				{
                    await this.quixService.CloseStream(this.streamId);
                }
				catch (Exception ex)
				{
					this.loggingService.LogError($"Failed to close stream: {streamId ?? String.Empty}", ex);
				}

				isRunning = false;

				if (task != null && task.Status == TaskStatus.RanToCompletion)
				{
					task.Dispose();
				}
			}
			catch (Exception ex)
			{
				this.connectionService.OnConnectionError(ex.Message, ex);
				this.lastErrorMessage = DateTime.Now;
			}
			finally
			{
				StopSelf();
				this.quixService.Dispose();
				OnPause();
				await CrossGeolocator.Current.StopListeningAsync();
				this.connectionService.ClearError();
				this.connectionService.OnOutputConnectionChanged(ConnectionState.Disconnected);
			}
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			if (!isRunning)
			{
				isRunning = true;
				task.Start();
			}
			return StartCommandResult.Sticky;
		}

		public void StartForegroundServiceCompat()
		{

		}

		#endregion

		public async void DoWork()
		{
            var settingsMessage = this.connectionService.Settings.CheckSettings();
            if (settingsMessage != "")
			{
				this.notificationService = new NotificationService(GetSystemService(Context.NotificationService) as NotificationManager, this);
				this.notificationService.SendForegroundNotification("Quix Tracker", $"Error: {settingsMessage}");
				Logger.Instance.Log("Settings Error: " + settingsMessage);
			}

            this.sensorManager = GetSystemService(Context.SensorService) as SensorManager;

			this.gyroSensor = this.sensorManager.GetDefaultSensor(SensorType.Accelerometer);
			this.tempSensor = this.sensorManager.GetDefaultSensor(SensorType.AmbientTemperature);


			OnResume();
		
			try
			{
				this.btAdapter = BluetoothAdapter.DefaultAdapter;
				if (this.btAdapter != null)
				{
					btAdapter.StartDiscovery();
					this.heartRateDiscovery = new HeartRateDiscovery(this.btAdapter, Application.Context, this.connectionService, this.currentData, this.locationQueue, cancellationTokenSource.Token);
					this.heartRateDiscovery.Connect();
				}
				else
				{
                    LoggingService.Instance.LogInformation("Abort heart rate tracking: bluetooth adapter not found");
				}
			}
			catch(Exception ex)
			{
                this.connectionService.OnConnectionError("Bluetooth discovery failed", ex);
            }
			try
			{
				this.connectionService.OnOutputConnectionChanged(ConnectionState.Connecting);
				try
				{
                    await RetryService.Execute(async () => await this.quixService.StartInputConnection());
                }
				catch (Exception ex)
				{
					this.loggingService.LogError("Failed to start input connection", ex);
				}

                await RetryService.Execute(async () => await this.quixService.StartOutputConnection(), -1, 1000, (ex) =>
				{
					this.notificationService.SendForegroundNotification("Quix Tracker", "Failed to connect to Quix");
					this.connectionService.OnConnectionError("Failed to connect to Quix", ex);
                });
                
				this.CleanErrorMessage();

                this.streamId = await RetryService.Execute(async () => await this.quixService.CreateStream(
                    this.connectionService.Settings.DeviceId,
                    this.connectionService.Settings.Rider,
                    this.connectionService.Settings.Team,
                    this.connectionService.Settings.SessionName), -1, 1000, (ex) => this.connectionService.OnConnectionError("Error: failed to create output stream", ex));
                
				this.CleanErrorMessage();

                try
				{
                    await RetryService.Execute(async () => await this.quixService.SubscribeToEvent(this.streamId, "notification"));
                }
				catch (Exception ex)
				{
                    this.loggingService.LogError("Failed to subscribe to notifications", ex);
                }

                this.quixService.EventDataRecieved += QuixService_EventDataRecieved;

				await this.StartGeoLocationTracking();
				this.queueConsumer = this.ConsumeQueue();

                this.notificationService.SendForegroundNotification("Quix Tracker", "Tracking in progress...");
                this.loggingService.LogInformation("Tracking in progress");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                if (this.connectionService.Settings.LogGForce)
				{
					this.gforceTracking();
				}
				#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

				this.connectionService.OnOutputConnectionChanged(ConnectionState.Connected);
			}
			catch (Exception ex)
			{
				Logger.Instance.Log(ex.ToString());
 				this.connectionService.OnConnectionError(ex.Message, ex);
				this.lastErrorMessage = DateTime.Now;
				this.connectionService.OnInputConnectionChanged(ConnectionState.Disconnected);
				StopSelf();
				this.quixService.Dispose();
				OnPause();
				await CrossGeolocator.Current.StopListeningAsync();
			}
		}


		private void QuixService_EventDataRecieved(object sender, EventDataDTO e)
		{
			var notification = JsonSerializer.Deserialize<NotificationDTO>(e.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			this.notificationService.SendNotifcation(notification.Title, notification.Content);
			this.currentData.Message = $"{DateTime.Now.TimeOfDay.ToString()}: {notification.Title}\n {notification.Content}";
		}

		private void OnResume()
		{
			this.sensorManager.RegisterListener(this, this.gyroSensor, SensorDelay.Normal);
			this.sensorManager.RegisterListener(this, this.tempSensor, SensorDelay.Normal);
		}

		private void OnPause()
		{
			this.sensorManager.UnregisterListener(this, this.gyroSensor);
			this.sensorManager.UnregisterListener(this, this.tempSensor);

		}

		private async Task ConsumeQueue()
		{
			ParameterDataDTO data = null;
			while (!this.cancellationTokenSource.IsCancellationRequested || this.locationQueue.Count > 0)
			{
				try
				{
                    LoggingService.Instance.LogTrace("Queue: " + this.locationQueue.Count);

                    if (!this.locationQueue.TryTake(out data))
                    {
                        data = this.locationQueue.Take(this.cancellationTokenSource.Token);
                    }

                    LoggingService.Instance.LogTrace("Queue: " + this.locationQueue.Count);

					await this.quixService.SendParameterData(this.streamId, data);
                    

					this.connectionService.OnOutputConnectionChanged(
						this.cancellationTokenSource.IsCancellationRequested ? ConnectionState.Draining : ConnectionState.Connected);

					this.CleanErrorMessage();
				}
				catch (Exception ex)
				{
					this.connectionService.OnConnectionError("Error sending data: connection error", ex);
					this.lastErrorMessage = DateTime.Now;
				}

				if (this.locationQueue.Count >= 0)
				{
					this.currentData.LocationBufferSize = this.locationQueue.Count;
					this.connectionService.OnDataReceived(currentData);
				}
			}
		}

		async Task StartGeoLocationTracking()
		{
			CrossGeolocator.Current.DesiredAccuracy = 5;
			await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromMilliseconds(this.connectionService.Settings.Interval), 1, true);

			CrossGeolocator.Current.PositionChanged += Geolocator_PositionChanged;
		}

		private void Geolocator_PositionChanged(object sender, PositionEventArgs e)
		{
            var location = e.Position;

            this.currentData.Speed = location.Speed;
            this.currentData.Accuracy = location.Accuracy;
            this.currentData.Altitude = location.Altitude;
            this.currentData.Bearing = (float)location.Heading;
            this.connectionService.OnDataReceived(currentData);
            this.locationQueue.Add(GetParameterDataDTO(location));
            LoggingService.Instance.LogTrace("Geolocator_PositionChanged");

        }


        private async Task gforceTracking()
		{
			while (!this.cancellationTokenSource.IsCancellationRequested)
			{

				if (!this.gforces.IsEmpty)
				{
					var timestamps = this.gforces.ToArray();
					this.gforces.Clear();

					this.locationQueue.Add(new ParameterDataDTO
					{

						Timestamps = new long[] { timestamps.First().Key },
						NumericValues = new Dictionary<string, double[]>()
							{
								{ "gForceX", new []{ timestamps.Average(s => s.Value.Item1) } },
								{ "gForceY", new []{ timestamps.Average(s => s.Value.Item2) } },
								{ "gForceZ", new[] { timestamps.Average(s => s.Value.Item3) } },
							},
						TagValues = new Dictionary<string, string[]>()
							{
								{"version", new string[]{ this.packageInfo.VersionName } },
								{"rider", new string[]{ this.connectionService.Settings.Rider} },
								{"team", new string[]{ this.connectionService.Settings.Team} },
							}
					});

					await Task.Delay(this.connectionService.Settings.Interval);
					LoggingService.Instance.LogTrace("Gforce");
				}
			}
		}

		public void OnSensorChanged(SensorEvent e)
		{
			switch (e.Sensor.Type)
			{
				case SensorType.Accelerometer:
					this.gforces.TryAdd((DateTime.UtcNow - new DateTime(1970, 1, 1)).Ticks * 100, new Tuple<double, double, double>(e.Values[0], e.Values[1], e.Values[2]));
					break;
				case SensorType.AmbientTemperature:
					this.temperatures.TryAdd((DateTime.UtcNow - new DateTime(1970, 1, 1)).Ticks * 100, e.Values[0]);
					break;
			}
		}

		private void CleanErrorMessage()
		{
			if ((DateTime.Now - lastErrorMessage).TotalSeconds > 10)
			{
				this.connectionService.ClearError();

			}
		}

		public ParameterDataDTO GetParameterDataDTO(Position location)
		{
			this.lastTimeStamp = (long)(location.Timestamp.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000;

			return new ParameterDataDTO
			{
				Timestamps = new[] { lastTimeStamp },
				NumericValues = new Dictionary<string, double[]>
							 {
								 { "Accuracy" , new[] { (double)location.Accuracy } },
								 { "Altitude" , new[] { location.Altitude } },
								 { "Heading" , new[] { (double)location.Heading } },
								 { "Latitude" , new[] { location.Latitude } },
								 { "Longitude" , new[] { location.Longitude } },
								 { "Speed" , new[] { (double)location.Speed * 3.6} },
								 { "BatteryLevel" , new[] { Battery.ChargeLevel } }

							 },
				StringValues = new Dictionary<string, string[]>
							 {
								 { "BatteryState" , new[] { Battery.State.ToString() } },
								 { "BatteryPowerSource" , new[] { Battery.PowerSource.ToString() } },
								 { "EnergySaverStatus" , new[] { Battery.EnergySaverStatus.ToString() } },
							 },
				TagValues = new Dictionary<string, string[]>()
							{
								{"version", new string[]{ this.packageInfo.VersionName } },
								{"rider", new string[]{ this.connectionService.Settings.Rider} },
								{"team", new string[]{ this.connectionService.Settings.Team} },
							}

			};

		}

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }
    }
}