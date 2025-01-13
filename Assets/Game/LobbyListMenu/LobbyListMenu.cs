using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LobbyListMenu : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _root;
    private Button _lobbyButton1;
    private Button _lobbyButton2;
    private Button _startMatchButton;

    private CallResult<LobbyCreated_t> _onCreateLobby;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;

        _lobbyButton1 = _root.Q("LobbyButton1") as Button;
        _lobbyButton2 = _root.Q("LobbyButton2") as Button;
        _startMatchButton = _root.Q("StartMatchButton") as Button;
        SetLobbyButtonCallbacks();

        _onCreateLobby = new(OnCreateLobby);
    }

    private void SetLobbyButtonCallbacks()
    {
        // TODO: client면 (방 참가), (방 나가기)로 변경
        _lobbyButton1.RegisterCallback<ClickEvent>(OnClickCreateLobbyButton);
        _lobbyButton2.RegisterCallback<ClickEvent>(OnClickDeleteLobbyButton);
        _startMatchButton.RegisterCallback<ClickEvent>(OnClickStartMatchButton);
    }

    private void OnClickCreateLobbyButton(ClickEvent evt)
    {
        NetworkManager.Singleton.StartHost();
        _onCreateLobby.Set(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 100));
    }

    private void OnClickDeleteLobbyButton(ClickEvent evt)
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void OnClickStartMatchButton(ClickEvent evt)
    {
        Debug.Log("StartMatchButton");
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(Scenes.TestMap, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("You are not the host.");
        }
    }

    private void OnCreateLobby(LobbyCreated_t arg, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("OnCreateLobby IOFailure.");
            return;
        }

        if (arg.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Lobby created.");
            LobbyManager.Singleton.SetJoinedLobbyId(arg.m_ulSteamIDLobby);
        }
        else
        {
            Debug.Log("Failed to create a lobby.");
        }
    }
}
