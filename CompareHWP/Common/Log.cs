using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CompareHWP.Common
{
    public static class Log
    {
        private static CultureInfo culture = new CultureInfo("en");
        public enum Priority : int
        {
            OFF = 200,
            DEBUG = 100,
            INFO = 75,
            WARN = 50,
            ERROR = 25,
            FATAL = 0
        }

        public enum eType : int
        {
            Seq = 0,
            IO = 1,
            Thread = 2,
            Scheduler = 3,
            Exception = 4
        }

        public static CultureInfo GetLogCulture()
        {
            return culture;
        }

        private static string logPath = null;
        public static string LogPath
        {
            get { return logPath; }
            set
            {
                logPath = value;
                Init();
            }
        }

        public class LogInfo
        {
            public string LogFullPath { get; set; }
            public TextWriter Writer { get; set; }
            public DateTime LastLogTime { get; set; }
            public Object Locker { get; set; }
            public bool FirstFlag { get; set; }
        }

        public static string LoggingLevel = "Normal";
        public readonly static ConcurrentDictionary<String, LogInfo> logInfo = new ConcurrentDictionary<String, LogInfo>();

        public static void Debug(String message)
        {
            Log.Append("[D]" + message, Priority.DEBUG);
        }

        public static string Info2(String category1, String category2, String message, String typeName, bool includeBase = true, bool includeBaseHierarchy = false, bool bPublish = false)
        {
            StringBuilder resultMessage = new StringBuilder();

            string baseMethod1 = null;
            string baseMethod2 = null;
            string finalMessage = null;

            try
            {
                StackTrace stackTrace = null;

                if (includeBase || includeBaseHierarchy)
                    stackTrace = new StackTrace();

                if (includeBase)
                {
                    StackFrame stackFrame = stackTrace.GetFrame(1);
                    MethodBase methodBase = stackFrame.GetMethod();

                    baseMethod1 = String.Format("{0}.{1}", methodBase.DeclaringType.Name, methodBase.Name);
                }

                if (includeBaseHierarchy)
                {
                    StackFrame[] stackFrames = stackTrace.GetFrames();

                    int count = 0;
                    foreach (var frame in stackFrames.Skip(2).Take(2))
                    {
                        MethodBase methodBase = frame.GetMethod();

                        if (methodBase.DeclaringType.Name == "ThreadHelper"
                            || methodBase.DeclaringType.Name == "ExecutionContext")
                            continue;

                        baseMethod2 += String.Format("<{0}> {1}.{2} ", count++, methodBase.DeclaringType.Name, methodBase.Name);
                    }
                }

                if (category1 != null)
                    finalMessage += String.Format("[{0}]", category1);

                if (category2 != null)
                    finalMessage += String.Format("[{0}]", category2);

                finalMessage += message;

                if (includeBase || includeBaseHierarchy)
                {
                    string tempMessage = null;

                    if (includeBase)
                        tempMessage = String.Format("at {0} ", baseMethod1);

                    if (includeBaseHierarchy)
                        tempMessage += String.Format("from {0}", baseMethod2);

                    finalMessage += String.Format("( {0})", tempMessage);
                }

                String resultLog = Log.Append(String.Format("{0}", finalMessage), Priority.INFO, typeName);

                resultMessage.Append(resultLog);


                //if (bPublish == true)
                //{
                //    NetMQHelper.Instance.Publish(message);
                //}
            }
            catch
            {
            }

            return resultMessage.ToString();
        }

        public static void Info(String message, string type = null, bool isLogBase = true, bool bPublish = false)
        {
            if (type == null)
                type = eType.Seq.ToString();

            try
            {
                if (isLogBase)
                {
                    StackTrace stackTrace = new StackTrace(true);
                    StackFrame stackFrame = stackTrace.GetFrame(1);
                    MethodBase methodBase = stackFrame.GetMethod();
                    var fullPath = stackFrame.GetFileName();
                    var fileName = fullPath is null ? "Unknown fullPath" : System.IO.Path.GetFileName(fullPath);
                    var lineNumber = stackFrame.GetFileLineNumber();

                    //  로그 추적을 위해 파일명과 라인줄 수도 남김
                    Log.Append($"[I][{methodBase.DeclaringType.Name}.{methodBase.Name}({fileName ?? "Unknown FileName"}:{lineNumber})]\n{message}\n", Priority.INFO, type);

                    return;
                }
            }
            catch
            {
            }
            Log.Append(String.Format("[I]{0}", message), Priority.INFO, type.ToString());
        }

        public static void Warn(String message, bool bPublish = false)
        {
            Log.Append("[W]" + message, Priority.WARN);

            //if (bPublish == true)
            //{
            //    NetMQHelper.Instance.Publish(message);
            //}

        }

        public static string Error(String message, string type = null, bool isLogBase = true, string messageToAdd = null, bool bPublish = false)
        {
            if (type == null)
                type = eType.Seq.ToString();

            if (messageToAdd != null)
                messageToAdd += string.Format("{0}{1}", messageToAdd == "" ? "" : Environment.NewLine, message);

            try
            {
                if (isLogBase)
                {
                    StackTrace stackTrace = new StackTrace();
                    StackFrame stackFrame = stackTrace.GetFrame(1);
                    MethodBase methodBase = stackFrame.GetMethod();

                    Log.Append(String.Format("[E][{1}.{2}]{0}", message, methodBase.DeclaringType.Name, methodBase.Name), Priority.ERROR, type);


                    //if (bPublish == true)
                    //{
                    //    NetMQHelper.Instance.Publish(message);
                    //}

                    return messageToAdd;
                }
            }
            catch
            {
            }
            Log.Append(String.Format("[E]{0}", message), Priority.ERROR, type.ToString());
            return messageToAdd;
        }

        public static string Error(Exception ex, string userMessage = null, string type = null, string messageToAdd = null, bool bPublish = false)
        {
            if (type == null)
                type = eType.Seq.ToString();

            if (messageToAdd != null)
                messageToAdd += string.Format("{0}{1}", messageToAdd == "" ? "" : Environment.NewLine, ex.ToString());

            string message = ex.ToString();
            if (userMessage != null)
                message += "\r\nUserMessage : " + userMessage;

            Log.Append("[E]" + message, Priority.ERROR, type);


            //if (bPublish == true)
            //{
            //    NetMQHelper.Instance.Publish(message);
            //}

            return messageToAdd;
        }

        public static void Fatal(String message, bool bPublish = false)
        {
            Log.Append("[F]" + message, Priority.FATAL);

            //if (bPublish == true)
            //{
            //    NetMQHelper.Instance.Publish(message);
            //}
        }

        // constructor for static resources
        public static void Init()
        {
            if (string.IsNullOrWhiteSpace(LogPath))
                LogPath = GetBaseDirectory(@"LogFolder");

            if (!String.IsNullOrWhiteSpace(LogPath))
                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);
        }

        private static void CreateLogFile(String type, LogInfo info = null)
        {
            var realPath = GetRealLogPath(type);

            // if the file doesn't exist, create it
            try
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(realPath)))
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(realPath));

                if (!File.Exists(realPath))
                {
                    using (FileStream fs = File.Create(realPath))
                    {
                        fs.Close();
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Log file create error in CreateLogFile()");
                throw new Exception();
            }

            if (info == null)
            {
                if (!logInfo.ContainsKey(type))
                {
                    var now = DateTime.Now;

                    logInfo.TryAdd(type, new LogInfo()
                    {
                        LastLogTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0),
                        Locker = new object(),
                        LogFullPath = realPath,
                        Writer = File.AppendText(realPath),
                        FirstFlag = true
                    });
                }
            }
            else
            {
                try
                {
                    info.LogFullPath = realPath;
                    info.Writer = File.AppendText(realPath);
                    info.FirstFlag = true;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Log writing error in CreateLogFile()");
                }
            }
        }

        public static void Delete(String type = "Seq")
        {
            try
            {
                LogInfo info = logInfo[type];
                info.Writer.Close();
                File.Delete(info.LogFullPath);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Log file delete error");
            }
        }

        public static string Append(String message, Priority level, String type = "Seq")
        {
            StringBuilder resultMessage = new StringBuilder();

            int logLevel = (int)Priority.OFF;
            // find out what logLevel we're currently at, default to 0
            String strLogLevel = LoggingLevel.ToUpper() == "NORMAL" ? "INFO" : "DEBUG";

            switch (strLogLevel)
            {
                case "DEBUG":
                    logLevel = (int)Priority.DEBUG;
                    break;

                case "INFO":
                    logLevel = (int)Priority.INFO;
                    break;

                case "WARN":
                    logLevel = (int)Priority.WARN;
                    break;

                case "ERROR":
                    logLevel = (int)Priority.ERROR;
                    break;

                case "FATAL":
                    logLevel = (int)Priority.FATAL;
                    break;

                default:
                    logLevel = (int)Priority.OFF;
                    break;
            }

            // if this message has a priority greater than or equal to the current logging level, log it
            if (logLevel >= (int)level)
            {
                LogInfo info = null;
                try
                {
                    info = logInfo[type];
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("{0} log type is first time", type));

                    try
                    {
                        CreateLogFile(type);
                        info = logInfo[type];
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Log file create error");
                    }
                }
                if (info != null)
                {
                    lock (info.Locker)
                    {
                        DateTime now = DateTime.Now;
                        var nowHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                        string nowString = now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                        System.Diagnostics.Debug.WriteLine("{0} : {1}", nowString, message);

                        resultMessage.Append($"[{nowString}]{message}");

                        try
                        {
                            if (info.LastLogTime != nowHour)
                            {
                                CreateLogFile(type, info);
                            }

                            info.LastLogTime = nowHour;

                            if (info.FirstFlag)
                            {
                                info.Writer.WriteLine("[{0}]{1}", nowString, "************* New log instance intialized *************");
                                info.FirstFlag = false;
                            }

                            info.Writer.WriteLine("[{0}]{1}", nowString, message);
                            info.Writer.Flush();
                        }
                        catch
                        {
                            CreateLogFile(type, info);
                            System.Diagnostics.Debug.WriteLine("Log writing error");
                        }
                    }
                }
            }

            return resultMessage.ToString();
        }

        private static string GetRealLogPath(String type)
        {
            if (string.IsNullOrWhiteSpace(LogPath))
                LogPath = GetBaseDirectory(@"LogFolder");

            var now = DateTime.Now;

            return System.IO.Path.Combine(LogPath, now.ToString("yyyyMM"), now.ToString("yyyyMMdd"), String.Format("{0}{1}.txt", type, now.ToString("yyyyMMddHH", GetLogCulture())));
        }

        public static string GetLogPath(String type, bool createIfNotExist = true)
        {
            string path = GetRealLogPath(type);
            if (createIfNotExist)
                if (!File.Exists(path))
                    CreateLogFile(type);

            return path;
        }

        /// <summary>
        /// 지정된 날짜가 지난 폴더와 하위폴더의 모든 데이터를 삭제
        /// 사용법 : DeleteLogs(@"C:\1.IntiPharmLogFolder", 15, true, true);
        /// </summary>
        /// <param name="folderDir"></param>
        /// <param name="LimitedDay"></param>
        /// <param name="ByLastWritedTime"></param>
        /// <param name="bRecursive"></param>
        public static void DeleteLogs(string folderDir, int LimitedDay, bool ByLastWritedTime = true, bool bRecursive = true)
        {
            // 폴더내에 존재하는 파일 삭제
            DeleteFiles(folderDir, LimitedDay, ByLastWritedTime);

            // 폴더내에 존재하는 햐위 폴더 삭제
            DeleteFolder(folderDir, LimitedDay, ByLastWritedTime, bRecursive);
        }

        /// <summary>
        /// 주의 : 지정될 폴더내에 있는 하위폴더는 검사없이 삭제해버리므로 주의할 것. 다만 bRecursive를 false로 하면 하위폴더 삭제하지 않음
        /// 주의 : 날짜로 검사하는데 폴더 생성날짜 기준이므로 폴더 내부 파일은 검사하지 않고 폴더 날짜가 지나면 내부파일까지 무조건 삭제함. 
        /// 예외발생시에만 false가 반환됨. 그 외엔 삭제여부와 상관없이 true 반환
        /// </summary>
        /// <param name="folderDir"></param>
        /// <param name="LimitedDay"></param>
        /// <param name="ByLastWritedTime"></param>
        /// <param name="bRecursive"></param>
        /// <returns></returns>
        public static bool DeleteFolder(string folderDir, int LimitedDay, bool ByLastWritedTime = true, bool bRecursive = true)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(folderDir);

                if (di.Exists)
                {
                    DirectoryInfo[] dirInfo = di.GetDirectories();
                    string LimitedDate = DateTime.Today.AddDays(-LimitedDay).ToString("yyyyMMdd");

                    foreach (DirectoryInfo dir in dirInfo)
                    {
                        if (LimitedDate.CompareTo(ByLastWritedTime == true ? dir.LastWriteTime.ToString("yyyyMMdd") : dir.CreationTime.ToString("yyyyMMdd")) > 0)
                        {
                            //dir.Attributes = FileAttributes.Normal; // 로그파일은 항상 일반이므로 속성 변경 필요 없음
                            dir.Delete(bRecursive);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// 대상 폴더안에 파일만 삭제함. 대상 폴더에 하위 폴더가 존재하더라도 하위 폴더안에 파일은 삭제하지 않음
        /// 예외 발생시에만 false 반환됨. 그 외엔 삭제여부와 상관없이 true 반환
        /// </summary>
        /// <param name="folderDir"></param>
        /// <param name="LimitedDay"></param>
        /// <param name="ByLastWritedTime"></param>
        /// <returns></returns>
        public static bool DeleteFiles(string folderDir, int LimitedDay, bool ByLastWritedTime = true)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(folderDir);

                if (di.Exists)
                {
                    FileInfo[] fileInfo = di.GetFiles();
                    string LimitedDate = DateTime.Today.AddDays(-LimitedDay).ToString("yyyyMMdd");

                    foreach (FileInfo file in fileInfo)
                    {
                        if (LimitedDate.CompareTo(ByLastWritedTime == true ? file.LastWriteTime.ToString("yyyyMMdd") : file.CreationTime.ToString("yyyyMMdd")) > 0)
                        {
                            file.Delete();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public static string CheckDrivePath(string logPath)
        {
            //  D 드라이브가 없는 경우 C 드라이브로 교체
            if (!Directory.GetLogicalDrives().Contains(System.IO.Path.GetPathRoot(logPath)))
            {
                logPath = logPath.Replace(System.IO.Path.GetPathRoot(logPath), @"C:\");
                LogPath = logPath;
            }

            return LogPath;

        }

        private static string GetBaseDirectory(string customDirectory)
        {
            var baseDir = System.IO.Path.GetPathRoot(customDirectory);

            if (baseDir.StartsWith(@"\") == false)
                baseDir = @"C:\";

            return System.IO.Path.Combine(baseDir, customDirectory);
        }
    }
}
