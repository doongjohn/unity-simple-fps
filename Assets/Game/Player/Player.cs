using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private float WalkSpeed = 4.0f;
    [SerializeField] private float LookSensitivity = 5.0f;
    [SerializeField] private Transform CameraTarget;

    private CinemachineCamera _cmFpsCamera;
    private Rigidbody _rb;

    private InputAction _look;
    private InputAction _move;

    private void Awake()
    {
        _cmFpsCamera = GameObject.Find("Cm FPS Camera").GetComponent<CinemachineCamera>();

        _rb = GetComponent<Rigidbody>();

        _look = InputSystem.actions.FindAction("Player/Look");
        _move = InputSystem.actions.FindAction("Player/Move");
    }

    private void Start()
    {
        Debug.Log(IsOwner);
        if (IsOwner)
        {
            _cmFpsCamera.Target.TrackingTarget = CameraTarget;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            CameraControl();
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Movement();
        }
    }

    private void CameraControl()
    {
        var lookDelta = _look.ReadValue<Vector2>();
        var rotationY = lookDelta.x * LookSensitivity * Time.deltaTime;
        var rotationX = -lookDelta.y * LookSensitivity * Time.deltaTime;

        // Rotate self
        var rotation = transform.eulerAngles;
        rotation.x = 0;
        rotation.z = 0;
        transform.eulerAngles = rotation;
        transform.RotateAround(transform.position, transform.up, rotationY);

        // Rotate camera
        var cameraTransform = _cmFpsCamera.transform;
        cameraTransform.RotateAround(cameraTransform.position, transform.right, rotationX);
        var cameraRotation = cameraTransform.eulerAngles;
        cameraRotation.y = transform.eulerAngles.y;
        cameraRotation.z = transform.eulerAngles.z;
        cameraTransform.eulerAngles = cameraRotation;
    }

    private void Movement()
    {
        var inputDir = _move.ReadValue<Vector2>();

        var targetForwardSpeed = inputDir.y * WalkSpeed;
        var targetRightSpeed = inputDir.x * WalkSpeed;
        var velocity = _rb.linearVelocity;

        var forwardSpeed = Vector3.Dot(transform.forward, velocity);
        var rightSpeed = Vector3.Dot(transform.right, velocity);

        if (Mathf.Abs(targetForwardSpeed) > Mathf.Abs(forwardSpeed))
        {
            var addSpeed = (targetForwardSpeed - forwardSpeed) / Time.fixedDeltaTime;
            _rb.AddForce(transform.forward * addSpeed, ForceMode.Acceleration);
        }

        if (Mathf.Abs(targetRightSpeed) > Mathf.Abs(rightSpeed))
        {
            var addSpeed = (targetRightSpeed - rightSpeed) / Time.fixedDeltaTime;
            _rb.AddForce(transform.right * addSpeed, ForceMode.Acceleration);
        }
    }
}
