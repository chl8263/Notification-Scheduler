using log4net;
using NotiScheduler.Bean;
using NotiScheduler.BusinessLogic.ScheduleTaskImpl;
using NotiScheduler.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/* TODO
 * 최초 실행시 Fist run time 기록하기 - done
 * HOURLY 일때 몇시부터 몇시까지 돌아야하는 시간 제한을 두어야 하는가? - 보류
 * Class 명을 찾지 못한경우 이메일 큐에 삽입 - done
 * 실행 로그 테이블 생성 - done
 */

namespace NotiScheduler.BusinessLogic {

    class SchedulerManager {

        private const string TASK_NAMESPACE = "NotiScheduler.BusinessLogic.ScheduleTaskImpl.";

        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private SAPDAO sdUP = null;

        public List<NotificationSchedulerVO> GetValidTaskQueue() {
            sdUP = new SAPDAO("UP");

            var resultQueue = new List<NotificationSchedulerVO>();

            try {
                var list = sdUP.getScheduler();

                list.ForEach(x => {

                    Console.WriteLine("==========");
                    Console.WriteLine("Id ->" + x.Id + " ");
                    Console.WriteLine("Department ->" + x.Department + " ");
                    Console.WriteLine("Cycle ->" + x.Cycle + " ");
                    Console.WriteLine("CycleDate ->" + x.CycleDate + " ");
                    Console.WriteLine("CycleDay ->" + x.CycleDay + " ");
                    Console.WriteLine("CycleTime ->" + x.CycleTime + " ");
                    Console.WriteLine("EmailServer ->" + x.EmailServer + " ");
                    Console.WriteLine("EmailTitle ->" + x.EmailTitle + " ");
                    Console.WriteLine("EmailContent ->" + x.EmailContent + " ");
                    Console.WriteLine("ReceiverEmail ->" + x.ReceiverEmail + " ");
                    Console.WriteLine("SenderName ->" + x.SenderName + " ");
                    Console.WriteLine("SenderEmail ->" + x.SenderEmail + " ");
                    Console.WriteLine("CcEmail ->" + x.CcEmail + " ");
                    Console.WriteLine("BccEmail ->" + x.BccEmail + " ");
                    Console.WriteLine("ProgramClass ->" + x.ProgramClass + " ");
                    Console.WriteLine("FisrtRunDate ->" + x.FirstRunDate + " ");
                    Console.WriteLine("LastRunDate ->" + x.LastRunDate + " ");
                    Console.WriteLine("CreateDate ->" + x.CreateDate + " ");
                    Console.WriteLine("ModifiedDate ->" + x.ModifiedDate + " ");
                    Console.WriteLine("==========");

                    if (x.Active == 1) {
                        try {
                            switch (x.Cycle) {
                                case CYCLE.ANY: {
                                        Console.WriteLine("INSERT QUEUE ============");
                                        resultQueue.Add(x);

                                        if (x.LastRunDate == new DateTime(1900, 01, 01)) { // if first run
                                            sdUP.UpdateFirstRunDate(x.Id, DateTime.Now);
                                            sdUP.UpdateLastRunDate(x.Id, DateTime.Now);
                                        } else sdUP.UpdateLastRunDate(x.Id, DateTime.Now);

                                        break;
                                    }
                                case CYCLE.HOURLY: {
                                        Console.WriteLine("HOURLY ============");

                                        if (TimeValidationAndInsert(x, CYCLE.HOURLY)) {
                                            resultQueue.Add(x);
                                        }

                                        break;
                                    }
                                case CYCLE.DAILY: {
                                        Console.WriteLine("DAILY ============");

                                        if (TimeValidationAndInsert(x, CYCLE.DAILY)) {
                                            resultQueue.Add(x);
                                        }

                                        break;
                                    }
                                case CYCLE.WEEKLY: {

                                        if (x.CycleDay == TODAY.DAYOFWEEK) {
                                            Console.WriteLine("WEEKLY PASS ============");

                                            if (TimeValidationAndInsert(x, CYCLE.WEEKLY)) {
                                                resultQueue.Add(x);
                                            }

                                        } else {
                                            Console.WriteLine("WEEKLY NOT PASS ============");
                                        }

                                        break;
                                    }
                                case CYCLE.MONTHLY: {

                                        if (TODAY.DATE == x.CycleDate) {
                                            Console.WriteLine("MONTHLY PASS ============");
                                            if (TimeValidationAndInsert(x, CYCLE.MONTHLY)) {
                                                resultQueue.Add(x);
                                            }

                                        } else {
                                            Console.WriteLine("MONTHLY NOT PASS ============");
                                        }
                                        break;
                                    }
                            }
                        } catch (Exception e) {
                            MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
                        }
                    } else {
                        Console.WriteLine("NOT ACTIVE ============");
                    }
                });
            } catch (Exception e) {
                log.Error(e);
                MailHelper.AddSystemIssueString(GetType().Name, e.ToString());
            } finally {
                sdUP.closeConnection();
            };

            return resultQueue;
        }

