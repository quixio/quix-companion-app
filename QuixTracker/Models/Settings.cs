using System;
using Xamarin.Essentials;

namespace QuixTracker.Models
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

        public Settings()
        {
            this.SessionName = Preferences.Get("SessionName", $"Session on {DateTime.Now:yyyy-M-dd h:mm:ss}");
            this.DeviceId = Preferences.Get("DeviceId", "My Device");
            this.Rider = Preferences.Get("Rider", "My Name");
            this.Team = Preferences.Get("Team", "My Team");
            this.Interval = Preferences.Get("Interval", 250);
            this.LogGForce = Preferences.Get("LogGForce", true);
            this.WorkspaceId = Preferences.Get("Workspace", "");
            this.Token = Preferences.Get("Token", "");

            // these will be populated by Quix when you save the project to your workspace.
            this.workspaceId = "quix-iotdemo";
            this.subDomain = "platform";
            this.token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6Ik1qVTBRVE01TmtJNVJqSTNOVEpFUlVSRFF6WXdRVFF4TjBSRk56SkNNekpFUWpBNFFqazBSUSJ9.eyJodHRwczovL3F1aXguYWkvb3JnX2lkIjoicXVpeCIsImh0dHBzOi8vcXVpeC5haS9vd25lcl9pZCI6ImF1dGgwfGJjZmZmYTYyLTE1YTQtNDllYy05OWE0LWQ1Mjk5OTBkN2I1MSIsImh0dHBzOi8vcXVpeC5haS90b2tlbl9pZCI6IjcxOTIxNDg0LTcxMDQtNDkzZS1hYzk3LTk5NTIyYzk1YjlkYSIsImh0dHBzOi8vcXVpeC5haS9leHAiOiIxNzAzMTEzMjAwIiwiaXNzIjoiaHR0cHM6Ly9hdXRoLnF1aXguYWkvIiwic3ViIjoiMVZXVUtKUEFaaUFCWVNwbFg1WXAxb2dtTTFoazlLMDhAY2xpZW50cyIsImF1ZCI6InF1aXgiLCJpYXQiOjE2NDg3MTkyMjAsImV4cCI6MTY1MTMxMTIyMCwiYXpwIjoiMVZXVUtKUEFaaUFCWVNwbFg1WXAxb2dtTTFoazlLMDgiLCJndHkiOiJjbGllbnQtY3JlZGVudGlhbHMiLCJwZXJtaXNzaW9ucyI6W119.h0n2xE768f6R871r_S1GFZnAG_PIpINiFRn3qK-drHWl9gpUV8REzosU3HlMZx8BJGUBNqL2I-FVncdIf49f32GJOEO6RQ9YSXTDHEcbYBTFE2fXiUMWpPFd6iY-GLfMfY1WJt4XLq9ixw0dN1Nc360cW8vtXoXivnqfpIfjKaB2JYodC4GRY_WPunwjq85mjAdME0nxb8fKjDzuDH_MpfPfuWR4XJaNZ2qcUWkJM3I0i-Svb4cbzd9rrNpUQE4x2MwX5hInMIPblZ8OInvNCN7enPmsAliqfL_w2h-x_r554byIb8T1yjQPfqtTThMB5I66YGXRY4vsUKeAI87Ihw";
            this.topic = "phone-data";
            this.notificationsTopic = "phone-out";

            // debug values
            // this.token = "";
            // this.workspaceId = "";
            // this.subDomain = "dev";
            // this.topic = "phone";
        }

        public string SubDomain => subDomain;
        public string Topic => topic;
        public string NotificationsTopic => notificationsTopic;

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
    }
}