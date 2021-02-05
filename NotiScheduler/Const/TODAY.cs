using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotiScheduler.Const {
    class TODAY {

        public static DateTime DETETIME = DateTime.Now;

        public static string SIMPLE_DETE_TIME = DateTime.Now.ToString("yyyy-MM-dd");

        public static string DAYOFWEEK = DateTime.Now.ToString("ddd").ToUpper();

        public static string HOURMIN = DateTime.Now.ToString("HH:mm");

        public static string DEFAULT_CYCLE_TIME = "13:00";

        public static int DATE = Convert.ToInt32(DateTime.Now.ToString("dd"));

        public static string HOUR = DateTime.Now.ToString("HH");

        public static string MIN = DateTime.Now.ToString("mm");

        public static string SEC = DateTime.Now.ToString("ss");
    }
}
