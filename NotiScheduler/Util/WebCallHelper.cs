using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NotiScheduler.Util {
    class WebCallHelper {

        protected static readonly ILog log = LogManager.GetLogger(typeof(WebCallHelper));

        public XmlDocument PostRequestXML(string xmlPostString, string url, string stringType) {
            try {
                string userName = System.Configuration.ConfigurationManager.AppSettings["SAP.UserName"];
                string password = System.Configuration.ConfigurationManager.AppSettings["SAP.Password"];

                HttpWebRequest request = GenerateRequest(url, userName, password);
                request.Method = "POST";
                //request.Timeout = 200;
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] bytedata = Encoding.UTF8.GetBytes(xmlPostString);
                request.ContentLength = bytedata.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytedata, 0, bytedata.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Gets the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encode);
                XmlDocument doc = new XmlDocument();
                doc.Load(readStream);
                return doc;
            } catch (Exception ex) {
                log.Error("Exception caught when Generating request [" + xmlPostString + "] : {0}", ex);
                throw ex;
            }
        }

        public string GetRequestString(string url) {
            try {
                const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
                const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
                ServicePointManager.SecurityProtocol = Tls12;

                HttpWebRequest request = GenerateRequest(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receiveStream, encode);
                string content = readStream.ReadToEnd();

                return content;
            } catch (Exception ex) {
                log.Error("Exception caught when Generating request [" + url + "] : {0}", ex);
                throw ex;
            }
        }

        public string GetHttpRequestString(string url) {
            try {
                HttpWebRequest request = GenerateRequest(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receiveStream, encode);
                string content = readStream.ReadToEnd();

                return content;
            } catch (Exception ex) {
                log.Error("Exception caught when Generating request [" + url + "] : {0}", ex);
                throw ex;
            }
        }

        private HttpWebRequest GenerateRequest(string relativeUrl, string id = null, string password = null) {
            try {
                HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(relativeUrl);
                if (id != null) {
                    webreq.Credentials = new NetworkCredential(id, password);
                }
                return webreq;
            } catch (Exception ex) {
                log.Error("Exception caught when Generating request [" + relativeUrl + "] : {0}", ex);
                throw ex;
            }
        }
    }
}
