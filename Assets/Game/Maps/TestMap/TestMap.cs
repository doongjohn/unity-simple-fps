using UnityEngine;
using Unity.Netcode;

public class TestMap : NetworkBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        var player = Instantiate(PlayerPrefab);
        var network_player = player.GetComponent<NetworkObject>();
        network_player.Spawn();
    }
}
