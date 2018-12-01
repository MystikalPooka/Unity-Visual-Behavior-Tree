using System;
using UniRx;

namespace Assets.Scripts.AI.Behavior_Logger
{
    public static class BehaviorDebugExtensions
    {
        public static IObservable<T> Debug<T>(this IObservable<T> source, BehaviorLogger logger)
        {
#if DEBUG
            return source.Materialize()
                .Do(x => logger.Debug(x.ToString()))
                .Dematerialize()
                .DoOnCancel(() => logger.Debug("OnCancel"))
                .DoOnSubscribe(() => logger.Debug("OnSubscribe"));
#else
            return source;
#endif
        }

    }
}
