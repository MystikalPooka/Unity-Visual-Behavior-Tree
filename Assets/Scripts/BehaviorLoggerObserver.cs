using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class BehaviorLogMessage : ILoggerMessage
    {
    }

    class BehaviorLoggerObserver : MonoBehaviour, ILoggerObserver<BehaviorLogMessage>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(BehaviorLogMessage value)
        {
            throw new NotImplementedException();
        }
    }
}
