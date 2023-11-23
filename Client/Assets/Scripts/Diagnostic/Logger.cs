using System;
using Resux.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Resux
{
    /// <summary>
    /// debug日志类型
    /// </summary>
    public enum LogType
    {
        Normal,
        Warning,
        Error
    }

    public class LogItem
    {
        public string content;

        public LogType type;

        public LogItem(string content, LogType type)
        {
            this.content = content;
            this.type = type;
        }
    }

    /// <summary>
    /// debug类封装
    /// </summary>
    public static class Logger
    {
        #region properties

        // 临时日志存储路径
        //use bugly plugin to save log
        static string tempNormalLogPath => Assets.AssetsFilePathDM.AllLogsPath;
        static string tempErrorLogPath => Assets.AssetsFilePathDM.ErrorLogsPath;
        // 设备信息
        static string deviceMessage;

        // 日志最大容量
        private static int maxLogCount;

        private static List<LogItem> errorLogs;

        public static List<LogItem> ErrorLogs
        {
            get
            {
                if (errorLogs == null)
                {
                    errorLogs = new List<LogItem>();
                }

                return errorLogs;
            }
        }

        private static List<LogItem> normalLogs;

        public static List<LogItem> NormalLogs
        {
            get
            {
                if (normalLogs == null)
                {
                    normalLogs = new List<LogItem>();
                }

                return normalLogs;
            }
        }

        private const string IndicatorColor = "teal";
        
        private static readonly UnityLogHandler LogHandler;

        private static string LogBufferPath => Assets.AssetsFilePathDM.LogBufferPath;

        /// <summary>
        /// 本次会话的日志，关闭游戏后就无。
        /// 每次App Center上传一次也清空
        /// </summary>
        private static List<string> localLogBuffer = new List<string>();

        private static uint uploadPart = 0;
        #endregion

        static Logger()
        {
            deviceMessage = $"型号：{SystemInfo.deviceModel}\n操作系统：{SystemInfo.operatingSystem}\n系统内存：{SystemInfo.systemMemorySize}";
            maxLogCount = 8192;

            localLogBuffer.Add(deviceMessage);

            LogHandler = new UnityLogHandler();
            LogHandler.OnException += (e) =>
            {
                StringBuilder builder = new StringBuilder();

                uploadPart++;
                builder.AppendLine("LOG: This is Part " + uploadPart);

                for (int i = localLogBuffer.Count - 1; i >= 0; i--)
                {
                    builder.AppendLine(localLogBuffer[i]);
                    builder.Append(i);
                    builder.AppendLine("------------- LOG ITEM -------------");
                }

                builder.Clear();
                localLogBuffer.Clear();
            };
            Debug.unityLogger.logHandler = LogHandler;
        }

        #region Public Method

        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="obj"></param>
#if !UNITY_EDITOR
        [System.Diagnostics.Conditional("ENABLE_LOG")]
#endif
        public static void Log(object obj,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0, [CallerMemberName] string methodName = null)
        {
            var str = $"<color=white>[Normal]</color>{obj}";
            AppendIndicator(ref str, callerFile, callerLine, methodName);
            var debug = new LogItem(str, LogType.Normal);
            AddToLog(debug);
            Debug.Log(str);
        }

#if !UNITY_EDITOR
        [System.Diagnostics.Conditional("ENABLE_LOG")]
#endif
        public static void Log(object obj, Color color,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0, [CallerMemberName] string methodName = null)
        {
            var colorStr = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{obj}</color>";
            AppendIndicator(ref colorStr, callerFile, callerLine, methodName);
            var debug = new LogItem(colorStr, LogType.Normal);
            AddToLog(debug);
            Debug.Log(colorStr);
        }

        /// <summary>
        /// warning日志
        /// </summary>
        /// <param name="obj"></param>
#if !UNITY_EDITOR
        [System.Diagnostics.Conditional("ENABLE_LOG")]
#endif
        public static void LogWarning(object obj,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0, [CallerMemberName] string methodName = null)
        {
            var str = $"<color=yellow>[Warning]</color>{obj}";
            AppendIndicator(ref str, callerFile, callerLine, methodName);
            var debug = new LogItem(str, LogType.Warning);
            AddToLog(debug);
            Debug.LogWarning(str);
        }

        /// <summary>
        /// error日志
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="showPopup">显示错误弹窗</param>
#if !UNITY_EDITOR
        [System.Diagnostics.Conditional("ENABLE_LOG")]
#endif
        public static void LogError(object obj, bool showPopup = false, UnityAction onCancel = null, UnityAction onOk = null,
            [CallerFilePath] string callerFile = null, [CallerLineNumber] int callerLine = 0, [CallerMemberName] string methodName = null)
        {
            var str = $"<color=red>[Error]</color>{obj}";
            AppendIndicator(ref str, callerFile, callerLine, methodName);
            var debug = new LogItem(str, LogType.Error);
            AddToLog(debug);
            Debug.LogError(str);
            if (showPopup)
            {
                // 显示弹窗
                PopupView.Instance.ShowErrorWindow(obj.ToString(), onCancel: onCancel, onOk: onOk);
            }
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="e">异常信息</param>
#if !UNITY_EDITOR
        [System.Diagnostics.Conditional("ENABLE_LOG")]
#endif
        public static void LogException(Exception e)
        {
            LogError($"{e.Message}: {e.StackTrace}");
        }

        /// <summary>
        /// 保存全部日志
        /// </summary>
        public static void SaveLog()
        {
            SaveNormalLog();
            SaveErrorLog();
        }

        #endregion

        #region Private Method

        private static void AppendIndicator(ref string text, string callerFile, int callerLine, string methodName)
        {
            text += $"\n<color={IndicatorColor}><b>{methodName}(...)</b> in {callerFile}:{callerLine}</color>";
        }

        /// <summary>
        /// 保存全部日志到对应文件
        /// </summary>
        private static void SaveNormalLog()
        {
            var sb = new StringBuilder();
            if (!File.Exists(tempNormalLogPath))
            {
                File.Create(tempNormalLogPath).Close();
                sb.AppendLine(deviceMessage);
                sb.AppendLine("********************");
            }

            foreach (var log in NormalLogs)
            {
                sb.AppendLine(log.content);
            }

            StreamWriter sw = new StreamWriter(tempNormalLogPath, true);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();

            
        }

        /// <summary>
        /// 保存error日志到对应文件
        /// </summary>
        private static void SaveErrorLog()
        {
            var sb = new StringBuilder();
            if (!File.Exists(tempErrorLogPath))
            {
                File.Create(tempErrorLogPath).Close();
                sb.AppendLine(deviceMessage);
                sb.AppendLine("********************");
            }

            foreach (var log in ErrorLogs)
            {
                sb.AppendLine(log.content);
            }
            StreamWriter sw = new StreamWriter(tempErrorLogPath, true);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 添加debug信息到日志
        /// </summary>
        /// <param name="debug">debug信息</param>
        private static void AddToLog(LogItem debug)
        {
            localLogBuffer.Add(debug.content);
            NormalLogs.Add(debug);
            if (debug.type == LogType.Error)
            {
                ErrorLogs.Add(debug);
            }

            // 检查容量
            if (NormalLogs.Count >= maxLogCount)
            {
                SaveNormalLog();
                NormalLogs.Clear();
            }

            if (ErrorLogs.Count >= maxLogCount)
            {
                SaveErrorLog();
                ErrorLogs.Clear();
            }
        }
        #endregion

        #region Nested Class
        public class UnityLogHandler : ILogHandler
        {
            private ILogHandler lastLogHandler;
            public event Action<Exception> OnException = null;
            public UnityLogHandler()
            {
                lastLogHandler = Debug.unityLogger.logHandler;
            }
            void ILogHandler.LogFormat(UnityEngine.LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                lastLogHandler.LogFormat(logType, context, format, args);
            }

            void ILogHandler.LogException(Exception exception, UnityEngine.Object context)
            {
                lastLogHandler.LogException(exception, context);
                OnException?.Invoke(exception);
            }
        }
        #endregion
    }
}