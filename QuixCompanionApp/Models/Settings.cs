using System;
using Xamarin.Essentials;

namespace QuixCompanionApp.Models
{
    public class Settings
    {
        private string deviceId;

        private int interval = 250;
        private string rider;
        private string team;
        private bool logGForce;
        private string sessionName;
        private string workspaceId;
        private string subDomain;
        private string token;
        private string topic;
        private string notificationsTopic;
        private string workspace;
        private string bluetoothDevice;
        private string firmware;

        public Settings()
        {
            this.SessionName = Preferences.Get("SessionName", $"");
            this.DeviceId = Preferences.Get("DeviceId", "My Device");
            this.Rider = Preferences.Get("Rider", "My Name");
            this.Team = Preferences.Get("Team", "My Team");
            this.Interval = Preferences.Get("Interval", 1000);
            this.LogGForce = Preferences.Get("LogGForce", true);

            this.WorkspaceId = Preferences.Get("Workspace", "");
            this.Token = Preferences.Get("Token", "");
            this.Topic = Preferences.Get("Topic", "phone-data");
            this.NotificationsTopic = Preferences.Get("NotificationsTopic", "phone-out");
            this.SubDomain = Preferences.Get("SubDomain", "platform");

            this.Firmware = Preferences.Get("Firmware", "1.0.0.0");
        }

        public string SubDomain
        {
            get { return subDomain; }
            set
            {
                subDomain = value;
                Preferences.Set("SubDomain", value);
            }
        }

        public string Topic
        {
            get { return topic; }
            set
            {
                topic = value;
                Preferences.Set("Topic", value);
            }
        }

        public string NotificationsTopic
        {
            get { return notificationsTopic; }
            set
            {
                notificationsTopic = value;
                Preferences.Set("NotificationsTopic", value);

            }
        }

        public string WorkspaceId
        {
            get { return workspace; }
            set
            {
                workspace = value;
                Preferences.Set("Workspace", value);
            }
        }

        public string Token
        {
            get { return token; }
            set
            {
                token = value;
                Preferences.Set("Token", value);
            }
        }

        public string BluetoothDevice
        {
            get { return bluetoothDevice; }
            set
            {
                bluetoothDevice = value;
                Preferences.Set("BluetoothDevice", value);
            }
        }


        public string SessionName
        {
            get { return sessionName; }
            set
            {
                sessionName = value;
                Preferences.Set("SessionName", value);
            }
        }

        public string DeviceId
        {
            get { return deviceId; }
            set
            {
                deviceId = value;
                Preferences.Set("DeviceId", value);
            }
        }

        public string Rider
        {
            get { return rider; }
            set
            {
                rider = value;
                Preferences.Set("Rider", value);
            }
        }

        public string Team
        {
            get { return team; }
            set
            {
                team = value;
                Preferences.Set("Team", value);
            }
        }

        public string Firmware
        {
            get { return firmware; }
            set
            {
                firmware = value;
                Preferences.Set("Firmware", value);
            }
        }

        public int Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                Preferences.Set("Interval", value);
            }
        }

        public bool LogGForce
        {
            get { return logGForce; }
            set
            {
                logGForce = value;
                Preferences.Set("logGForce", value);
            }
        }

        public string CheckSettings()
        {
            if (string.IsNullOrWhiteSpace(this.WorkspaceId)) return "WorkspaceId is empty";
            if (string.IsNullOrWhiteSpace(this.Token)) return "Token is empty";
            if (string.IsNullOrWhiteSpace(this.Topic)) return "Topic is empty";
            if (string.IsNullOrWhiteSpace(this.NotificationsTopic)) return "NotificationTopic is empty";
            if (string.IsNullOrWhiteSpace(this.SubDomain)) return "Subdomain is empty";
            if (string.IsNullOrWhiteSpace(this.Rider)) return "Rider is empty";
            if (string.IsNullOrWhiteSpace(this.DeviceId)) return "Device id is empty";
            return "";
        }
    }
}