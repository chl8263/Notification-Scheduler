using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotiScheduler.Bean {
    class NotificationSchedulerVO {
        public int Id{ get; set; }
        public string Department { get; set; }
        public string Cycle { get; set; }
        public int CycleDate { get; set; }
        public string CycleDay { get; set; }
        public string CycleTime { get; set; }
        public string EmailServer { get; set; }
        public string EmailTitle { get; set; }
        public string EmailContent { get; set; }
        public string ReceiverEmail { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string CcEmail { get; set; }
        public string BccEmail { get; set; }
        public string ProgramClass { get; set; }
        public DateTime FirstRunDate { get; set; }
        public DateTime LastRunDate { get; set; }
        public string CreateDate { get; set; }
        public string ModifiedDate { get; set; }
        public int Active { get; set; }
        public int EmailOnlyDataFlag { get; set; }

    }
}
