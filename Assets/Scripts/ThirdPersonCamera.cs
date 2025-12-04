using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;

    private void LateUpdate()
    {
        if (_target == null) return;
        transform.position = _target.position + _target.TransformDirection(_offset);
        transform.LookAt(_target.position);
    }
}
