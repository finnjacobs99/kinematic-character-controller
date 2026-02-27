using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;

    private Vector3 _eulerAngles;

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void UpdateRotation(PlayerInput input)
    {
        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
