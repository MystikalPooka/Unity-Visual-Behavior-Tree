using UniRx;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InputHandler inputs;

    public float walkSpeed = 0.5f;
    private CharacterController character;
    // Use this for initialization
    private void Start()
    {
        character = FindObjectOfType<CharacterController>();
        inputs.Movement
          .Where(v => v != Vector2.zero)
          .Subscribe(inputMovement => {
              var inputVelocity = inputMovement * walkSpeed;

              var playerVelocity =
            inputVelocity.x * transform.right +
            inputVelocity.y * transform.forward;

              var distance = playerVelocity * Time.fixedDeltaTime;
              character.Move(distance);
          }).AddTo(this);
    }
}
