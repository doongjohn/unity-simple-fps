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

    private CallResult<LobbyCreated_t> _onCreateLobby;
    private CallResult<LobbyEnter_t> _onJoinLobby;
    private Callback<GameLobbyJoinRequested_t> _onGameLobbyJoinRequested;

    private void Awake()
    {
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

        _onCreateLobby = CallResult<LobbyCreated_t>.Create(OnCreateLobby);
        _onJoinLobby = CallResult<LobbyEnter_t>.Create(OnJoinLobby);
        _onGameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
    }

    private void OnClickHostGameButton(ClickEvent evt)
    {
        Debug.Log("HostGameButton");
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 100);
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
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t arg)
    {
        Debug.Log("Invite accepted.");
        SteamMatchmaking.JoinLobby(arg.m_steamIDLobby);
    }
}
