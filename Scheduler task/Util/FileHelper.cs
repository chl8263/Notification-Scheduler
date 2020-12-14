using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;


namespace Scheduler_task.Util {
    class FileHelper<T> {

        private readonly string CARRIAGE_RETURN = "\r\n";
        private readonly string ROOT_PATH = Properties.Settings.Default.Rootpath;
        private readonly string STAGE_ROOT_PATH = Properties.Settings.Default.StageRootpath;
        private readonly string COMPLETE_ROOT_PATH = Properties.Settings.Default.CompleteRootpath;
        private readonly string currentShotDate = DateTime.Now.ToString("yyMM");

        public FileInfo CreateCvsFileAtStage(List<T> itemList, string fileName) {

            var fileInfoVO = new FileInfo();
            fileInfoVO.dataCount = itemList.Count;

            if (itemList.Count == 0) {
                fileInfoVO.filePath = FileInfo.nil;
                fileInfoVO.fileName = fileName;
                return fileInfoVO;
            }

            var pullPath = "";
            var realFileName = "";

            if (makeDirectory(STAGE_ROOT_PATH, currentShotDate)) {

                pullPath = Path.Combine(STAGE_ROOT_PATH, currentShotDate);
                fileName = fileName + "_" + TODAY.DETETIME;
                realFileName = fileName + ".csv";

                var fileInfo = new FileInfo(Path.Combine(pullPath, realFileName));

                if (fileInfo.Exists) {
                    var random = new Random();
                    int extendName = random.Next(0000, 9999);

                    realFileName = fileName + "_" + extendName + ".csv";
                }

                using (FileStream fs = new FileStream(Path.Combine(pullPath, realFileName), FileMode.OpenOrCreate, FileAccess.Write)) {

                    using (StreamWriter sw = new StreamWriter(fs)) {
                        sw.NewLine = CARRIAGE_RETURN;

                        try {
                            if (itemList.Count > 0) {
                                FieldInfo[] fields = itemList[0].GetType().GetFields();

                                StringBuilder sb = new StringBuilder();

                                for (int i = 0; i < fields.Length; i++) {

                                    sb.Append(fields[i].Name);

                                    if (i == fields.Length - 1) {
                                        sb.Append(CARRIAGE_RETURN);
                                    } else {
                                        sb.Append(",");
                                    }
                                }

                                for (int i = 0; i < itemList.Count; i++) {
                                    for (int j = 0; j < fields.Length; j++) {

                                        FieldInfo info = itemList[i].GetType().GetField(fields[j].Name);

                                        string temp = info.GetValue(itemList[i]).ToString().Replace(CARRIAGE_RETURN, "");

                                        if (temp.Contains(",")) {
                                            temp = temp.Insert(temp.Length, "\u0022");
                                            temp = temp.Insert(0, "\u0022");
                                        }
                                        sb.Append(temp);
                                        //sb.Append(info.GetValue(itemList[i]).ToString().Replace(CARRIAGE_RETURN, ""));

                                        if (j == fields.Length - 1) {
                                            sb.Append(CARRIAGE_RETURN);
                                        } else {
                                            sb.Append(",");
                                        }
                                    }
                                }
                                sw.WriteLine(sb.ToString());
                            }
                        } catch (System.IO.IOException e) {
                            fileName = "";
                            MailHelper.AddSystemIssueString(GetType().Name, "File Create error", e.ToString());
                        }
                    }
                }
            } else MailHelper.AddSystemIssueString(GetType().Name, MailString.FILE_CREATE_ERROR_SUBJECT, MailString.FILE_CREATE_ERROR_BODY + STAGE_ROOT_PATH + currentShotDate);

            fileInfoVO.filePath = pullPath;
            fileInfoVO.fileName = realFileName;

            return fileInfoVO;
        }

        public void moveCompleteDirectory(CreatedFileInfoVO fileInfoVO) {

            if (fileInfoVO.filePath == CreatedFileInfoVO.nil) return;

            if (makeDirectory(COMPLETE_ROOT_PATH, currentShotDate)) {

                GC.Collect();   // OS can access file

                FileInfo destFile = new FileInfo(Path.Combine(STAGE_ROOT_PATH, currentShotDate, fileInfoVO.fileName));

                if (destFile.Exists) {

                    var fileInfo = new FileInfo(Path.Combine(COMPLETE_ROOT_PATH, currentShotDate, fileInfoVO.fileName));

                    if (fileInfo.Exists) {
                        var random = new Random();
                        int extendName = random.Next(0000, 9999);

                        fileInfoVO.fileName = fileInfoVO.fileName.Replace(".csv", "") + "_" + extendName + ".csv";
                    }

                    try {
                        destFile.MoveTo(Path.Combine(COMPLETE_ROOT_PATH, currentShotDate, fileInfoVO.fileName));
                    } catch (Exception e) {
                        MailHelper.AddSystemIssueString(GetType().Name, "File Move error", e.ToString());
                    }
                }

            } else MailHelper.AddSystemIssueString(GetType().Name, MailString.FILE_CREATE_ERROR_SUBJECT, MailString.FILE_CREATE_ERROR_BODY + COMPLETE_ROOT_PATH + currentShotDate);
        }

        private bool makeDirectory(string path, string directoryName) {

            try {
                string newPath = Path.Combine(path, directoryName);
                DirectoryInfo di = new DirectoryInfo(newPath);

                if (di.Exists == false) di.Create();

                return true;
            } catch (Exception e) {
                MailHelper.AddSystemIssueString(GetType().Name, "Creat directory error", e.ToString());
                return false;
            }
        }
    }
}
