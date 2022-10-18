using Android.Content;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using QuixTracker.Services;
using Android.Bluetooth;
using QuixTracker.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuixTracker.Droid
{
    public class HeartRateDiscovery : IDisposable
    {
        private readonly BluetoothAdapter btAdapter;
        private readonly Context context;
        private readonly ConnectionService connectionService;
        private readonly CurrentData currentData;
        private readonly BlockingCollection<ParameterDataDTO> locationQueue;
        private readonly CancellationToken cancellationToken;
        private BluetoothGatt gatt;
        private bool connecting;

        public HeartRateDiscovery(BluetoothAdapter btAdapter, Context context, ConnectionService connectionService, CurrentData currentData, BlockingCollection<ParameterDataDTO> locationQueue, CancellationToken cancellationToken)
        {
            this.btAdapter = btAdapter;
            this.context = context;
            this.connectionService = connectionService;
            this.currentData = currentData;
            this.locationQueue = locationQueue;
            this.cancellationToken = cancellationToken;
        }

        public void Connect()
        {

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {

                        var devices = this.btAdapter.BondedDevices.ToList();


                        //var heartRateDevice = devices.FirstOrDefault(a => a.Name == "808S 0040986"); // Tomas HR
                        var heartRateDevice = devices.FirstOrDefault(a => a.Name == "808S 0008720"); // Javi HR
                        //var heartRateDevice = devices.FirstOrDefault(a => a.Name == "808S 0026070"); // Clara HR

                        this.locationQueue.Add(new ParameterDataDTO
                        {
                            Timestamps = new long[] { (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000 },
                            StringValues = new Dictionary<string, string[]>
                                {
                                    { "LogInfo", new string[] {"Devices loaded" } }
                                }
                        });

                        if (heartRateDevice != null)
                        {
                            this.locationQueue.Add(new ParameterDataDTO
                            {
                                Timestamps = new long[] { (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000 },
                                StringValues = new Dictionary<string, string[]>
                                {
                                    { "LogInfo", new string[] {"Device founded" } }
                                }
                            });


                            if (!connecting)
                            {
                                var callback = new HeartrateStrapCallback(this.locationQueue, this.currentData, this.connectionService, this.cancellationToken);
                                this.gatt = heartRateDevice.ConnectGatt(this.context, false, callback);
                                this.gatt.Connect();

                                var connectionResult = callback.ConnectionTask.Result;

                                if (connectionResult == GattStatus.Success)
                                {
                                    this.connecting = true;


                                    this.locationQueue.Add(new ParameterDataDTO
                                    {
                                        Timestamps = new long[] { (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000 },
                                        StringValues = new Dictionary<string, string[]>
                                {
                                    { "LogInfo", new string[] {"Device connected" } }
                                }
                                    });
                                    break;
                                }
                               
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        this.locationQueue.Add(new ParameterDataDTO
                        {
                            Timestamps = new long[] { (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000 },
                            StringValues = new Dictionary<string, string[]>
                                {
                                    { "LogInfo", new string[] {ex.ToString() } }
                                }
                        });
                    }
                    finally
                    {
                        Task.Delay(5000).Wait();
                    }

                }

            });


        }

        public void Dispose()
        {
            this.gatt.Dispose();
        }
    }
}