using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;

namespace FoldersMonitor
{
    public partial class Service1 : ServiceBase
    {
        private Monitor monitor;
        private Timer timer;

        public Service1()
        {
            InitializeComponent();
            monitor = new Monitor();
            SetTimer();
        }

        protected override void OnStart(string[] args)
        {
            Record.Trace("FoldersMonitor服務開始");
            monitor.CheckFile();
            timer.Start();
        }

        protected override void OnStop()
        {
            monitor.StopMonitor();
            timer.Stop();
            timer.AutoReset = true;
            Record.Trace("FoldersMonitor服務結束");
        }

        /// <summary>
        /// 設置定時器
        /// </summary>
        private void SetTimer()
        {
            string interval = ConfigurationManager.AppSettings["CheckFileInterval(Minute)"];
            timer = new Timer()
            {
                Interval = Convert.ToDouble(interval) * 60 * 1000,
                Enabled = true
            };
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        }

        /// <summary>
        /// Timer.Elapsed的事件處理常式-定時比對檔案
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Record.Trace("定時比對檔案...");
            monitor.CheckFile();
        }
    }
}

