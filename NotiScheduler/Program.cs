using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using NotiScheduler.Bean;
using System.IO;
using NotiScheduler.Helper;
using NotiScheduler.VO;
using NotiScheduler.BusinessLogic.ScheduleTaskImpl;
using NotiScheduler.BusinessLogic;
using NotiScheduler.Const;

namespace NotiScheduler {
    class Program {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main (string[] args) {

            try {

                //new ShipEmail().Execute(new NotificationSchedulerVO());

                // time ->> queue -->>
                var sm = new SchedulerManager();
                var resultQueue = sm.GetValidTaskQueue();
                sm.ExecuteTaskQueue(resultQueue);


                MailHelper.SnedSystemIssueEmail();

                //if (resultQueue.Where(x => x.Cycle != "any").ToList().Count > 0)
                //    MailHelper.SnedSystemIssueEmail();

            } catch (Exception e) {
                log.Error(e);
            }
        }
    }
}
