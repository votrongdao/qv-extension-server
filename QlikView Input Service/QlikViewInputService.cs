using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace QlikView_Input_Service
{
    public partial class QlikViewInputService : ServiceBase
    {
        System.Timers.Timer aTimer = null;

        public QlikViewInputService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Create a timer with a ten second interval.
            aTimer = new System.Timers.Timer(10000);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);

            aTimer.Start();

            GC.KeepAlive(aTimer);



        }

        // Specify what you want to happen when the Elapsed event is 
        // raised.
        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!System.Diagnostics.EventLog.SourceExists("test"))
                    System.Diagnostics.EventLog.CreateEventSource("test", "Application");

            EventLog MyEventLog = new EventLog();
            MyEventLog.Source = "test";
            MyEventLog.WriteEntry("this is a test ...", EventLogEntryType.Warning);
        }

        protected override void OnStop()
        {
        }
    }
}
