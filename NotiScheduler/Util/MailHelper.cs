using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using log4net;
using System.Net.Mime;
using System.IO;
using NotiScheduler.VO;
using System.Net;
using NotiScheduler.Bean;
using NotiScheduler.Const;
using NotiScheduler.Util;

namespace NotiScheduler {

    class MailHelper {

        protected static readonly ILog log = LogManager.GetLogger(typeof(MailHelper));

        private const string mailBodyHeader = @"<div style=""font-family:Calibri,Tahoma,Verdana,Arial; font-size:16px;\"">";
        private const string mailBodyEnd = "</div>";
        private const string mailBodyFileInfoHeader = "<br><br>&lt;Attachment information&gt;";
        private const string mailBodyFileInfoFooter = "<br><br>Thank you<br>";

        public const string SERVERTYPE_INTERNAL = "INTERNAL";
        public const string SERVERTYPE_EXTERNAL = "EXTERNAL";

        public static List<SysErrorMsgVO> systemInfoEmailList = new List<SysErrorMsgVO>();

        public static void SendmailAdmin(String subject, String mailMsg, string serverType = SERVERTYPE_INTERNAL, CreatedFileInfoVO file = null) {
            if (serverType == SERVERTYPE_INTERNAL) {
                if (file == null) MailHelper.SendmailInternalAdmin(subject, mailMsg);
                else {
                    var fileList = new List<CreatedFileInfoVO>();
                    fileList.Add(file);
                    MailHelper.SendmailAdmin(subject, mailMsg, serverType, fileList);
                }
            } else {
                if (file == null) MailHelper.SendmailExternalAdmin(subject, mailMsg);
                else {
                    var fileList = new List<CreatedFileInfoVO>();
                    fileList.Add(file);
                    MailHelper.SendmailAdmin(subject, mailMsg, serverType, fileList);
                }
            }
        }

        public static void SendmailAdmin(string subject, string mailMsg, string serverType, List<CreatedFileInfoVO> fileList) {
            if (serverType == SERVERTYPE_INTERNAL) {
                if (fileList == null) MailHelper.SendmailInternalAdmin(subject, mailMsg);
                else MailHelper.SendmailInternalAdmin(subject, mailMsg, fileList);
            } else {
                if (fileList == null) MailHelper.SendmailExternalAdmin(subject, mailMsg);
                else MailHelper.SendmailExternalAdmin(subject, mailMsg, fileList);
            }
        }

        public static void SendmailInternalAdmin(String subject, String mailMsg, List<CreatedFileInfoVO> fileList = null) {
            var senderName = System.Configuration.ConfigurationManager.AppSettings["SenderName"];
            var senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"];
            var recipientsEmail = System.Configuration.ConfigurationManager.AppSettings["RecipientsEmail"];
            var ccRecipientsEmail = System.Configuration.ConfigurationManager.AppSettings["CcRecipientsEmail"];
            var bccRecipientsEmail = System.Configuration.ConfigurationManager.AppSettings["BccRecipientsEmail"];
            var mailServer = System.Configuration.ConfigurationManager.AppSettings["MailServer"];

            //For test
            //var senderName = "Ewan";
            //var senderEmail = "wchoi@univera.com";
            //var recipientsEmail = "wchoi@univera.com";
            //var ccRecipientsEmail = "";
            //var bccRecipientsEmail = "";


            var mail = new MailMessage();
            mail.From = new MailAddress(senderEmail, senderName);
            //mail.To.Add(new System.Net.Mail.MailAddress("json@univera.com"));
            mail.To.Add(recipientsEmail);

            if (ccRecipientsEmail != null && !"".Equals(ccRecipientsEmail)) {
                mail.CC.Add(ccRecipientsEmail);
            }

            if (bccRecipientsEmail != null && !"".Equals(bccRecipientsEmail)) {
                mail.Bcc.Add(bccRecipientsEmail);
            }

            var attachedFileInfo = new StringBuilder();
            if (fileList != null) {

                var filterFileList = fileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList();

                if (filterFileList.Count() > 0) {
                    attachedFileInfo.Append(mailBodyFileInfoHeader);
                    filterFileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList().ForEach(x => {
                        attachedFileInfo.Append("<br><br>");
                        attachedFileInfo.Append($"&nbsp; - File name : {x.fileName}");
                        attachedFileInfo.Append("<br>");
                        attachedFileInfo.Append($"&nbsp; - Data count : {x.dataCount}");

                        var filePath = Path.Combine(x.filePath, x.fileName);
                        var attachment = new Attachment(filePath);

                        mail.Attachments.Add(attachment);
                    });
                    attachedFileInfo.Append(mailBodyFileInfoFooter);
                }
            }

            mail.Subject = subject;
            mail.Body = mailBodyHeader;
            mail.Body += mailMsg + attachedFileInfo.ToString();
            mail.Body += mailBodyEnd;
            mail.IsBodyHtml = true;

            try {
                var Client = new SmtpClient(mailServer);
                Client.Send(mail);
                Client.Dispose();
            } catch (SmtpException se) {
                Console.WriteLine("####################################################");
                Console.WriteLine("Exception caught when sendding email ");
                Console.WriteLine(se);
                Console.WriteLine("####################################################");
                log.Error("Exception caught when sendding email : {0}", se);
            }
        }

