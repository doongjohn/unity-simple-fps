using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class TestMap : MapBase
{
    [SerializeField] private Player PlayerPrefab;

    protected override void Start()
    {
        base.Start();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnClientStopped(bool isHost)
    {
        SceneManager.LoadScene(Scenes.LobbyListMenu);
    }

    protected override void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (IsHost)
        {
            switch (sceneEvent.SceneEventType)
            {
                // Client scene loaded.
                case SceneEventType.LoadComplete:
                    SpawnPlayer(sceneEvent.ClientId);
                    break;
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        var player = Instantiate(PlayerPrefab);
        player.SetInputActive(true);

        var networkPlayer = player.GetComponent<NetworkObject>();
        networkPlayer.transform.position = new(0, 3.0f, 0);
        networkPlayer.SpawnWithOwnership(clientId);
    }
}
