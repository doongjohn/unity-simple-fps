using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private float WalkSpeed = 4.0f;

    private Rigidbody _rb;
    private InputAction _move;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _move = InputSystem.actions.FindAction("Player/Move");
    }

    private void Update()
    {
        var velocity = _rb.linearVelocity;

        var inputDir = _move.ReadValue<Vector2>();
        var walkVelocity = inputDir * WalkSpeed;
        velocity.x = walkVelocity.x;
        velocity.z = walkVelocity.y;

        _rb.linearVelocity = velocity;
    }
}
