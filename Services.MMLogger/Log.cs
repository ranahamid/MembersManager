using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.MMLogger
{
    public static class Log
    {
        private static Logger logger = LogManager.GetLogger("NBI");

        public class Context
        {
            private string context;

            public Context(string context)
            {
                this.context = context;
            }

            public override string ToString()
            {
                return context;
            }
        }

        public static void Init(string baseFolder, LogLevel level)
        {
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget fileTarget = new FileTarget("file");
            fileTarget.FileName = baseFolder + "/${shortdate}_" + RoundUp(DateTime.Now, TimeSpan.FromMinutes(60)).ToString("HH_mm") + ".log";
            fileTarget.Layout = "${longdate} ${uppercase:${level}} ${message}";
            fileTarget.CreateDirs = true;

            config.AddTarget(fileTarget);
            config.LoggingRules.Add(new LoggingRule("*", level, fileTarget));

            LogManager.Configuration = config;
        }

        public static LogLevel ToLogLevel(string level)
        {
            return level == "Info" ? LogLevel.Info :
                   level == "Warn" ? LogLevel.Warn :
                   level == "Error" ? LogLevel.Error : LogLevel.Debug;
        }

        public static Context GetContext([CallerMemberName] string caller = null, [CallerFilePath] string path = null)
        {
            var startIndex = path.LastIndexOf('\\');
            return new Context(string.Concat(path.Substring(startIndex + 1, path.LastIndexOf(".") - startIndex), caller));
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }

        public static void Error(Exception exception, string message = null)
        {
            logger.Error(exception, message);
        }

        public static void Error(Exception exception, string message, params object[] parameters)
        {
            logger.Error(exception, message, parameters);
        }

        public static void Error(Exception exception, Context context, string message = null)
        {
            logger.Error(exception, string.Concat(context, ": ", message));
        }

        public static void Error(Exception exception, Context context, string message, params object[] parameters)
        {
            logger.Error(exception, string.Concat(context, ": ", message), parameters);
        }

        public static void Warn(string message)
        {
            logger.Warn(message);
        }

        public static void Warn(string message, params object[] parameters)
        {
            logger.Warn(message, parameters);
        }

        public static void Warn(Context context, string message)
        {
            logger.Warn(string.Concat(context, ": ", message));
        }

        public static void Warn(Context context, string message, params object[] parameters)
        {
            logger.Warn(string.Concat(context, ": ", message), parameters);
        }

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Info(string message, params object[] parameters)
        {
            logger.Info(message, parameters);
        }

        public static void Info(Context context, string message)
        {
            logger.Info(string.Concat(context, ": ", message));
        }

        public static void Info(Context context, string message, params object[] parameters)
        {
            logger.Info(string.Concat(context, ": ", message), parameters);
        }

        public static void Debug(string message)
        {
            logger.Debug(message);
        }

        public static void Debug(string message, params object[] parameters)
        {
            logger.Debug(message, parameters);
        }

        public static void Debug(Context context, string message)
        {
            logger.Debug(string.Concat(context, ": ", message));
        }

        public static void Debug(Context context, string message, params object[] parameters)
        {
            logger.Debug(string.Concat(context, ": ", message), parameters);
        }
    }

}
