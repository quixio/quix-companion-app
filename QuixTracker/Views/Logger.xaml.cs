using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QuixTracker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Logger : ContentPage
    {
        StringBuilder log = new StringBuilder();

        private static Logger instance;

        public Logger()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null) instance = new Logger();
                return instance;
            }
        }


        public string FullLog
        {
            get => log.ToString();
        }

        public void Log(string message)
        {
            log.Append("\n");
            log.Append(message);
            this.OnPropertyChanged("FullLog");
        }

    }
}