using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public IObservable<Vector2> Movement { get; private set; }

    private void Awake()
    {
        Movement = this.FixedUpdateAsObservable()
          .Select(_ => {
              var x = Input.GetAxis("Horizontal");
              var y = Input.GetAxis("Vertical");
              return new Vector2(x, y).normalized;
          });
    }
}
