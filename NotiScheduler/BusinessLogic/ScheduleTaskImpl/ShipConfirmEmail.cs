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
    class ShipConfirmEmail : ScheduleTask {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        string ShipConfirmEmailUS = System.Configuration.ConfigurationManager.AppSettings["ShipConfirmEmailUS"];
        string ShipConfirmEmailCA = System.Configuration.ConfigurationManager.AppSettings["ShipConfirmEmailCA"];

        private SAPDAO sdUS = null;
        private SAPDAO sdCA = null;
        public void Execute(NotificationSchedulerVO vo) {
            try {
                if (ShipConfirmEmailUS == "Y") {
                    sdUS = new SAPDAO("US");
                    var shipmentInfoList_US = sdUS.GetShipmentInfoData();
                    shipmentInfoList_US.ForEach(x => {
                        MailHelper.SendShipEmailToCustomer(x);
                        sdUS.UpdateIsShipped(x.DocEntry);
                    });
                }

                if (ShipConfirmEmailCA == "Y") {
                    sdCA = new SAPDAO("CA");
                    var shipmentInfoList_CA = sdCA.GetShipmentInfoData();
                    shipmentInfoList_CA.ForEach(x => {
                        MailHelper.SendShipEmailToCustomer(x);
                        sdCA.UpdateIsShipped(x.DocEntry);
                    });
                }

            } catch (Exception e) {
                log.Error(e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                sdUS.closeConnection();
                sdCA.closeConnection();
            };
        }
    }
}
