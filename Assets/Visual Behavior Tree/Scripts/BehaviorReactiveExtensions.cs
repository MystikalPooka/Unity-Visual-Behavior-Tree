using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Assets.Visual_Behavior_Tree.Scripts
{
    public static class BehaviorReactiveExtensions
    {
        public static IObservable<R> Publish<T, R>(
            this IObservable<T> source, Func<IObservable<T>, IObservable<R>> selector)
        {
            return Observable.CreateSafe((IObserver<R> observer) =>
            {
                var s = source.Publish();
                var p = selector(s).Subscribe(observer);
                return new CompositeDisposable(p, s.Connect());
            });
        }

        public static IObservable<T> TakeWhileInclusive<T>(
            this IObservable<T> source, Func<T, bool> predicate)
        {
            return source.Publish(co => co.TakeWhile(predicate)
                                          .Merge(co.SkipWhile(predicate)
                                          .Take(1)));
        }

        public static IObservable<bool> Any<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return source.Where(predicate).Select(e => true).Concat(Observable.Return(false)).Take(1);
        }
    }
}
