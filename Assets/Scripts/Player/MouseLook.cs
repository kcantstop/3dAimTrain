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

    public const float MinSensitivity = 0.1f;
    public const float MaxSensitivity = 1.0f;

    public const float MinSliderValue = 1.0f;
    public const float MaxSliderValue = 5.0f;

    [Header("Degrees of Freedom")]
    [SerializeField] private RotationAxes _axes = RotationAxes.MouseXAndY;

    [Space(5), Header("Sensitivity")]
    [SerializeField, Range(MinSensitivity, MaxSensitivity)] private float _sensitivityHorizontal = 2.0f;
    [SerializeField, Range(MinSensitivity, MaxSensitivity)] private float _sensitivityVertical = 2.0f;

    [Space(5), Header("Constraints")]
    [SerializeField] private float _minVerticalAngle = -75.0f;
    [SerializeField] private float _maxVerticalAngle = 75.0f;

    private float _verticalRotation = 0.0f;

    public float Sensitivity
    {
        get => _sensitivityHorizontal;
        set
        {
            float clamped = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
            _sensitivityHorizontal = clamped;
            _sensitivityVertical = clamped;
        }
    }

    
    public static float SensitivityFromSlider(float sliderValue)
    {
        float t = Mathf.InverseLerp(MinSliderValue, MaxSliderValue, sliderValue);
        return Mathf.Lerp(MinSensitivity, MaxSensitivity, t);
    }

    public static float SliderFromSensitivity(float sensitivity)
    {
        float t = Mathf.InverseLerp(MinSensitivity, MaxSensitivity, sensitivity);
        return Mathf.Lerp(MinSliderValue, MaxSliderValue, t);
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
