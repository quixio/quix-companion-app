using System;
using System.Diagnostics;
using QuixCompanionApp.Models;

namespace QuixCompanionApp.Services
{

    public class ConnectionService
    {
        private static ConnectionService instance;
        public static ConnectionService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectionService();
                }

                return instance;
            }
        }

        public Models.Settings Settings { get; private set; }

        public event EventHandler<ConnectionState> InputConnectionChanged;
        public event EventHandler<ConnectionState> OutputConnectionChanged;
        public event EventHandler<string> ConnectionError;
        public event EventHandler<CurrentData> DataReceived;
        public event EventHandler<FirmwareUpdate> FirmwareUpdateReceived;
        public event EventHandler<FirmwareUpdate> FirmwareUpdated;

        public ConnectionState OutputConnectionState { get; private set; }

        public void OnFirmwareUpdateReceived(FirmwareUpdate data)
        {
            FirmwareUpdateReceived?.Invoke(this, data);
        }

        public void OnFirmwareUpdated(FirmwareUpdate data)
        {
            FirmwareUpdated?.Invoke(this, data);
        }

        public ConnectionService()
        {
            this.Settings = new Models.Settings();
        }

        public void OnDataReceived(CurrentData data)
        {
            DataReceived?.Invoke(this, data);
        }

        public void OnInputConnectionChanged(ConnectionState newState)
        {
            InputConnectionChanged?.Invoke(this, newState);
        }

        public void OnOutputConnectionChanged(ConnectionState newState)
        {
            LoggingService.Instance.LogInformation(
                $"Output connection changed from {this.OutputConnectionState} to {newState}");

            OutputConnectionChanged?.Invoke(this, newState);
            OutputConnectionState = newState;
        }

        public void OnConnectionError(string message, Exception ex)
        {
            ConnectionError?.Invoke(this, message);
            LoggingService.Instance.LogError(message, ex);
        }

        public void ClearError()
        {
            ConnectionError?.Invoke(this, null);
        }
    }
}