        public static void SendmailExternalAdmin(String subject, String mailMsg, List<CreatedFileInfoVO> fileList = null) {
            var emailServer = System.Configuration.ConfigurationManager.AppSettings["AWSEmailServer"];
            var emailUserName = System.Configuration.ConfigurationManager.AppSettings["AWSEmailUserName"];
            var emailPassword = System.Configuration.ConfigurationManager.AppSettings["AWSEmailPassword"];

            var customerSenderName = System.Configuration.ConfigurationManager.AppSettings["CustomerSenderName"];
            var customerSenderEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerSenderEmail"];
            var customerRecipientsName = System.Configuration.ConfigurationManager.AppSettings["CustomerRecipientsEmail"];
            var customerRecipientsEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerRecipientsEmail"];
            var customerCcEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerCcEmail"];
            var customerBccEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerBccEmail"];

            var mail = new MailMessage();
            mail.From = new MailAddress(customerSenderEmail, customerSenderName);
            
            if (customerRecipientsEmail != null) {
                mail.To.Add(new MailAddress(customerRecipientsName, customerRecipientsEmail));
            } else {
                mail.To.Add(customerRecipientsEmail);
            }
            if (customerCcEmail != null && !"".Equals(customerCcEmail)) {
                mail.CC.Add(customerCcEmail);
            }
            if (customerBccEmail != null && !"".Equals(customerBccEmail)) {
                mail.Bcc.Add(customerBccEmail);
            }

            var attachedFileInfo = new StringBuilder();
            if (fileList != null) {

                var filterFileList = fileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList();

                if (filterFileList.Count() > 0) {
                    attachedFileInfo.Append(mailBodyFileInfoHeader);
                    filterFileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList().ForEach(x => {
                        attachedFileInfo.Append("<br><br>");
                        attachedFileInfo.Append($"&nbsp; - File name : {x.fileName}");
                        attachedFileInfo.Append("<br>");
                        attachedFileInfo.Append($"&nbsp; - Data count : {x.dataCount}");

                        var filePath = Path.Combine(x.filePath, x.fileName);
                        var attachment = new Attachment(filePath);

                        mail.Attachments.Add(attachment);
                    });
                    attachedFileInfo.Append(mailBodyFileInfoFooter);
                }
            }

            mail.Subject = subject;
            mail.Body = mailBodyHeader;
            mail.Body += mailMsg + attachedFileInfo.ToString();
            mail.Body += mailBodyEnd;
            mail.IsBodyHtml = true;

            try {
                var Client = new SmtpClient(emailServer);
                Client.Port = 587;
                Client.EnableSsl = true;
                //Client.UseDefaultCredentials = true;
                Client.Credentials = new NetworkCredential(emailUserName, emailPassword);
                Client.Send(mail);
                Client.Dispose();
            } catch (SmtpException se) {
                log.Error("###### Exception caught when sendding email : {0}", se);
            }
        }

