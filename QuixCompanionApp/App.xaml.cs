using System;
using System.Threading.Tasks;
using QuixCompanionApp.Services;
using Xamarin.Forms;

namespace QuixCompanionApp
{
    public partial class App : Application
    {
        private LoggingService loggingService;

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            this.loggingService = LoggingService.Instance;

        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            this.loggingService.LogError("TaskSchedulerOnUnobservedTaskException", e.Exception);

        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.loggingService.LogError(e.ToString());

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}