using System;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Behavior_Logger
{
    public class ObservableBehaviorLogger : IObservable<BehaviorLogEntry>
    {
        static readonly Subject<BehaviorLogEntry> logPublisher = new Subject<BehaviorLogEntry>();

        public static readonly ObservableBehaviorLogger Listener = new ObservableBehaviorLogger();

        private ObservableBehaviorLogger()
        { }

        public static Action<BehaviorLogEntry> RegisterLogger(BehaviorLogger logger)
        {
            if (logger.Name == null) throw new ArgumentNullException("logger.Name is null");

            return logPublisher.OnNext;
        }

        public IDisposable Subscribe(IObserver<BehaviorLogEntry> observer)
        {
            return logPublisher.Subscribe(observer);
        }
    }
}