using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QuixTracker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Logger : ContentPage
    {
        private readonly int maxCount = 100;
        private readonly LinkedList<int> lengths = new LinkedList<int>();
        private readonly object mutex = new object();

        private int count = 0;
        StringBuilder log = new StringBuilder();

        private static Logger instance;
        private static readonly int LFLength = Environment.NewLine.Length;

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
            get => GetFullLog();
        }

        public void Log(string message)
        {
            lock (mutex)
            {
                // app becomes unresponsive if the log size gets out of hand.
                if (count > maxCount)
                {
                    this.log.Remove(0, this.lengths.First());
                    this.lengths.RemoveFirst();
                }
                log.AppendLine(message);
                this.lengths.AddLast(message.Length + LFLength);
                count++;
            }
            this.OnPropertyChanged("FullLog");
        }

        private string GetFullLog()
        {
            lock (mutex)
            {
                return log.ToString();
            }
        }

    }
}