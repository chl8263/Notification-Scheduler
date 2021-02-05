using NotiScheduler.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotiScheduler.BusinessLogic {
    interface ScheduleTask {
        void Execute(NotificationSchedulerVO vo);
    }
}
