using UnityEngine;
using Unity.Netcode;

public class TestMap : NetworkBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    private void Start()
    {
        Debug.Log($"OwnerClientId: {OwnerClientId}");
        SpawnPlayerRpc(OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerRpc(ulong ownerId)
    {
        var player = Instantiate(PlayerPrefab);
        var network_player = player.GetComponent<NetworkObject>();
        network_player.SpawnWithOwnership(ownerId);
    }
}
