using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Flurl.Util;
using Newtonsoft.Json;
using QuixTracker.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace QuixTracker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : ContentPage
    {
        private string deviceId;

        private string interval = "250";
        private string rider;
        private string team;
        private bool logGForce;
        private string sessionName;
        private ConnectionService connectionService;
        private string workspace;
        private string token;
        private string subdomain;
        private string topic;
        private string notificationsTopic;
        private string bluetoothDevice;
        private List<string> bluetoothDevices;

        private readonly HttpClient client;

        public string Workspace
        {
            get { return workspace; }
            set
            {
                workspace = value;
                this.OnPropertyChanged();
                connectionService.Settings.WorkspaceId = value;
            }
        }

        public string Token
        {
            get { return token; }
            set
            {
                token = value;
                this.OnPropertyChanged();
                connectionService.Settings.Token = value;
            }
        }
        public string Topic
        {
            get { return topic; }
            set
            {
                topic = value;
                this.OnPropertyChanged();
                connectionService.Settings.Topic = value;
            }
        }
        public string NotificationsTopic
        {
            get { return notificationsTopic; }
            set
            {
                notificationsTopic = value;
                this.OnPropertyChanged();
                connectionService.Settings.NotificationsTopic = value;
            }
        }
        public string Subdomain
        {
            get { return subdomain; }
            set
            {
                subdomain = value;
                this.OnPropertyChanged();
                connectionService.Settings.SubDomain = value;
            }
        }

        public List<string> BluetoothDevices {
            get { return bluetoothDevices; }
            set
            {
                bluetoothDevices = value;
                this.OnPropertyChanged();
            }
        }

        public string BluetoothDevice
        {
            get { return bluetoothDevice; }
            set
            {
                bluetoothDevice = value;
                this.OnPropertyChanged();
                connectionService.Settings.BluetoothDevice = value;
            }
        }


        public string SessionName
        {
            get { return sessionName; }
            set
            {
                sessionName = value;
                this.OnPropertyChanged();
                connectionService.Settings.SessionName = value;
            }
        }

        public string DeviceId
        {
            get { return deviceId; }
            set
            {
                deviceId = value;
                this.OnPropertyChanged();
                connectionService.Settings.DeviceId = value;
            }
        }

        public string Rider
        {
            get { return rider; }
            set
            {
                rider = value;
                this.OnPropertyChanged();
                connectionService.Settings.Rider = value;
            }
        }

        public string Team
        {
            get { return team; }
            set
            {
                team = value;
                this.OnPropertyChanged();
                connectionService.Settings.Team = value;
            }
        }

        public string Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                this.OnPropertyChanged();
                connectionService.Settings.Interval = int.Parse(value);
            }
        }

        public bool LogGForce
        {
            get { return logGForce; }
            set
            {
                logGForce = value;
                this.OnPropertyChanged();
                connectionService.Settings.LogGForce = value;
            }
        }

        public Settings()
        {
            InitializeComponent();

            this.connectionService = ConnectionService.Instance;
            this.SessionName = connectionService.Settings.SessionName;
            this.DeviceId = connectionService.Settings.DeviceId;
            this.Rider = connectionService.Settings.Rider;
            this.Team = connectionService.Settings.Team;
            this.LogGForce = connectionService.Settings.LogGForce;
            this.Interval = connectionService.Settings.Interval.ToString();
            
            this.Workspace = connectionService.Settings.WorkspaceId;
            this.Token = connectionService.Settings.Token;
            this.Topic = connectionService.Settings.Topic;
            this.NotificationsTopic = connectionService.Settings.NotificationsTopic;
            this.Subdomain = connectionService.Settings.SubDomain;

            client = new HttpClient();

            BindingContext = this;
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            ZXingScannerPage scanPage = new ZXingScannerPage();
            scanPage.OnScanResult += (result) =>
            {
                scanPage.IsScanning = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();

                    var rtn = result.Text.StripQuotes();
                    
                    GetTokenFromUrl(rtn);
                });
            };
            await Navigation.PushAsync(scanPage);
        }

        private string TryGetDictionaryValue(string key, Dictionary<string, string> dict)
        {
            return dict.TryGetValue(key, out var value) ? value : "";
        }
        
        private void GetTokenFromUrl(string url)
        {
            try
            {
                var response = client.GetAsync(url);
                if (response.Result.IsSuccessStatusCode)
                {
                    string content = response.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var o = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

                    Token = TryGetDictionaryValue("bearerToken", o);
                    Topic = TryGetDictionaryValue("topic", o);
                    Workspace = TryGetDictionaryValue("workspaceId", o);
                    NotificationsTopic = TryGetDictionaryValue("notificationsTopic", o);
                    Subdomain = TryGetDictionaryValue("subdomain", o);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    
    }
}