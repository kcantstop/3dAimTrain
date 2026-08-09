using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Game/Player/Player Controller")]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Attributes")]
    [SerializeField, Range(1.0f, 15.0f)] private float _speed = 6.0f;
    [SerializeField, Range(1.0f, 3.0f)] private float _sprintMultiplier = 1.6f;
    [SerializeField] private KeyCode _sprintKey = KeyCode.LeftShift;

    [Space(5), Header("Jumping Attributes")]
    [SerializeField, Range(5.0f, 20.0f)] private float _jumpVelocity = 8.0f;
    [SerializeField, Range(0.5f, 5.0f)] private float _fallingScalar = 3.0f;

    private CharacterController _controller;
    private float _gravity = -9.81f;
    private float _verticalVelocity;
    private float _speedMultiplier = 1.0f;
    private bool _isSprinting;
    
    public float Speed
    {
        get => _speed;
        set => _speed = Mathf.Clamp(value, 0.0f, 30.0f);
    }
    
    public float SpeedMultiplier
    {
        get => _speedMultiplier;
        set => _speedMultiplier = Mathf.Clamp(value, 0.1f, 5.0f);
    }
    
    public float CurrentSpeed => _speed * _speedMultiplier * (_isSprinting ? _sprintMultiplier : 1.0f);
    
    public bool IsGrounded => _controller && _controller.isGrounded;
    

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (GameManager.Exists && GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        _isSprinting = Input.GetKey(_sprintKey);

        float activeSpeed = CurrentSpeed;

        float deltaX = Input.GetAxis("Horizontal") * activeSpeed;
        float deltaZ = Input.GetAxis("Vertical") * activeSpeed;

        Vector3 movement = new(deltaX, 0.0f, deltaZ);

        
        movement = Vector3.ClampMagnitude(movement, activeSpeed);

        if (_controller.isGrounded)
        {
            _verticalVelocity = _gravity;

            if (Input.GetButtonDown("Jump"))
            {
                _verticalVelocity = _jumpVelocity;
            }
        }
        else
        {
            _verticalVelocity += _gravity * _fallingScalar * Time.deltaTime;
        }

        
        movement.y = _verticalVelocity;

        
        movement *= Time.deltaTime;

       
        movement = transform.TransformDirection(movement);

        _controller.Move(movement);
    }
}
