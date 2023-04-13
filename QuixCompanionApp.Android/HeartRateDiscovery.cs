using Android.Content;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using QuixCompanionApp.Services;
using Android.Bluetooth;
using QuixCompanionApp.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuixCompanionApp.Droid
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

                        LoggingService.Instance.LogInformation("Devices loaded");

                        if (heartRateDevice != null)
                        {
                            LoggingService.Instance.LogInformation("Device found");


                            if (!connecting)
                            {
                                var callback = new HeartrateStrapCallback(this.locationQueue, this.currentData, this.connectionService, this.cancellationToken);
                                this.gatt = heartRateDevice.ConnectGatt(this.context, false, callback);
                                this.gatt.Connect();

                                var connectionResult = callback.ConnectionTask.Result;

                                if (connectionResult == GattStatus.Success)
                                {
                                    this.connecting = true;

                                    LoggingService.Instance.LogInformation("Device connected");

                                    break;
                                }
                               
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        LoggingService.Instance.LogError("Bluetooth discovery failed.", ex);
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