        public static void Sendmail(NotificationSchedulerVO vo, CreatedFileInfoVO file = null) {
            if (vo.EmailServer == SERVERTYPE_INTERNAL) {
                if (file == null) MailHelper.SendmailInternal(vo);
                else {
                    var fileList = new List<CreatedFileInfoVO>();
                    fileList.Add(file);
                    MailHelper.Sendmail(vo, fileList);
                }
            } else {
                if (file == null) MailHelper.SendmailExternal(vo);
                else {
                    var fileList = new List<CreatedFileInfoVO>();
                    fileList.Add(file);
                    MailHelper.Sendmail(vo, fileList);
                }
            }
        }

        public static void Sendmail(NotificationSchedulerVO vo, List<CreatedFileInfoVO> fileList) {
            if (vo.EmailServer == SERVERTYPE_INTERNAL) {
                if (fileList == null) MailHelper.SendmailInternal(vo);
                else MailHelper.SendmailInternal(vo, fileList);
            } else {
                if (fileList == null) MailHelper.SendmailExternal(vo);
                else MailHelper.SendmailExternal(vo, fileList);
            }
        }

        public static void SendmailInternal(NotificationSchedulerVO vo, List<CreatedFileInfoVO> fileList = null) {

            var senderName = vo.SenderName;
            var senderEmail = vo.SenderEmail;
            var recipientsEmail = vo.ReceiverEmail;
            var ccRecipientsEmail = vo.CcEmail;
            var bccRecipientsEmail = vo.BccEmail;
            var mailServer = System.Configuration.ConfigurationManager.AppSettings["MailServer"];
            var mail = new MailMessage();
            mail.From = new MailAddress(senderEmail, senderName);
            //mail.To.Add(new System.Net.Mail.MailAddress("json@univera.com"));
            mail.To.Add(recipientsEmail);
            
            if (ccRecipientsEmail != null && !"".Equals(ccRecipientsEmail)) {
                mail.CC.Add(ccRecipientsEmail);
            }

            if (bccRecipientsEmail != null && !"".Equals(bccRecipientsEmail)) {
                mail.Bcc.Add(bccRecipientsEmail);
            }

            var attachedFileInfo = new StringBuilder();
            if (fileList != null) {

                var filterFileList = fileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList();

                if (filterFileList.Count()> 0) {
                    attachedFileInfo.Append(mailBodyFileInfoHeader);
                    filterFileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList().ForEach(x => {
                        attachedFileInfo.Append("<br><br>");
                        attachedFileInfo.Append($"&nbsp; - File name : {x.fileName}");
                        attachedFileInfo.Append("<br>");
                        attachedFileInfo.Append($"&nbsp; - Data count : {x.dataCount}");

                        var filePath = Path.Combine(x.filePath, x.fileName);
                        var attachment = new Attachment(filePath);

                        mail.Attachments.Add(attachment);
                    });
                    attachedFileInfo.Append(mailBodyFileInfoFooter);
                }
            }

            mail.Subject = vo.EmailTitle;
            mail.Body = mailBodyHeader;
            mail.Body += vo.EmailContent.Replace(Environment.NewLine, "<BR>") + attachedFileInfo.ToString();
            mail.Body += mailBodyEnd;
            mail.IsBodyHtml = true;

            try {
                var Client = new SmtpClient(mailServer);
                Client.Send(mail);
                Client.Dispose();
            } catch (SmtpException se) {
                Console.WriteLine("####################################################");
                Console.WriteLine("Exception caught when sendding email ");
                Console.WriteLine(se);
                Console.WriteLine("####################################################");
                log.Error("Exception caught when sendding email : {0}", se);
            }
        }

