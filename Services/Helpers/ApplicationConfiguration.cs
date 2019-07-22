using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Helpers
{
    public class ApplicationConfiguration
    {
        public static TimeSpan AlphaWorkerPeriod
        {
            //get { return TimeSpan.Parse(ConfigurationManager.AppSettings["AlphaWorkerPeriod"]); }
            get { return TimeSpan.FromSeconds(15); }
        }


        // public static string SomeKey => ConfigurationManager.AppSettings["SomeKey"];
        // public static string SomeKey => "SomeKey";
    }
}
