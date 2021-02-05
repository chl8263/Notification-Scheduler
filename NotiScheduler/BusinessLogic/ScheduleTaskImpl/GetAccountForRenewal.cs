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

    public enum MonthPeroid {
        CURRENT_MONTH,
        LAST_MONTH,
        NEXT_MONTH
    }
    
    class GetAccountForRenewal : ScheduleTask {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private readonly string FILE_SHOT_NAME_CURRENT_MONTH = "CurrentMonthAccountsForRenewal";
        private readonly string FILE_SHOT_NAME_LAST_MONTH = "LastMonthAccountsForRenewal";
        private readonly string FILE_SHOT_NAME_NEXT_MONTH = "NextMonthAccountsForRenewal";

        private SAPDAO sd = null;

        public void Execute(NotificationSchedulerVO vo) {
            sd = new SAPDAO("US");

            try {
                var currentList = sd.GetAccountForRenewalData(MonthPeroid.CURRENT_MONTH);
                var currentFileHelper = new FileHelper<AccountForRenewalVO>();
                var currentStageFileInfo = currentFileHelper.CreateCvsFileAtStage(currentList, FILE_SHOT_NAME_CURRENT_MONTH);

                var lastList = sd.GetAccountForRenewalData(MonthPeroid.LAST_MONTH);
                var lastFileHelper = new FileHelper<AccountForRenewalVO>();
                var lastStageFileInfo = currentFileHelper.CreateCvsFileAtStage(lastList, FILE_SHOT_NAME_LAST_MONTH);

                var nextList = sd.GetAccountForRenewalData(MonthPeroid.NEXT_MONTH);
                var nextFileHelper = new FileHelper<AccountForRenewalVO>();
                var nextStageFileInfo = currentFileHelper.CreateCvsFileAtStage(nextList, FILE_SHOT_NAME_NEXT_MONTH);

                var fileList = new List<CreatedFileInfoVO> {
                    currentStageFileInfo,
                    lastStageFileInfo,
                    nextStageFileInfo
                };

                MailHelper.Sendmail(vo, fileList);

                currentFileHelper.moveCompleteDirectory(currentStageFileInfo);
                currentFileHelper.moveCompleteDirectory(lastStageFileInfo);
                currentFileHelper.moveCompleteDirectory(nextStageFileInfo);


            } catch (Exception e) {
                log.Error(e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                sd.closeConnection();
            };
        }
    }
}
