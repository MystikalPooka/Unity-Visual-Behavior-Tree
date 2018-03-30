using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    interface ILoggerMessage
    { }

    internal interface ILoggerObserver<T> : IObserver<T> where T : ILoggerMessage {}
}
