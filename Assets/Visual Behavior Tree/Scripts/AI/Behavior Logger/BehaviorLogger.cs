using System;
using UnityEngine;

namespace Assets.Scripts.AI.Behavior_Logger
{
    public partial class BehaviorLogger
    {
        static bool isInitialized = false;
        static bool isDebugBuild = false;

        public string Name { get; private set; }
        protected readonly Action<BehaviorLogEntry> logPublisher;

        public BehaviorLogger(string loggerName)
        {
            this.Name = loggerName;
            this.logPublisher = ObservableBehaviorLogger.RegisterLogger(this);
        }

        /// <summary>Output LogType.Log but only enabled if isDebugBuild</summary>
        public virtual void Debug(object message, UnityEngine.Object context = null)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                isDebugBuild = UnityEngine.Debug.isDebugBuild;
            }

            if (isDebugBuild)
            {
                logPublisher(new BehaviorLogEntry(
                    message: (message != null) ? message.ToString() : "",
                    logType: LogType.Log,
                    timestamp: DateTime.Now,
                    loggerName: Name,
                    context: context));
            }
        }

        /// <summary>Output LogType.Log but only enables isDebugBuild</summary>
        public virtual void DebugFormat(string format, params object[] args)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                isDebugBuild = UnityEngine.Debug.isDebugBuild;
            }

            if (isDebugBuild)
            {
                logPublisher(new BehaviorLogEntry(
                    message: (format != null) ? string.Format(format, args) : "",
                    logType: LogType.Log,
                    timestamp: DateTime.Now,
                    loggerName: Name,
                    context: null));
            }
        }

        public virtual void Log(object message, UnityEngine.Object context = null)
        {
            logPublisher(new BehaviorLogEntry(
                message: (message != null) ? message.ToString() : "",
                logType: LogType.Log,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: context));
        }

        public virtual void LogFormat(string format, params object[] args)
        {
            logPublisher(new BehaviorLogEntry(
                message: (format != null) ? string.Format(format, args) : "",
                logType: LogType.Log,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: null));
        }

        public virtual void Warning(object message, UnityEngine.Object context = null)
        {
            logPublisher(new BehaviorLogEntry(
                message: (message != null) ? message.ToString() : "",
                logType: LogType.Warning,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: context));
        }

        public virtual void WarningFormat(string format, params object[] args)
        {
            logPublisher(new BehaviorLogEntry(
                message: (format != null) ? string.Format(format, args) : "",
                logType: LogType.Warning,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: null));
        }

        public virtual void Error(object message, UnityEngine.Object context = null)
        {
            logPublisher(new BehaviorLogEntry(
                message: (message != null) ? message.ToString() : "",
                logType: LogType.Error,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: context));
        }

        public virtual void ErrorFormat(string format, params object[] args)
        {
            logPublisher(new BehaviorLogEntry(
                message: (format != null) ? string.Format(format, args) : "",
                logType: LogType.Error,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: null));
        }

        public virtual void Exception(Exception exception, UnityEngine.Object context = null)
        {
            logPublisher(new BehaviorLogEntry(
                message: (exception != null) ? exception.ToString() : "",
                exception: exception,
                logType: LogType.Exception,
                timestamp: DateTime.Now,
                loggerName: Name,
                context: context));
        }

        /// <summary>Publish raw LogEntry.</summary>
        public virtual void Raw(BehaviorLogEntry logEntry)
        {
            logPublisher(logEntry);
        }
    }
}