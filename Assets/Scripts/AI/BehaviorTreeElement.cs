using Assets.Scripts.AI.Behavior_Logger;
using Assets.Scripts.AI.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI
{
    [Serializable]
    public abstract class BehaviorTreeElement : TreeElement, IDisposable
    {
        //used for JSON saving/loading
        public string ElementType { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual bool CanHaveChildren { get => true; }

        [Newtonsoft.Json.JsonIgnore]
        public MonoBehaviour Manager { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public BehaviorLogger DebugLogger { get; protected set; }

        [Newtonsoft.Json.JsonIgnore]
        private BehaviorState _currentState;
        [Newtonsoft.Json.JsonIgnore]
        public BehaviorState CurrentState { get => _currentState; set => _currentState = value; }

        [Newtonsoft.Json.JsonIgnore]
        protected bool Initialized = false;

        public BehaviorTreeElement(string name,int depth, int id) 
            : base(name, depth, id)
        {
            ElementType = this.GetType().ToString();
            CurrentState = BehaviorState.Null;
            if(CanHaveChildren)
                Children = new List<TreeElement>();
        }

        protected Action<BehaviorState> DebugLogAction;

        /// <summary>
        /// The primary method of action
        /// </summary>
        /// <returns>observable stream of states from this behavior</returns>
        public abstract IObservable<BehaviorState> Start();

        public virtual void Initialize()
        {
            Debug.Log("Initializing " + this.ElementType);

            DebugLogger = new BehaviorLogger(Manager.name + " | " + Name);

            if (Initialized) return;
            if(CanHaveChildren)
            {
                var allChildrenToRun = from x in Children
                                       select x as BehaviorTreeElement;

                foreach (var ch in allChildrenToRun)
                {
                    ch.Manager = this.Manager;
                    ch.Initialize();
                }
            }

            DebugLogAction = (debugState) =>
            {
                var logEntry = new BehaviorLogEntry(
                   loggerName: DebugLogger.Name,
                   logType: LogType.Log,
                   timestamp: DateTime.Now,
                   message: "Ticked!",
                   behaviorID: this.ID,
                   newState: debugState,
                   context: this);
                DebugLogger.Raw(logEntry);
            };

            Initialized = true;
        }

        public override string ToString()
        {
            var depthPad = "";
            for (int d = 0; d < this.Depth +1; ++d)
            {
                depthPad += "     ";
            }
            var retString = depthPad + "ID: " + ID + "\n" +
                            depthPad + "Name: " + this.Name + "\n" +
                            depthPad + "Depth: " + Depth + "\n" +
                            depthPad + "Type: " + ElementType.ToString() + "\n" +
                            depthPad + "NumChildren: " + (HasChildren ? Children.Count : 0) + "\n";

            if (Children != null)
            {
                retString += depthPad + "Children: \n";
                foreach (var child in Children)
                {
                    retString += child.ToString();
                }
            }
            return retString;

        }

        #region IDisposable Support

        // CompositeDisposable is similar with List<IDisposable>, manage multiple IDisposable
        [NonSerialized]
        protected CompositeDisposable Disposables = new CompositeDisposable(); // field
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disposables.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Children.Clear();
                Children = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}