using UnityEngine;
using Unity.Netcode;

public class TestMap : NetworkBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    private void Start()
    {
        SpawnPlayerRpc();
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerRpc()
    {
        var player = Instantiate(PlayerPrefab);
        var network_player = player.GetComponent<NetworkObject>();
        network_player.Spawn();
    }
}