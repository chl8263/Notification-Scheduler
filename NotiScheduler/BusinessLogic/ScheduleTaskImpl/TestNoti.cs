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
    class TestNoti : ScheduleTask {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private readonly string FILE_SHOT_NAME = "Test noti";

        private SAPDAO sd = null;

        public void Execute(NotificationSchedulerVO vo) {
            sd = new SAPDAO("CA");

            try {
                var list = sd.getTestNoti();
                var fileHelper = new FileHelper<TestNotiVO>();
                var stageFileInfo = fileHelper.CreateCvsFileAtStage(list, FILE_SHOT_NAME);

                MailHelper.Sendmail(vo, stageFileInfo);
                //MailHelper.SendmailAdmin("test", "test", MailHelper.SERVERTYPE_INTERNAL, Path.Combine(stageFileInfo.path, stageFileInfo.fileName));
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
