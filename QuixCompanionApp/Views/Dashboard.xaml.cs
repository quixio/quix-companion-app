using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuixCompanionApp.Models;
using QuixCompanionApp.Services;
using System;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QuixCompanionApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dashboard : ContentPage
    {
        private bool reconnectingWebSocket;
        private bool connected = false;
        private bool reconnecting;
        private string errorMessage;
        private ConnectionService connectionService;
        private QuixWriterService writerService;
        private readonly QuixReaderService readerService;
        private readonly LoggingService loggingService;
        private CurrentData currentData;
        private string speed;
        private string accuracy;
        private string bufferSize;
        private string altitude;
        private string bearing;
        private string message;
        private bool connecting;
        private bool draining;
        private bool disconnected = true;
        private string heartRate;
        private FirmwareUpdate newFirmwareAvailable;
        private string firmware;
        private bool writerStarted = false;
        private string newFirmwareMessage;
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        private readonly Task task;
        private readonly FleetService fleetService;

        private bool isTracking = false;

        #region properties
        public bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                this.OnPropertyChanged();
            }
        }

        public bool Connecting
        {
            get { return connecting; }
            set
            {
                connecting = value;
                this.OnPropertyChanged();
            }
        }

        public bool Disconnected
        {
            get { return disconnected; }
            set
            {
                disconnected = value;
                this.OnPropertyChanged();
            }
        }

        public bool Draining
        {
            get { return draining; }
            set
            {
                draining = value;
                this.OnPropertyChanged();
            }
        }

        public bool Reconnecting
        {
            get { return reconnecting; }
            set
            {
                reconnecting = value;
                this.OnPropertyChanged();
            }
        }

        public bool ReconnectingWebSocket
        {
            get { return reconnectingWebSocket; }
            set
            {
                reconnectingWebSocket = value;
                this.OnPropertyChanged();
            }
        }

        public FirmwareUpdate NewFirmwareAvailable
        {
            get { return newFirmwareAvailable; }
            set
            {
                newFirmwareAvailable = value;
                this.OnPropertyChanged();
            }
        }

        public string NewFirmwareMessage
        {
            get { return newFirmwareMessage; }
            set
            {
                newFirmwareMessage = value;
                this.OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                this.OnPropertyChanged();
            }
        }
        public string Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                this.OnPropertyChanged();
            }
        }

        public string Accuracy
        {
            get { return accuracy; }
            set
            {
                accuracy = value;
                this.OnPropertyChanged();
            }
        }

        public string BufferSize
        {
            get { return bufferSize; }
            set
            {
                bufferSize = value;
                this.OnPropertyChanged();
            }
        }

        public string Altitude
        {
            get { return altitude; }
            set
            {
                altitude = value;
                this.OnPropertyChanged();
            }
        }

        public string Bearing
        {
            get { return bearing; }
            set
            {
                bearing = value;
                this.OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                this.OnPropertyChanged();
            }
        }

        public string HeartRate
        {
            get { return heartRate; }
            set
            {
                heartRate = value;
                this.OnPropertyChanged();
            }
        }

        public string Firmware
        {
            get { return firmware; }
            set
            {
                firmware = value;
                this.OnPropertyChanged();
            }
        }
