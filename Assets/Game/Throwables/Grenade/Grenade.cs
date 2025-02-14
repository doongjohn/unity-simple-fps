using UnityEngine;
using Unity.Netcode;

public class Grenade : NetworkBehaviour
{
    private NetworkObject _networkObject;
    private Rigidbody _rigidbody;
    private GameTimer _despawnTimer = new(3.0f);

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            _rigidbody.AddForce(transform.forward * 500f);
        }
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsSpawned)
            return;

        _despawnTimer.Tick(Time.deltaTime);
        if (_despawnTimer.IsEnded)
        {
            // TODO: damage player in area
            _networkObject.Despawn();
        }
    }
}