        public static void SendmailExternal(NotificationSchedulerVO vo, List<CreatedFileInfoVO> fileList = null) {
            var emailServer = System.Configuration.ConfigurationManager.AppSettings["AWSEmailServer"];
            var emailUserName = System.Configuration.ConfigurationManager.AppSettings["AWSEmailUserName"];
            var emailPassword = System.Configuration.ConfigurationManager.AppSettings["AWSEmailPassword"];

            var customerSenderName = vo.SenderName;
            var customerSenderEmail = vo.SenderEmail;
            var customerRecipientsEmail = vo.ReceiverEmail;
            var customerCcEmail = vo.CcEmail;
            var customerBccEmail = vo.BccEmail;

            var mail = new MailMessage();
            mail.From = new MailAddress(customerSenderEmail, customerSenderName);

            if (customerRecipientsEmail != null) {
                mail.To.Add(new MailAddress(customerRecipientsEmail));
            } else {
                mail.To.Add(customerRecipientsEmail);
            }
            if (customerCcEmail != null && !"".Equals(customerCcEmail)) {
                mail.CC.Add(customerCcEmail);
            }
            if (customerBccEmail != null && !"".Equals(customerBccEmail)) {
                mail.Bcc.Add(customerBccEmail);
            }

            var attachedFileInfo = new StringBuilder();
            if (fileList != null) {

                var filterFileList = fileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList();
                if (filterFileList.Count == 0 && vo.EmailOnlyDataFlag == 1) return;

                attachedFileInfo.Append(mailBodyFileInfoHeader);
                filterFileList.Where(x => x.filePath != CreatedFileInfoVO.nil).ToList().ForEach(x => {
                    attachedFileInfo.Append("<br><br>");
                    attachedFileInfo.Append($"File name : {x.fileName}");
                    attachedFileInfo.Append("<br>");
                    attachedFileInfo.Append($"Data count : {x.dataCount}");

                    var filePath = Path.Combine(x.filePath, x.fileName);

                    var attachment = new Attachment(filePath);

                    mail.Attachments.Add(attachment);
                });
                attachedFileInfo.Append(mailBodyFileInfoFooter);
            }

            mail.Subject = vo.EmailTitle;
            mail.Body = mailBodyHeader;
            mail.Body += vo.EmailContent + attachedFileInfo.ToString();
            mail.Body += mailBodyEnd;
            mail.IsBodyHtml = true;

            try {
                var Client = new SmtpClient(emailServer);
                Client.Port = 587;
                Client.EnableSsl = true;
                //Client.UseDefaultCredentials = true;
                Client.Credentials = new NetworkCredential(emailUserName, emailPassword);
                Client.Send(mail);
                Client.Dispose();
            } catch (SmtpException se) {
                log.Error("###### Exception caught when sendding email : {0}", se);
            }
        }

        public static void AddSystemIssueString(string subject, string msg) {
            MailHelper.systemInfoEmailList.Add(new SysErrorMsgVO(subject + " - " + msg));
        }
        public static void AddSystemIssueString(string className, string subject,string msg) {
            MailHelper.systemInfoEmailList.Add(new SysErrorMsgVO(className ,subject + " - " + msg));
        }

        public static void SnedSystemIssueEmail() {

            if (MailHelper.systemInfoEmailList.Count == 0) {
                //MailHelper.SendmailAdmin(MailString.SYSTEM_ISSUE_SUBJECT, "Clear Issue");
            } else {
                var sb = new StringBuilder();
                var count = 1;

                var errorList = MailHelper.systemInfoEmailList.OrderBy(o => o.className).ToList();

                sb.Append(mailBodyHeader);
                errorList.ForEach(x => {
                    sb.Append($"=========== Error Class name : {x.className} ===========");
                    sb.Append("<br>");
                    sb.Append(count++ + " : " + x.errormsg);
                    sb.Append("<br><br>");
                });
                sb.Append(mailBodyEnd);

                MailHelper.SendmailAdmin(MailString.SYSTEM_ISSUE_SUBJECT, sb.ToString());
            }
        }


        /* *********************************************
         * s : Custom mail function for sepific Class
         * *********************************************/