#endregion

        public Dashboard()
        {

            InitializeComponent();

            BindingContext = this;

            this.connectionService = ConnectionService.Instance;
            this.connectionService.FirmwareUpdateReceived += ConnectionService_FirmwareUpdateReceived;

            this.Firmware = this.connectionService.Settings.Firmware;

            this.writerService = new QuixWriterService(this.connectionService);
            this.readerService = new QuixReaderService(this.connectionService);
            this.loggingService = LoggingService.Instance;
            this.fleetService = new FleetService();

            this.task = new Task(InitializeQuixServices);
            this.task.Start();
        }

        private async void InitializeQuixServices()
        {
            try
            {
                await this.readerService.StartConnection();
                await this.writerService.StartConnection();
            }
            catch (Exception ex)
            {
                this.loggingService.LogError("Failed to start connection", ex);
            }

            try
            {
                await this.readerService.SubscribeToEvent(this.connectionService.Settings.DeviceId, "notification");
                await this.readerService.SubscribeToEvent(this.connectionService.Settings.DeviceId, "FirmwareUpdate");
            }
            catch (Exception ex)
            {
                this.loggingService.LogError("Failed to subscribe to notifications", ex);
            }

            this.readerService.EventDataRecieved += QuixService_EventDataRecieved;
            await this.fleetService.CheckFirmwareUpdates();
        }

        private void QuixService_EventDataRecieved(object sender, EventDataDTO e)
        {
            if (e.Id == "FirmwareUpdate")
            {
                var firmwareUpdate = JsonSerializer.Deserialize<FirmwareUpdate>(e.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                this.connectionService.OnFirmwareUpdateReceived(firmwareUpdate);
            }
        }

        private void ConnectionService_FirmwareUpdateReceived(object sender, FirmwareUpdate e)
        {
            this.NewFirmwareAvailable = e;
            if (e.Notification != null)
            {
                if (!string.IsNullOrEmpty(e.Notification.Title) && !string.IsNullOrEmpty(e.Notification.Text))
                    this.NewFirmwareMessage = $"{e.Notification.Title}: {e.Notification.Text}";
                else if (!string.IsNullOrEmpty(e.Notification.Title))
                    this.NewFirmwareMessage = $"{e.Notification.Title}";
                else if (!string.IsNullOrEmpty(e.Notification.Text))
                    this.NewFirmwareMessage = $"{e.Notification.Title}";
                else
                    this.NewFirmwareMessage = $"New firmware available: {e.Version}";
            }
            else
            {
                this.NewFirmwareMessage = $"New firmware available: {e.Version}";
            }
        }

        private void ConnectionService_DataReceived(object sender, CurrentData e)
        {
            this.Speed = ((int)(e.Speed * 3.6)).ToString();
            this.Accuracy = ((int)e.Accuracy).ToString();
            this.BufferSize = e.LocationBufferSize.ToString();
            this.Bearing = e.Bearing.ToString("0.00");
            this.Altitude = e.Altitude.ToString("0.00");
            this.Message = e.Message;
            this.HeartRate = e.Heartrate.ToString();
        }

        private void ConnectionService_ConnectionError(object sender, string e)
        {
            this.ErrorMessage = e;
        }

        private void QuixService_ConnectionError(object sender, string e)
        {
            this.ErrorMessage = e;
        }

        private void ConnectionService_OutputConnectionChanged(object sender, ConnectionState e)
        {
            switch (e)
            {
                case ConnectionState.Connected:
                    this.Reconnecting = false;
                    this.Connected = true;
                    this.Connecting = false;
                    this.Draining = false;
                    this.Disconnected = false;
                    break;
                case ConnectionState.Connecting:
                    this.Reconnecting = false;
                    this.Connected = false;
                    this.Connecting = true;
                    this.Draining = false;
                    this.Disconnected = false;
                    break;
                case ConnectionState.Reconnecting:
                    this.Reconnecting = true;
                    this.Connected = false;
                    this.Connecting = false;
                    this.Draining = false;
                    this.Disconnected = false;
                    break;
                case ConnectionState.Disconnected:
                    this.Connected = false;
                    this.Reconnecting = false;
                    this.Connecting = false;
                    this.Draining = false;
                    this.Disconnected = true;
                    if (!this.isTracking)
                    {
                        this.connectionService.OutputConnectionChanged -= ConnectionService_OutputConnectionChanged;
                        this.connectionService.ConnectionError -= ConnectionService_ConnectionError;
                        this.connectionService.DataReceived -= ConnectionService_DataReceived;
                    }
                    break;
                case ConnectionState.Draining:
                    this.Connected = false;
                    this.Reconnecting = false;
                    this.Connecting = false;
                    this.Draining = true;
                    this.Disconnected = false;
                    break;
            }
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            this.isTracking = true;

            this.connectionService.OutputConnectionChanged += ConnectionService_OutputConnectionChanged;
            this.connectionService.ConnectionError += ConnectionService_ConnectionError;
            this.connectionService.DataReceived += ConnectionService_DataReceived;

            this.ConnectionService_OutputConnectionChanged(this, this.connectionService.OutputConnectionState);

            DependencyService.Get<IStartService>().StartForegroundServiceCompat();
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            this.isTracking = false;
            DependencyService.Get<IStartService>().StopForegroundServiceCompat();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
        }

        async void Update_Clicked(System.Object sender, System.EventArgs e)
        {
            var ev = this.NewFirmwareAvailable;
            var campaignId = this.NewFirmwareAvailable.CampaignId;
            this.connectionService.Settings.Firmware = this.NewFirmwareAvailable.Version;
            this.Firmware = this.NewFirmwareAvailable.Version;
            this.NewFirmwareAvailable = null;

            var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000;

            var payload = new EventDataDTO[] 
            {
                new EventDataDTO
                {
                    Id = "FirmwareUpdated",
                    Timestamp = timestamp,
                    Value = JsonConvert.SerializeObject(new FirmwareUpdatedDTO
                    {
                        BikeId = this.connectionService.Settings.DeviceId,
                        CampaignId = campaignId,
                        Version = this.Firmware,
                        Status = "Success"
                    }, this.jsonSettings)
                }
            };

            if (!writerStarted)
            {
                try
                {
                    await this.writerService.StartConnection();
                    writerStarted= true;
                } catch (Exception ex)
                {
                    LoggingService.Instance.LogError("Failed to start writer service", ex);
                }
            }
            await this.writerService.SendEventData("default", payload);

            this.connectionService.OnFirmwareUpdated(ev);
        }
    }
}