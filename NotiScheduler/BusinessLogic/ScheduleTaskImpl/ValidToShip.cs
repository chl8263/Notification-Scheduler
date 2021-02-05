using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using NotiScheduler.Bean;
using System.IO;
using NotiScheduler.Helper;
using NotiScheduler.VO;
using System.Reflection;
using NotiScheduler.Const;
using System.Threading;
using static NotiScheduler.MailHelper;

namespace NotiScheduler.BusinessLogic.ScheduleTaskImpl {
    class ValidToShip : ScheduleTask {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private readonly string FILE_SHOT_NAME = "ValidToShip";

        private SAPDAO sd = null;

        public void Execute(NotificationSchedulerVO vo) {
            sd = new SAPDAO("CA");

            try {
                var list = sd.GetValidToShipData();
                var fileHelper = new FileHelper<ValidToShipVO>();
                var stageFileInfo = fileHelper.CreateCvsFileAtStage(list, FILE_SHOT_NAME);

                MailHelper.Sendmail(vo, stageFileInfo);
                fileHelper.moveCompleteDirectory(stageFileInfo);

            } catch (Exception e) {
                log.Error(e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                sd.closeConnection();
            };
        }
    }
}
