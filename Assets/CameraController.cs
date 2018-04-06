using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{

    public GameObject Player;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - Player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Player.transform.position + offset;

    }
}