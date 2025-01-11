using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LobbyListMenu : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _root;
    private Button _hostGameButton;
    private Button _startGameButton;
    private Button _joinGameButton;

    private CSteamID? _joinedLobbyID; // move it to static field

    private CallResult<LobbyCreated_t> _onCreateLobby;
    private CallResult<LobbyEnter_t> _onJoinLobby;
    private Callback<GameLobbyJoinRequested_t> _onGameLobbyJoinRequested;

    private void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
        {
            Debug.Log("NetworkManager client connected.");
        };

        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;

        _hostGameButton = _root.Q("HostGameButton") as Button;
        _startGameButton = _root.Q("StartGameButton") as Button;
        _joinGameButton = _root.Q("JoinGameButton") as Button;

        _hostGameButton.RegisterCallback<ClickEvent>(OnClickHostGameButton);
        _startGameButton.RegisterCallback<ClickEvent>(OnClickStartGameButton);
        _joinGameButton.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            NetworkManager.Singleton.StartClient();
            NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
        });

        _onCreateLobby = new(OnCreateLobby);
        _onJoinLobby = new(OnJoinLobby);
        _onGameLobbyJoinRequested.Register(OnGameLobbyJoinRequested);
    }

    private void OnClickHostGameButton(ClickEvent evt)
    {
        Debug.Log("HostGameButton");
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
        _onCreateLobby.Set(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 100));
    }

    private void OnClickStartGameButton(ClickEvent evt)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(Scenes.TestMap, LoadSceneMode.Single);
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
            _joinedLobbyID = new(arg.m_ulSteamIDLobby);
        }
        else
        {
            Debug.Log("Failed to create a lobby.");
        }
    }

    private void OnJoinLobby(LobbyEnter_t arg, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("OnJoinLobby IOFailure.");
            return;
        }

        Debug.Log("Lobby joined.");
        _joinedLobbyID = new(arg.m_ulSteamIDLobby);
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t arg)
    {
        Debug.Log("Invite accepted.");
        _onJoinLobby.Set(SteamMatchmaking.JoinLobby(arg.m_steamIDLobby));
    }
}
