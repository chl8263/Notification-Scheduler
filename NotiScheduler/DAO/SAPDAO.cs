using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Data.SqlClient;
using System.Data;
using NotiScheduler.Bean;
using NotiScheduler.VO;
using NotiScheduler.Const;
using System.Configuration;
using NotiScheduler.BusinessLogic.ScheduleTaskImpl;

namespace NotiScheduler {
    class SAPDAO : Helper.DBHelper {

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public SAPDAO() {
        }

        public SAPDAO(string country) {
            setConnection("SAP", country);
        }

        public List<NotificationSchedulerVO> getScheduler() {
            SqlDataReader rdr = null;

            var tdlist = new List<NotificationSchedulerVO>();
            string specialRunOnly = (ConfigurationManager.AppSettings["SpecialRunOnly"]=="Y"?"1":"0");
            try {
            var sqlQuery = @"SELECT Id
	                                , Department
	                                , Cycle
	                                , CycleDate
	                                , CycleDay
	                                , CycleTime
	                                , EmailServer
	                                , EmailTitle
	                                , EmailContent
	                                , ReceiverEmail
	                                , SenderName
	                                , SenderEmail
	                                , CcEmail
	                                , BccEmail
	                                , ProgramClass
	                                , Isnull(FirstRunDate,'1900-01-01') FirstRunDate
	                                , Isnull(LastRunDate,'1900-01-01') LastRunDate
	                                , CreateDate
	                                , ModifiedDate
                                    , isnull(Active,0) Active
                                    , isnull(EmailOnlyDataFlag,0) EmailOnlyDataFlag
                                FROM SAP_NotificationScheduler
                                WHERE SpecialRun = " + specialRunOnly + @"
                                ORDER BY Id";
            rdr = getRecordSet(sqlQuery);

            if (rdr.HasRows) {
                while (rdr.Read()) {
                    var vo = new NotificationSchedulerVO();
                    vo.Id = Convert.ToInt32(rdr["Id"]);
                    vo.Department = rdr["Department"].ToString();
                    vo.Cycle = rdr["Cycle"].ToString();
                    vo.CycleDate = (rdr["CycleDate"].ToString() == "") ? 0 : Convert.ToInt32(rdr["CycleDate"]);
                    vo.CycleDay = rdr["CycleDay"].ToString();
                    vo.CycleTime = (rdr["CycleTime"].ToString() == "") ? TODAY.DEFAULT_CYCLE_TIME : rdr["CycleTime"].ToString();
                    vo.EmailServer = rdr["EmailServer"].ToString();
                    vo.EmailTitle = rdr["EmailTitle"].ToString();
                    vo.EmailContent = rdr["EmailContent"].ToString();
                    vo.ReceiverEmail = rdr["ReceiverEmail"].ToString();
                    vo.SenderName = rdr["SenderName"].ToString();
                    vo.SenderEmail = rdr["SenderEmail"].ToString();
                    vo.CcEmail = rdr["CcEmail"].ToString();
                    vo.BccEmail = rdr["BccEmail"].ToString();
                    vo.ProgramClass = rdr["ProgramClass"].ToString();
                    vo.FirstRunDate = Convert.ToDateTime(rdr["FirstRunDate"]);
                    vo.LastRunDate = Convert.ToDateTime(rdr["LastRunDate"]);
                    vo.CreateDate = rdr["CreateDate"].ToString();
                    vo.ModifiedDate = rdr["ModifiedDate"].ToString();
                    vo.Active = Convert.ToInt32(rdr["Active"]);
                    vo.EmailOnlyDataFlag = Convert.ToInt32(rdr["EmailOnlyDataFlag"]);
                    tdlist.Add(vo);
                }
            }
            if (rdr != null && rdr.IsClosed == false) {
                rdr.Close();
            }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return tdlist;
        }

        // --------------------------------------------------------
        // ---------------- Valid to Shipment check ---------------
        // --------------------------------------------------------
        public List<ValidToShipVO> GetValidToShipData() {
            SqlDataReader rdr = null;
            var tdlist = new List<ValidToShipVO>();

            try {
                var sqlQuery = @"SELECT OC.CardCode
                                      , OC.CardName
	                                  , OG.GroupName
	                                  , OC.Phone1
	                                  , OC.E_Mail
	                                  , OC.U_JoinDate
	                                  , OC.MailAddres + ' ' + OC.MailCity + ' ' + OC.State2 + ' ' + OC.MailZipCod MailAddres
	                                  , OC.U_Sponsor
	                                  , OCS.CardName AS SponsorName
                                   FROM SAPUS.dbo.OCRD OC
                                  INNER JOIN SAPUS.dbo.OCRG OG
                                     ON OC.GroupCode = OG.GroupCode
                                   LEFT JOIN SAPUS.dbo.OCRD OCS
                                     ON OC.U_Sponsor = OCS.CardCode
                                   LEFT JOIN SAPUS.dbo.OCRD OCP
                                     ON OC.U_Sponsor = OCP.CardCode
                                  WHERE (OC.U_ValidToShip is null or OC.U_ValidToShip = 'N')
                                    AND OC.U_IsTerminated = 0
                                    AND OC.CardType = 'C'
                                    AND OC.frozenFor = 'N'
                                    AND ((SELECT count(*) 
                                            FROM SAPUS.dbo.ORDR OH 
                                           WHERE OH.CardCode = OC.CardCOde 
                                             AND OH.CANCELED='N' 
                                             AND OH.U_OrderType <> 'STORE' 
                                             AND OH.DocDate >= '2020-01-01') +
                                         (SELECT count(*) 
                                            FROM SAPCA.dbo.ORDR OH 
                                           WHERE OH.CardCode = OC.CardCOde 
                                             AND OH.CANCELED='N' 
                                             AND OH.DocDate >= '2020-01-01')
                                        ) > 0
                                    ORDER BY 1";
                //SqlParameter[] param = { new SqlParameter("@Period", period) };
                rdr = getRecordSet(sqlQuery);

                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        var vo = new ValidToShipVO();
                        vo.CardCode = rdr["CardCode"].ToString();
                        vo.CardName = rdr["CardName"].ToString();
                        vo.GroupName = rdr["GroupName"].ToString();
                        vo.Phone1 = rdr["Phone1"].ToString();
                        vo.E_Mail = rdr["E_Mail"].ToString();
                        vo.U_JoinDate = rdr["U_JoinDate"].ToString();
                        vo.MailAddres = rdr["MailAddres"].ToString();
                        vo.U_Sponsor = rdr["U_Sponsor"].ToString();
                        vo.SponsorName = rdr["SponsorName"].ToString();

                        tdlist.Add(vo);
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return tdlist;
        }

        // --------------------------------------------------------
        // ---------------- Payment Differnt check ----------------
        // --------------------------------------------------------
        public List<PaymentDiffVO> getPaymentDiffData() {
            SqlDataReader rdr = null;
            var tdlist = new List<PaymentDiffVO>();

            try {
                var sqlQuery = @"-- US Auth payment different
                                 SELECT 'Payment different' AS Category
                                      , OH.CardCode
                                      , OH.DocNum
                                      , CONVERT(CHAR(10), OH.DocDate, 23) DocDate
                                   FROM SAPUS.dbo.ORDR OH
                                  INNER JOIN SAPUS.dbo.[@BOY_E0_OTCC] CC
                                     ON CC.U_DOCENTRY = OH.DocEntry
                                  INNER JOIN SAPUS.dbo.[@BOY_E0_AUTH_DATAST] AUTH
                                     ON AUTH.U_DocEntry = CC.U_DOCENTRY
                                    AND AUTH.U_CreditCardId = CC.U_TRANSREF
                                    AND AUTH.U_ObjectType = '17'
                                    AND AUTH.U_SettleStatus <> '4'
                                  WHERE OH.DocDate >= DATEADD(DAY, -7, GETDATE())
                                    AND OH.DocTotal <> AUTH.U_Amount
                                  GROUP BY OH.DocTotal, OH.CardCode, OH.DocNum, CONVERT(CHAR(10), OH.DocDate, 23)
                                 HAVING SUM(AUTH.U_Amount) <> OH.DocTotal
                                 UNION ALL
                                 -- US Auth payment different
                                 SELECT 'Payment different' AS Category
                                      , OH.CardCode
                                      , OH.DocNum
                                      , CONVERT(CHAR(10), OH.DocDate, 23) DocDate
                                   FROM SAPCA.dbo.ORDR OH
                                  INNER JOIN SAPCA.dbo.[@BOY_E0_OTCC] CC
                                     ON CC.U_DOCENTRY = OH.DocEntry
                                  INNER JOIN SAPCA.dbo.[@BOY_E0_AUTH_DATAST] AUTH
                                     ON AUTH.U_DocEntry = CC.U_DOCENTRY
                                    AND AUTH.U_CreditCardId = CC.U_TRANSREF
                                    AND AUTH.U_ObjectType = '17'
                                    AND AUTH.U_SettleStatus <> '4'
                                  WHERE OH.DocDate >= DATEADD(DAY, -7, GETDATE())
                                    AND OH.DocTotal <> AUTH.U_Amount
                                  GROUP BY OH.DocTotal, OH.CardCode, OH.DocNum, CONVERT(CHAR(10), OH.DocDate, 23)
                                 HAVING SUM(AUTH.U_Amount) <> OH.DocTotal
                                 UNION ALL
                                 -- US Settleed payment different
                                 SELECT 'Payment different' AS Category
                                      , OH.CardCode
                                      , OH.DocNum
                                      , CONVERT(CHAR(10), OH.DocDate, 23) DocDate
                                   FROM SAPUS.dbo.ORDR OH
                                  INNER JOIN SAPUS.dbo.OINV OAR
                                     ON OH.DocNum = OAR.U_RelatedOrder
                                  INNER JOIN SAPUS.dbo.[@BOY_E0_OTCC] CC
                                     ON CC.U_DOCENTRY = OH.DocEntry
                                  INNER JOIN SAPUS.dbo.[@BOY_E0_AUTH_DATAST] AUTH
                                     ON AUTH.U_DocEntry = OAR.DocEntry
                                    AND AUTH.U_ObjectType = '13'
                                    AND AUTH.U_CreditCardId = CC.U_TRANSREF
                                    AND AUTH.U_SettleStatus <> '4'
                                  WHERE OH.DocDate >= DATEADD(DAY, -7, GETDATE())
                                    AND OH.DocTotal <> AUTH.U_Amount
                                  GROUP BY OH.DocTotal, OH.CardCode, OH.DocNum, CONVERT(CHAR(10), OH.DocDate, 23)
                                 HAVING SUM(AUTH.U_Amount) <> OH.DocTotal
                                 UNION ALL
                                 -- CA Settleed payment different
                                 SELECT 'Payment different' AS Category
                                      , OH.CardCode
                                      , OH.DocNum
                                      , CONVERT(CHAR(10), OH.DocDate, 23) DocDate
                                   FROM SAPCA.dbo.ORDR OH
                                  INNER JOIN SAPCA.dbo.OINV OAR
                                     ON OH.DocNum = OAR.U_RelatedOrder
                                  INNER JOIN SAPCA.dbo.[@BOY_E0_OTCC] CC
                                     ON CC.U_DOCENTRY = OH.DocEntry
                                  INNER JOIN SAPCA.dbo.[@BOY_E0_AUTH_DATAST] AUTH
                                     ON AUTH.U_DocEntry = OAR.DocEntry
                                    AND AUTH.U_ObjectType = '13'
                                    AND AUTH.U_CreditCardId = CC.U_TRANSREF
                                    AND AUTH.U_SettleStatus <> '4'
                                  WHERE OH.DocDate >= DATEADD(DAY, -7, GETDATE())
                                    AND OH.DocTotal <> AUTH.U_Amount
                                  GROUP BY OH.DocTotal, OH.CardCode, OH.DocNum, CONVERT(CHAR(10), OH.DocDate, 23)
                                 HAVING SUM(AUTH.U_Amount) <> OH.DocTotal
                                 UNION ALL
                                 -- US payment error
                                 SELECT 'Payment error' AS Category
                                      , OH.CardCode
                                      , OH.DocNum
                                      , CONVERT(CHAR(10), OH.DocDate, 23) DocDate
                                   FROM SAPUS.dbo.ORDR OH
                                   LEFT JOIN SAPUS.dbo.[@BOY_E0_OTCINTE] AA
                                     ON OH.DocEntry = AA.U_DOCENTRY
                                  WHERE OH.DocDate >= DATEADD(DAY, -7, GETDATE())
                                    AND U_IMPORTSTATUS = '2'
                                 UNION ALL
                                 -- CA payment error
                                 SELECT 'Payment error' AS Category
                                      , OH.CardCode
                                      , OH.DocNum
                                      , CONVERT(CHAR(10), OH.DocDate, 23) DocDate
                                   FROM SAPCA.dbo.ORDR OH
                                   LEFT JOIN SAPCA.dbo.[@BOY_E0_OTCINTE] AA
                                     ON OH.DocEntry = AA.U_DOCENTRY
                                  WHERE OH.DocDate >= DATEADD(DAY, -7, GETDATE())
                                    AND U_IMPORTSTATUS = '2'
                                  ORDER BY 1, 2
                                 ";
                rdr = getRecordSet(sqlQuery);

                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        var vo = new PaymentDiffVO();
                        vo.Category = rdr["Category"].ToString();
                        vo.CardCode = rdr["CardCode"].ToString();
                        vo.DocNum = rdr["DocNum"].ToString();
                        vo.DocDate = rdr["DocDate"].ToString();

                        tdlist.Add(vo);
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return tdlist;
        }

        // --------------------------------------------------------
        // ---------------- GetAccountForRenewalData  -------------
        // --------------------------------------------------------
        public List<AccountForRenewalVO> GetAccountForRenewalData(MonthPeroid peroid) {
            SqlDataReader rdr = null;
            var tdlist = new List<AccountForRenewalVO>();

            try {
                var sqlQuery = "";

                if (peroid == MonthPeroid.CURRENT_MONTH) {
                    sqlQuery = "Exec SAPCA.unv.Report_GetAccountForRenewal 0";
                } else if (peroid == MonthPeroid.LAST_MONTH) {
                    sqlQuery = "Exec SAPCA.unv.Report_GetAccountForRenewal -1";
                } else {
                    sqlQuery = "Exec SAPCA.unv.Report_GetAccountForRenewal 1";
                }

                
                
                //SqlParameter[] param = { new SqlParameter("@Period", period) };
                rdr = getRecordSet(sqlQuery);

                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        var vo = new AccountForRenewalVO();
                        vo.AccountID = rdr["Code"].ToString();
                        vo.RenewalDate = rdr["RenewalDate"].ToString();
                        vo.FirstName = rdr["FirstName"].ToString();
                        vo.LastName = rdr["LastName"].ToString();
                        vo.Phone = rdr["Phone"].ToString();
                        vo.Email = rdr["Email"].ToString();
                        vo.EmailOptout = rdr["OptOut"].ToString();
                        vo.NextCPDate = rdr["NextCPDate"].ToString();

                        tdlist.Add(vo);
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return tdlist;
        }


        // --------------------------------------------------------
        // ---------------- Ship Email ---------------
        // --------------------------------------------------------

        public List<ShipmentInfoVO> GetShipmentInfoData() {
            SqlDataReader rdr = null;
            var list = new List<ShipmentInfoVO>();
            try {
                //SqlParameter[] param = { new SqlParameter("@DeliveryNumber", deliveryNumber)};
                string sqlQuery = @"SELECT DISTINCT 
                                    	   R1.DocEntry AS DocEntry
	                                     , OC.CardCode AS CardCode
	                                     , OH.DocNum AS OrderNumber
	                                     , OH.CardName AS ShipToName
                                         , OC.E_Mail ShipToEmail
	                                     , OH.U_ShippingMethod AS ShippingMethod
	                                     , R12.StreetS AS ShipToAddress1
	                                     , R12.BlockS AS ShipToAddress2
	                                     , R12.CityS AS ShipToCity
	                                     , R12.StateS AS ShipToState
	                                     , R12.ZipCodeS AS ShipToZip
	                                     , R1.U_TrackingNumber AS TrackingNumber
	                                     , R1.U_DeliveryNumber AS DeliveryNumber
	                                     , CASE WHEN OL.ShortName = 'CO' THEN 'ES' ELSE OL.ShortName END AS Language
                                      FROM ORDR OH
                                     INNER JOIN RDR1 R1
                                        ON OH.DocEntry = R1.DocEntry
                                     INNER JOIN RDR12 R12
                                        ON OH.DocEntry = R12.DocEntry
                                     INNER JOIN OCRD OC
                                        ON OH.CardCode = OC.CardCode
                                     INNER JOIN OLNG OL 
                                        ON OC.LangCode = OL.Code
                                     WHERE CANCELED = 'N'
                                       AND ISNULL(R1.U_DeliveryNumber,'') <> ''
                                       AND ISNULL(R1.U_TrackingNumber,'') <> ''
                                       AND ISNULL(R1.U_IsShipped,'') = ''
                                       AND OH.DocDate >= '2020-05-01'
                                       AND OC.U_OptOut = 0
                                    ";
                rdr = getRecordSet(sqlQuery);
                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        var vo = new ShipmentInfoVO();
                        vo.DocEntry = rdr["DocEntry"].ToString();
                        vo.DeliveryNumber = rdr["DeliveryNumber"].ToString();
                        vo.OrderNumber = rdr["OrderNumber"].ToString();
                        vo.CardCode = rdr["CardCode"].ToString();
                        vo.Language = rdr["Language"].ToString();
                        vo.ShipToEmail = rdr["ShipToEmail"].ToString();
                        vo.ShipToName = rdr["ShipToName"].ToString();
                        vo.ShippingMethod = rdr["ShippingMethod"].ToString();
                        vo.ShipToAddress1 = rdr["ShipToAddress1"].ToString();
                        vo.ShipToAddress2 = rdr["ShipToAddress2"].ToString();
                        vo.ShipToCity = rdr["ShipToCity"].ToString();
                        vo.ShipToState = rdr["ShipToState"].ToString();
                        vo.ShipToZip = rdr["ShipToZip"].ToString();
                        vo.TrackingNumber = rdr["TrackingNumber"].ToString();
                        list.Add(vo);
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                throw ex;
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
                throw e;
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return list;
        }

        public bool UpdateIsShipped(string docEntry) {

            bool result = false;
            try {
                var sqlQuery = @" UPDATE RDR1
                                     SET U_IsShipped = 1
                                   WHERE DocEntry = @DocEntry
                                ;";
                SqlParameter[] param = { new SqlParameter("@DocEntry", docEntry) };
                result = setSaveData(sqlQuery, param);
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            }
            return result;
        }

        // --------------------------------------------------------
        // --------------------- this is test ---------------------
        // --------------------------------------------------------
        public List<TestNotiVO> getTestNoti() {
            SqlDataReader rdr = null;
            var tdlist = new List<TestNotiVO>();

            try {
                var sqlQuery = @"SELECT *
                                    FROM TestNoti";
                //SqlParameter[] param = { new SqlParameter("@Period", period) };
                rdr = getRecordSet(sqlQuery);

                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        var vo = new TestNotiVO();
                        vo.Test = rdr["Test"].ToString();

                        tdlist.Add(vo);
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return tdlist;
        }

        public bool UpdateLastRunDate(int id, DateTime lastRunDate) {

            bool result = false;
            try {
                var sqlQuery = @"UPDATE SAP_NotificationScheduler
                                    SET LastRunDate = @LastRunDate
                                  WHERE Id = @Id;";
                SqlParameter[] param = { new SqlParameter("@Id", id), new SqlParameter("@LastRunDate", lastRunDate) };
                result = setSaveData(sqlQuery, param);
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                //MailHelper.sendmail("Commission Adjust Error", @"Team,
                //            During insert data Commission Adjust Data, error occurred. Please check." +
                //            "The error message like below." +
                //            ex);
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
                //MailHelper.sendmail("Commission Adjust Error", @"Team,
                //            During insert data Commission Adjust Data, error occurred. Please check." +
                //            "The error message like below." +
                //            e);
            }
            return result;
        }

        public bool UpdateFirstRunDate(int id, DateTime FistRunDate) {

            bool result = false;
            try {
                var sqlQuery = @"UPDATE SAP_NotificationScheduler
                                    SET FirstRunDate = @FistRunDate
                                  WHERE Id = @Id;";
                SqlParameter[] param = { new SqlParameter("@Id", id), new SqlParameter("@FistRunDate", FistRunDate) };
                result = setSaveData(sqlQuery, param);
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                //MailHelper.sendmail("Commission Adjust Error", @"Team,
                //            During insert data Commission Adjust Data, error occurred. Please check." +
                //            "The error message like below." +
                //            ex);
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
                //MailHelper.sendmail("Commission Adjust Error", @"Team,
                //            During insert data Commission Adjust Data, error occurred. Please check." +
                //            "The error message like below." +
                //            e);
            }
            return result;
        }

        public String getPeriod(String period) {
            SqlDataReader rdr = null;
            String result = "";

            try {
                string sqlQuery = @"SELECT Code
                                      FROM Period
                                     WHERE Name = @Period";
                SqlParameter[] param = { new SqlParameter("@Period", period) };
                rdr = getRecordSet(sqlQuery, param);

                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        result = rdr["Code"].ToString();
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            }
            return result;
        }

        public bool noAccountCheck(String cardCode, String volumeCode, String period) {
            SqlDataReader rdr = null;
            bool result = false;

            try {
                string sqlQuery = @"SELECT COUNT(ID) AS IdCount
                                      FROM [unv].[UNV_VolumeAdjustment]
                                     WHERE CardCode = @cardCode
                                       AND Period = @period
                                       AND J6VolumeCode = @volumeCode";
                SqlParameter[] param = { new SqlParameter("@cardCode", cardCode)
                                       , new SqlParameter("@period", period)
                                       , new SqlParameter("@volumeCode", volumeCode)};
                rdr = getRecordSet(sqlQuery, param);

                if (rdr.HasRows) {
                    while (rdr.Read()) {
                        int existAccount = Convert.ToInt32(rdr["IdCount"]);
                        result = (existAccount == 0 ? true : false);
                    }
                }
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } catch (SqlException ex) {
                MailHelper.AddSystemIssueString(GetType().Name, ex.ToString());
                log.Error(ex);
            } catch (Exception e) {
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
                log.Error(e);
            } finally {
                if (rdr != null && rdr.IsClosed == false) {
                    rdr.Close();
                }
            } 

            return result;
        }

        public bool insertAdjustCommissionData(string sqlQuery, string period) {

            bool result = false;
            try {
                if (sqlQuery != null && sqlQuery != "") {
                    SqlParameter[] param = { new SqlParameter("@Peroid", period) };
                    result = setSaveData(sqlQuery, param);
                } else return false;
            } catch (SqlException ex) {
                log.Error("SQL Exception", ex);
                MailHelper.AddSystemIssueString(GetType().Name, "Commission Adjust Error", "During insert data Commission Adjust Data, error occurred. Please check.The error message like below." + ex);
            } catch (Exception e) {
                log.Error("Exception", e);
                MailHelper.AddSystemIssueString(GetType().Name, "Commission Adjust Error", "During insert data Commission Adjust Data, error occurred. Please check.The error message like below." + e);
            }
            return result;
        }
    }
}