        public static void SendShipEmailToCustomer(ShipmentInfoVO shipmentInfoVo) {

            var customerSenderName = System.Configuration.ConfigurationManager.AppSettings["CustomerSenderName"];
            var customerSenderEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerSenderEmail"];
            var customerRecipientsName = System.Configuration.ConfigurationManager.AppSettings["CustomerRecipientsName"];
            var customerRecipientsEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerRecipientsEmail"];
            var customerCcEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerCcEmail"];
            var customerBccEmail = System.Configuration.ConfigurationManager.AppSettings["CustomerBccEmail"];

            var EMAIL_CONTENT_SERVER_PROTOCOL = System.Configuration.ConfigurationManager.AppSettings["EmailConetentServerWithProtocol"];
            var EMAIL_CONTENT_SERVER = System.Configuration.ConfigurationManager.AppSettings["EmailConetentServer"];
            var EMAIL_SHIPMENT_CONTENT = System.Configuration.ConfigurationManager.AppSettings["EmailShipmentPath"];
            var EMAIL_WILLCALL_URL = System.Configuration.ConfigurationManager.AppSettings["EmailWillCallPath"];

            var SUBJECT_ENGLISH = System.Configuration.ConfigurationManager.AppSettings["SubjectEnglish"];
            var SUBJECT_SPANISH = System.Configuration.ConfigurationManager.AppSettings["SubjectFrench"];
            var SUBJECT_FRENCH = System.Configuration.ConfigurationManager.AppSettings["SubjectSpanish"];

            var ORDER_CONTENT_PATH_ENGLISH = System.Configuration.ConfigurationManager.AppSettings["OrderPathEnglish"];
            var ORDER_CONTENT_PATH_SPANISH = System.Configuration.ConfigurationManager.AppSettings["OrderPathSpanish"];
            var ORDER_CONTENT_PATH_FRENCH = System.Configuration.ConfigurationManager.AppSettings["OrderPathFrench"];

            string CUSTOMER_CARE_PHONE = System.Configuration.ConfigurationManager.AppSettings["OrderPathFrench"];

            var subject = SUBJECT_ENGLISH;
            var contentUrl = EMAIL_SHIPMENT_CONTENT;
            var orderContentPath = ORDER_CONTENT_PATH_ENGLISH;
            var langCode = shipmentInfoVo.Language.ToLower() + "-us";
            if (shipmentInfoVo.Language == "FR") {
                langCode = shipmentInfoVo.Language.ToLower() + "-ca";
            }

            if (shipmentInfoVo.Language == "ES") {
                subject = SUBJECT_SPANISH;
                orderContentPath = ORDER_CONTENT_PATH_SPANISH;
            } else if (shipmentInfoVo.Language == "ES") {
                subject = SUBJECT_FRENCH;
                orderContentPath = ORDER_CONTENT_PATH_FRENCH;
            }

            var message = string.Empty;
            if (shipmentInfoVo.ShippingMethod == "CA_VAN_WILLCALL") {
                subject = "Your Univera Order is Ready to Pick Up.";
                contentUrl = EMAIL_WILLCALL_URL;
            }

            var fullURL = EMAIL_CONTENT_SERVER_PROTOCOL + "/" + langCode + contentUrl;
            var orderLink = EMAIL_CONTENT_SERVER + "/" + langCode + orderContentPath;

            var trackingURL = GetCarrierURL(shipmentInfoVo.ShippingMethod, shipmentInfoVo.TrackingNumber, langCode);
            var firstName = string.Empty;
            try {
                firstName = shipmentInfoVo.ShipToName.Contains(',') ? shipmentInfoVo.ShipToName.Split(' ').Last() : shipmentInfoVo.ShipToName.Split(' ').First();
            } catch (Exception e) {
                firstName = shipmentInfoVo.ShipToName;
            }

            var emailRecipient = shipmentInfoVo.ShipToEmail;

            var content = new WebCallHelper().GetRequestString(fullURL);
            message = content.Replace("{{FIRSTNAME}}", shipmentInfoVo.ShipToName)
                             .Replace("{{ORDER_LINK}}", orderLink)
                             .Replace("{{ORDERNUMBER}}", shipmentInfoVo.OrderNumber)
                             .Replace("{{FULLNAME}}", shipmentInfoVo.ShipToName)
                             .Replace("{{ADDRESS1}}", shipmentInfoVo.ShipToAddress1)
                             .Replace("{{ADDRESS2}}", shipmentInfoVo.ShipToAddress2)
                             .Replace("{{CITY}}", shipmentInfoVo.ShipToCity)
                             .Replace("{{STATE}}", shipmentInfoVo.ShipToState)
                             .Replace("{{POSTALCODE}}", shipmentInfoVo.ShipToZip + "\r\n")
                             .Replace("{{TRACKINGNUMBER}}", shipmentInfoVo.TrackingNumber)
                             .Replace("{{TRACKING_LINK}}", trackingURL + "\r\n")
                             .Replace("{{COUNTRYPHONE}}", CUSTOMER_CARE_PHONE);

            if (emailRecipient != null && emailRecipient != "" && shipmentInfoVo.ShipToEmail.Equals("CustomerCare@univera.com") == false) {
                SendMailsOutside(subject, message, customerSenderName, customerSenderEmail, null, shipmentInfoVo.ShipToEmail, null, customerBccEmail, null);
            }
        }

