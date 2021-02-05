using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotiScheduler.Const {
    class MailString {

        public static string SYSTEM_ISSUE_SUBJECT =
             ConfigurationManager.AppSettings["EmailSubject"] + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        public static string FILE_CREATE_ERROR_SUBJECT = "Directory Create Error";
        public static string FILE_CREATE_ERROR_BODY = "Error occurred duting make directory create, error path like that -> ";
    }
}
