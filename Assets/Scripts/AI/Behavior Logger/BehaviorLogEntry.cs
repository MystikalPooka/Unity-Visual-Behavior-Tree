using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AI.Behavior_Logger
{
    public struct BehaviorLogEntry
    {
        // requires
        public string LoggerName { get; private set; }
        public LogType LogType { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        // options
        /// <summary>
        /// [Optional] Used to keep track of frames for debugging.
        /// </summary>
        public int TickNumber { get; private set; }

        public BehaviorState NewState { get; private set; }

        /// <summary>[Optional]</summary>
        public BehaviorTreeElement Context { get; private set; }
        /// <summary>[Optional]</summary>
        public Exception Exception { get; private set; }
        /// <summary>[Optional]</summary>
        public string StackTrace { get; private set; }
        /// <summary>[Optional]</summary>
        public object State { get; private set; }

        public BehaviorLogEntry(string loggerName, LogType logType, DateTime timestamp, string message, BehaviorState newState = BehaviorState.Null, int ticknum = -1,
            BehaviorTreeElement context = null, Exception exception = null, string stackTrace = null, object state = null)
            : this()
        {
            this.LoggerName = loggerName;
            this.LogType = logType;
            this.Timestamp = timestamp;
            this.Message = message;
            this.NewState = newState;
            this.TickNumber = ticknum;
            this.Context = context;
            this.Exception = exception;
            this.StackTrace = stackTrace;
            this.State = state;
        }

        public override string ToString()
        {
            var plusEx = (Exception != null) ? (Environment.NewLine + Exception.ToString()) : "";
            return "[" + Timestamp.ToString() + "]"
                + "[" + LoggerName + "]"
                + "[" + LogType.ToString() + "]"
                + Message
                + plusEx;
        }
    }
}