        public static void SendMailsOutside(String subject, String mailMsg, string senderName, string senderEmail, string receipientName, string receipientEmail, string ccEmail = null, string bccEmail = null, List<Attachment> attachmentList = null) {
            var emailServer = System.Configuration.ConfigurationManager.AppSettings["AWSEmailServer"];
            var emailUserName = System.Configuration.ConfigurationManager.AppSettings["AWSEmailUserName"];
            var emailPassword = System.Configuration.ConfigurationManager.AppSettings["AWSEmailPassword"];

            var mail = new System.Net.Mail.MailMessage();
            mail.From = new System.Net.Mail.MailAddress(senderEmail, senderName);
            if (receipientName != null) {
                mail.To.Add(new System.Net.Mail.MailAddress(receipientName, receipientEmail));
            } else {
                mail.To.Add(receipientEmail);
            }
            if (ccEmail != null && "".Equals(ccEmail) == false) {
                mail.CC.Add(ccEmail);
            }
            if (bccEmail != null && "".Equals(bccEmail) == false) {
                mail.Bcc.Add(bccEmail);
            }
            mail.Subject = subject;
            mail.Body = mailMsg;
            mail.IsBodyHtml = true;
            if (attachmentList != null) {
                foreach (Attachment attachment in attachmentList) {
                    mail.Attachments.Add(attachment);
                }
            }
            try {
                var Client = new System.Net.Mail.SmtpClient(emailServer);
                Client.Port = 587;
                Client.EnableSsl = true;
                //Client.UseDefaultCredentials = true;
                Client.Credentials = new NetworkCredential(emailUserName, emailPassword);
                Client.Send(mail);
            } catch (SmtpException se) {
                log.Error("###### Exception caught when sendding email : {0}", se);
            }
        }

        public static string GetCarrierURL (string shippingMethod, string trackingNumber, string language) {
            if (shippingMethod != null || "".Equals(shippingMethod) == false) {
                string carrier = shippingMethod.Substring(0, 3);
                switch (carrier.ToUpper()) {
                    case "GRO":
                        return System.Configuration.ConfigurationManager.AppSettings["Fedex_Tracking_URL"] + trackingNumber;
                    case "FED":
                        return System.Configuration.ConfigurationManager.AppSettings["Fedex_Tracking_URL"] + trackingNumber;
                    case "PRI":
                        return System.Configuration.ConfigurationManager.AppSettings["Fedex_Tracking_URL"] + trackingNumber;
                    case "SMA":
                        return System.Configuration.ConfigurationManager.AppSettings["Fedex_Tracking_URL"] + trackingNumber;
                    case "CAP":
                        return System.Configuration.ConfigurationManager.AppSettings["CanadaPost_Tracking_URL"] + trackingNumber + "&LOCALE=" + language + "&LOCALE2=" + language;
                    case "UPS":
                        return System.Configuration.ConfigurationManager.AppSettings["Ups_Tracking_URL"] + trackingNumber;
                    case "MAI":
                        return System.Configuration.ConfigurationManager.AppSettings["Usps_Tracking_URL"] + trackingNumber;
                    case "USP":
                        return System.Configuration.ConfigurationManager.AppSettings["Mi_Tracking_URL"] + trackingNumber.Replace(",", "%0a");
                    case "NOS":
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        /* *********************************************
         * e : Custom mail function for sepific function
         * *********************************************/
    }
}