        public Boolean TimeValidationAndInsert(NotificationSchedulerVO x, string CycleType) {

            var result = false;

            if (CycleType == CYCLE.HOURLY) {

                if (x.LastRunDate == new DateTime(1900, 01, 01)) {  // if first run // 마지막 실행날짜가 없으면, 즉 최초 실행일때
                    Console.WriteLine("FIRST RUN ============");
                    Console.WriteLine("INSERT QUEUE ============");

                    sdUP.UpdateFirstRunDate(x.Id, DateTime.Now);
                    sdUP.UpdateLastRunDate(x.Id, DateTime.Now);

                    result = true;

                } else {

                    Console.WriteLine("NOT FIRST RUN ============");

                    var schour = new DateTime(x.LastRunDate.Year, x.LastRunDate.Month, x.LastRunDate.Day, x.LastRunDate.Hour, 0, 0);
                    var current = new DateTime(TODAY.DETETIME.Year, TODAY.DETETIME.Month, TODAY.DETETIME.Day, TODAY.DETETIME.Hour, 0, 0);
                    var diff = current - schour;
                    if (diff.Hours >= 1) {  // 전에 돌렸던 시간과 1시간 이상 차이가 난다면
                        Console.WriteLine("INSERT QUEUE ============");
                        result = true;
                        sdUP.UpdateLastRunDate(x.Id, DateTime.Now);
                    } else {
                        Console.WriteLine("HOURLY ALREAY RUN ============");
                    }
                }
            } else {
                var currentHourMinute = DateTime.ParseExact(TODAY.HOUR, "HH", null);
                var cycleTime = DateTime.ParseExact(DateTime.ParseExact(x.CycleTime, "HH:mm", null).ToString("HH"), "HH", null);

                if (DateTime.Compare(currentHourMinute, cycleTime) == 0) {  // 현재 시간과 사이클 다음을 비교해서 클때만 시작하게하고
                    Console.WriteLine("OVER TIME PASS ============");

                    if (x.LastRunDate == new DateTime(1900, 01, 01)) {  // if first run // 마지막 실행날짜가 없으면, 즉 최초 실행일때
                        Console.WriteLine("FIRST RUN ============");
                        Console.WriteLine("INSERT QUEUE ============");

                        sdUP.UpdateFirstRunDate(x.Id, DateTime.Now);
                        sdUP.UpdateLastRunDate(x.Id, DateTime.Now);

                        result = true;

                    } else {
                        var todaty = new DateTime(TODAY.DETETIME.Year, TODAY.DETETIME.Month, TODAY.DETETIME.Day);
                        var scLastSimpleDate = new DateTime(x.LastRunDate.Year, x.LastRunDate.Month, x.LastRunDate.Day);

                        if (todaty == scLastSimpleDate) {
                            Console.WriteLine("TODAY ALREAY RUN ============");
                        } else {
                            Console.WriteLine("INSERT QUEUE ============");
                            sdUP.UpdateLastRunDate(x.Id, DateTime.Now);
                            result = true;
                        }
                    }
                } else {
                    Console.WriteLine("OVER TIME NOT PASS ============");
                }

            }

            //var currentHourMinute = DateTime.ParseExact(TODAY.HOURMIN, "HH:mm", null);
            //var cycleTime = DateTime.ParseExact(x.CycleTime, "HH:mm", null);

            //if (DateTime.Compare(currentHourMinute, cycleTime) == 0) {  // 현재 시간과 사이클 다음을 비교해서 클때만 시작하게하고
            //    Console.WriteLine("OVER TIME PASS ============");

            //    if (x.LastRunDate == new DateTime(1900, 01, 01)) {  // if first run // 마지막 실행날짜가 없으면, 즉 최초 실행일때
            //        Console.WriteLine("FIRST RUN ============");
            //        Console.WriteLine("INSERT QUEUE ============");

            //        sdUP.UpdateFirstRunDate(x.Id, DateTime.Now);
            //        sdUP.UpdateLastRunDate(x.Id, DateTime.Now);

            //        result = true;

            //    } else {

            //        if (CycleType == CYCLE.HOURLY) {

            //            Console.WriteLine("NOT FIRST RUN ============");

            //            var current = new DateTime(x.LastRunDate.Year, x.LastRunDate.Month, x.LastRunDate.Day, x.LastRunDate.Hour, 0, 0);
            //            var schour = new DateTime(TODAY.DETETIME.Year, TODAY.DETETIME.Month, TODAY.DETETIME.Day, TODAY.DETETIME.Hour, 0, 0);
            //            var diff = current - schour;

            //            if (diff.Hours >= 1) {  // 전에 돌렸던 시간과 1시간 이상 차이가 난다면
            //                Console.WriteLine("INSERT QUEUE ============");
            //                result = true;
            //                //resultQueue.Add(x);
            //                sdUP.UpdateLastRunDate(x.Id, DateTime.Now);
            //            } else {
            //                Console.WriteLine("HOURLY ALREAY RUN ============");
            //            }

            //        } else {    // 오늘 돌린 기록이 있는지 없는지

            //            var todaty = new DateTime(TODAY.DETETIME.Year, TODAY.DETETIME.Month, TODAY.DETETIME.Day);
            //            var scLastSimpleDate = new DateTime(x.LastRunDate.Year, x.LastRunDate.Month, x.LastRunDate.Day);

            //            if (todaty == scLastSimpleDate) {
            //                Console.WriteLine("TODAY ALREAY RUN ============");
            //            } else {
            //                Console.WriteLine("INSERT QUEUE ============");
            //                sdUP.UpdateLastRunDate(x.Id, DateTime.Now);
            //                result = true;
            //            }
            //        }
            //    }
            //} else {
            //    Console.WriteLine("OVER TIME NOT PASS ============");
            //}
            return result;
        }

        public void ExecuteTaskQueue(List<NotificationSchedulerVO> resultQueue) {

            resultQueue.ForEach(x => {

                try {
                    Type type = Type.GetType(TASK_NAMESPACE + x.ProgramClass);
                    if (type == null) {
                        // class 명 찾지못한경우 이메일 큐에 삽입
                        MailHelper.AddSystemIssueString(GetType().Name, "Class not found", "Cannot found class " + x.ProgramClass + " please check database");

                    } else {    // class 리플렉션으로 인스턴스 생성
                        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(k => k.GetParameters().Length == 0).First();
                        dynamic node = constructor.Invoke(null);
                        node.Execute(x);
                    }
                } catch (Exception e) {
                    MailHelper.AddSystemIssueString(GetType().Name, "Class Ececute Error", e.ToString());
                }
            });
        }
    }
}
