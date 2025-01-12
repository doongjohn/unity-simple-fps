using UnityEngine;
using Unity.Netcode;

public class TestMap : NetworkBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    private void Start()
    {
        SpawnPlayerRpc(OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerRpc(ulong ownerId)
    {
        Debug.Log(ownerId);
        var player = Instantiate(PlayerPrefab);
        var network_player = player.GetComponent<NetworkObject>();
        network_player.SpawnWithOwnership(ownerId);
    }
}
