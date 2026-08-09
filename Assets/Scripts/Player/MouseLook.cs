using UnityEngine;

[AddComponentMenu("Game/Player/Mouse Look")]
public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY,
        MouseX,
        MouseY
    }

    [Header("Degrees of Freedom")]
    [SerializeField] private RotationAxes _axes = RotationAxes.MouseXAndY;

    [Space(5), Header("Sensitivity")]
    [SerializeField, Range(0.5f, 20.0f)] private float _sensitivityHorizontal = 8.0f;
    [SerializeField, Range(0.5f, 20.0f)] private float _sensitivityVertical = 8.0f;

    [Space(5), Header("Constraints")]
    [SerializeField] private float _minVerticalAngle = -75.0f;
    [SerializeField] private float _maxVerticalAngle = 75.0f;

    private float _verticalRotation = 0.0f;
    
    public float Sensitivity
    {
        get => _sensitivityHorizontal;
        set
        {
            float clamped = Mathf.Clamp(value, 0.5f, 20.0f);
            _sensitivityHorizontal = clamped;
            _sensitivityVertical = clamped;
        }
    }
    

    private void Update()
    {
        if (GameManager.Exists && GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        switch (_axes)
        {
            case RotationAxes.MouseX:
                transform.Rotate(
                    0.0f,
                    Input.GetAxis("Mouse X") * _sensitivityHorizontal,
                    0.0f
                );
                break;

            case RotationAxes.MouseY:
                _verticalRotation -= Input.GetAxis("Mouse Y") * _sensitivityVertical;
                _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalAngle, _maxVerticalAngle);

                float horizontalRotation = transform.localEulerAngles.y;

                transform.localEulerAngles = new Vector3(
                    _verticalRotation,
                    horizontalRotation,
                    0.0f
                );

                break;

            case RotationAxes.MouseXAndY:
                _verticalRotation -= Input.GetAxis("Mouse Y") * _sensitivityVertical;
                _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalAngle, _maxVerticalAngle);

                float deltaX = Input.GetAxis("Mouse X") * _sensitivityHorizontal;
                horizontalRotation = transform.localEulerAngles.y + deltaX;

                transform.localEulerAngles = new Vector3(
                    _verticalRotation,
                    horizontalRotation,
                    0.0f
                );

                break;
        }
    }
}
