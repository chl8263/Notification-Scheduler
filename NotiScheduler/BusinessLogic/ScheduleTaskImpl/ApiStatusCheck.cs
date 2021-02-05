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
using System.Xml;
using System.Net;
using System.Configuration;

namespace NotiScheduler.BusinessLogic.ScheduleTaskImpl {
    class ApiStatusCheck : ScheduleTask {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private string SAP_USER = ConfigurationManager.AppSettings["SAP.UserId"];
        private string SAP_PASSWORD = ConfigurationManager.AppSettings["SAP.UserPass"];
        
        private string SERVER_NAME = ConfigurationManager.AppSettings["SAP.APIServers"];
        private string SCHEMA = ConfigurationManager.AppSettings["SAP.APISSSL"] == "YES" ? "https" : "http";
        private string API_PORT = ConfigurationManager.AppSettings["SAP.APIPorts"];

        private string SAP_US = ConfigurationManager.AppSettings["SAP.USCompanyName"];
        private string SAP_CA = ConfigurationManager.AppSettings["SAP.CACompanyName"];

        private string ITEMPRICE = "itemprice";
        private string ACCOUNT_NO = ConfigurationManager.AppSettings["CardCode"];
        private string ITEM_CODE = ConfigurationManager.AppSettings["ItemCode"];
        private string EXTENSION = "aspx";

        public void Execute(NotificationSchedulerVO vo) {
            try {
                
                var splitServerName = SERVER_NAME.Split(',');

                log.Info("============== Api state check ==============");
                foreach(var serverName in splitServerName) {
                    var urlUS = new UrlHelper().Scheme(SCHEMA)
                        .Host(serverName)
                        .Port(API_PORT)
                        .Path(SAP_US).Path(ITEMPRICE).Path(ACCOUNT_NO).Path(ITEM_CODE)
                        .Extension(EXTENSION)
                        .Build();

                    var urlCA = new UrlHelper()
                        .Scheme(SCHEMA)
                        .Host(serverName)
                        .Port(API_PORT)
                        .Path(SAP_CA).Path(ITEMPRICE).Path(ACCOUNT_NO).Path(ITEM_CODE)
                        .Extension(EXTENSION)
                        .Build();

                    Console.WriteLine("=======");
                    Console.WriteLine(urlUS);
                    Console.WriteLine(urlCA);

                    ApiServerStateCheck(serverName, SAP_US, urlUS);
                    ApiServerStateCheck(serverName, SAP_CA, urlCA);
                }
                log.Info("=============================================");

            } catch (Exception e) {
                log.Error(e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            }
        }

        private bool ApiServerStateCheck(string serverName, string contryType, String apiUrl){

            bool result = false;
            string xmlResult = "";

            try {

                Console.Write($"{serverName} {contryType} -->");
                XmlDocument resultXml = GetXML(apiUrl);
                XmlNodeList vStatusList = resultXml.GetElementsByTagName("b1Reply:status");

                if (vStatusList.Count > 0) {

                    xmlResult = vStatusList[0].InnerXml;
                    //if (serverName == "prdsapapi-02") throw new Exception(serverName + contryType);

                    if (xmlResult == "ERROR") {

                        XmlNodeList iErrorList = resultXml.GetElementsByTagName("b1Reply:error");

                        if (iErrorList.Count > 0) {

                            MailHelper.AddSystemIssueString(GetType().Name ,"API server check", $"{serverName} {contryType} status error check this server");
                            log.Error($"{serverName} {contryType} status error check this server");
                        } else {

                            MailHelper.AddSystemIssueString(GetType().Name, "API server check", $"{serverName} {contryType} status error check this server");
                            log.Error($"{serverName} {contryType} status error check this server");
                        }
                    } else {

                        Console.Write($" Status : {xmlResult} , ");
                        
                        var resultItemCode = resultXml.GetElementsByTagName("ItemCode");
                        var resultItemCodeString = resultItemCode[0].InnerXml;

                        if (resultItemCodeString == ITEM_CODE) {
                            Console.WriteLine(" Item code : Match");
                            result = true;

                        } else {
                            MailHelper.AddSystemIssueString(GetType().Name, $"API server check", $"{serverName} {contryType} Item Mathch fail");
                            log.Error($"{serverName} {contryType} Item Mathch fail");
                        }
                    }
                    
                } else {
                    MailHelper.AddSystemIssueString(GetType().Name, "API server check", $"{serverName} {contryType} status not exist");
                    log.Error($"{serverName} {contryType} status not exist");
                }

            } catch (Exception e) {
                MailHelper.AddSystemIssueString(GetType().Name, $"{serverName} error", e.ToString());
                log.Error($"{serverName} error {e.ToString()}");
            }

            return result;
        }

        private XmlDocument GetXML(string url) {
            try {
                HttpWebRequest request = GenerateRequest(url);
                request.Method = WebRequestMethods.Http.Get;
                //request.Timeout = 

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receiveStream, encode);
                XmlDocument doc = new XmlDocument();
                doc.Load(readStream);

                return doc;
            } catch (Exception ex) {
                log.Error("Exception caught when Generating request", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            }
        }

        private HttpWebRequest GenerateRequest(string relativeUrl) {
            try {

                HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(relativeUrl);
                webreq.Credentials = new NetworkCredential(SAP_USER, SAP_PASSWORD);
                return webreq;
            } catch (Exception ex) {
                log.Error("Exception caught when Generating request [" + relativeUrl + "] : {0}", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            }
        }
    }
}
