using Assets.Scripts.AI.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI
{
    [Serializable]
    public class BehaviorTreeElement : TreeElement, IDisposable
    {
        protected static readonly UniRx.Diagnostics.Logger BehaviorLogger = new UniRx.Diagnostics.Logger("Behavior Debugger");
        public string ElementType { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [SerializeField]
        private BehaviorManager _BehaviorTreeManager;
        [Newtonsoft.Json.JsonIgnore]
        public BehaviorManager BehaviorTreeManager
        {
            get
            {
                return _BehaviorTreeManager;
            }

            set
            {
                _BehaviorTreeManager = value;
            }
        }

        public BehaviorTreeElement(string name, int depth, int id) 
            : base(name, depth, id)
        {
            ElementType = this.GetType().ToString();
            CurrentState = (BehaviorState.Null);
            Children = new List<TreeElement>();
        }


        [Newtonsoft.Json.JsonIgnore]
        public BehaviorState CurrentState;

        public bool Initialized = false;
        public virtual IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            if (!Initialized) Initialize();
            if (delayStart != null)
            {
                yield return delayStart;
            }
        }

        public virtual void Initialize()
        {
            var allChildrenToRun = from x in Children
                                   select x as BehaviorTreeElement;

            foreach(var ch in allChildrenToRun)
            {
                //TODO: will be changed to an actual debugger instead of just unity logs. Issue #3
                //Subscribes to updates to state changes from all children
                ch.ObserveEveryValueChanged(x => x.CurrentState)
                    //.Do(x => BehaviorLogger.Log(ElementType + " state changed: " + x))
                    .Subscribe()
                    .AddTo(Disposables);
            }

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