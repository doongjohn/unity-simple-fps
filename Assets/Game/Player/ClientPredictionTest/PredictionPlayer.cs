using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public struct BufferedPlayerState : INetworkSerializeByMemcpy
{
    public UInt64 Tick;
    public Vector3 Pos;
}

public struct BufferedPlayerInput : INetworkSerializeByMemcpy
{
    public UInt64 Tick;
    public Vector2 InputMove;
}

public class PredictionPlayer : NetworkBehaviour
{
    public float MoveSpeed = 10;

    private UInt64 _tick = 0;
    private List<BufferedPlayerState> _stateBuffer = new();
    private List<BufferedPlayerInput> _inputBuffer = new();
    private Queue<BufferedPlayerState> _recivedStates = new();
    private Queue<BufferedPlayerInput> _recivedInputs = new();

    private InputAction _inputMove;

    private bool _isStun = false;
    private float _stunTime = 0;

    private void Awake()
    {
        _inputMove = InputSystem.actions.FindAction("Player/Move");
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Self stun.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendStunToServerRpc();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsSpawned)
        {
            return;
        }

        if (IsOwner)
        {
            _tick += 1;

            // Get input.
            var input = _inputMove.ReadValue<Vector2>();
            SendInputToServerRpc(new BufferedPlayerInput { Tick = _tick, InputMove = input });

            if (!IsHost)
            {
                // Client-side prediction.
                if (!_isStun)
                {
                    var pos = Move(input);
                    transform.position = pos;
                }

                ClientSaveStates(input);
                ClientRollback();
            }
        }

        if (IsHost)
        {
            while (_recivedInputs.Count > 0)
            {
                // Get input.
                var input = _recivedInputs.Dequeue();

                // Process input.
                if (!_isStun)
                {
                    var pos = Move(input.InputMove);
                    transform.position = pos;
                }

                var state = new BufferedPlayerState
                {
                    Tick = input.Tick,
                    Pos = transform.position,
                };

                SendStateToOwnerRpc(state);
                SendStateToNonOwnerRpc(state);
            }

            if (_isStun)
            {
                _stunTime += Time.fixedDeltaTime;
                if (_stunTime >= 2)
                {
                    _isStun = false;
                    _stunTime = 0;
                    SendStunToOwnerRpc(false);
                }
            }
        }
    }

    private Vector3 Move(Vector2 input)
    {
        var pos = transform.position;
        var moveDelta = input * (MoveSpeed * Time.fixedDeltaTime);
        pos.x += moveDelta.x;
        pos.z += moveDelta.y;
        return pos;
    }

    private void ClientSaveStates(Vector2 input)
    {
        // save state to buffer
        _stateBuffer.Add(new BufferedPlayerState
        {
            Tick = _tick,
            Pos = transform.position,
        });
        if (_stateBuffer.Count >= 30)
            _stateBuffer.RemoveAt(0);

        // save input to buffer
        _inputBuffer.Add(new BufferedPlayerInput
        {
            Tick = _tick,
            InputMove = input,
        });
        if (_inputBuffer.Count >= 30)
            _inputBuffer.RemoveAt(0);
    }

    private void ClientRollback()
    {
        while (_recivedStates.Count > 0)
        {
            var serverState = _recivedStates.Dequeue();
            var stateIndex = _stateBuffer.FindIndex(0, _stateBuffer.Count, (item) => item.Tick == serverState.Tick);
            var clientState = _stateBuffer[stateIndex];

            if (clientState.Pos == serverState.Pos)
            {
                Debug.Log("Prediction success");
            }
            else
            {
                Debug.Log("Prediction failed");

                transform.position = serverState.Pos;

                _stateBuffer.RemoveRange(0, stateIndex + 1);
                _inputBuffer.RemoveRange(0, stateIndex + 1);

                for (int i = 0; i < _stateBuffer.Count; ++i)
                {
                    var input = _inputBuffer[i];
                    var pos = Move(input.InputMove);
                    transform.position = pos;

                    _stateBuffer[i] = new BufferedPlayerState
                    {
                        Tick = _stateBuffer[i].Tick,
                        Pos = transform.position,
                    };
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SendInputToServerRpc(BufferedPlayerInput input)
    {
        _recivedInputs.Enqueue(input);
    }

    [Rpc(SendTo.Owner)]
    private void SendStateToOwnerRpc(BufferedPlayerState state)
    {
        _recivedStates.Enqueue(state);
    }

    [Rpc(SendTo.NotOwner)]
    private void SendStateToNonOwnerRpc(BufferedPlayerState state)
    {
        if (!IsHost)
        {
            transform.position = state.Pos;
        }
    }

    [Rpc(SendTo.Server)]
    private void SendStunToServerRpc()
    {
        _isStun = true;
        SendStunToOwnerRpc(true);
    }

    [Rpc(SendTo.Owner)]
    private void SendStunToOwnerRpc(bool value)
    {
        Debug.Log("Stun");
        _isStun = value;
    }
}
