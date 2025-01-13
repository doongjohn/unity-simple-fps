using System.Collections.Generic;
using System.Linq;
using Steamworks;
using Unity.Netcode;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public struct LobbyListEntryData
{
    public CSteamID LobbyId;
    public string LobbyName;
    public string LobbyOwnerName;
}

public class LobbyListEntryController
{
    private readonly Label _lobbyNameLabel;
    private readonly Label _lobbyOwnerLabel;

    public LobbyListEntryController(VisualElement _root)
    {
        _lobbyNameLabel = _root.Q("LobbyNameLabel") as Label;
        _lobbyOwnerLabel = _root.Q("LobbyOwnerLabel") as Label;
    }

    public void UpdateElements(LobbyListEntryData data)
    {
        _lobbyNameLabel.text = data.LobbyName;
        _lobbyOwnerLabel.text = data.LobbyOwnerName;
    }
}

public class LobbyListMenu : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset LobbyListEntryAsset;

    private UIDocument _document;
    private VisualElement _root;
    private Button _refreshButton;
    private ListView _lobbyListView;
    private TextField _lobbyNameTextField;
    private Button _lobbyButton1;
    private Button _lobbyButton2;
    private Button _startMatchButton;
    private IntegerField _maxPlayersNum;
    private Toggle _friendOnlyToggle;

    private CallResult<LobbyMatchList_t> _onRequestLobbyList;
    private CallResult<LobbyCreated_t> _onCreateLobby;

    private List<LobbyListEntryData> _lobbyEntries = new();

    private int _myMaxPlayersValue = 10;
    [CreateProperty]
    public int MyMaxPlayersValue
    {
        get => _myMaxPlayersValue;
        set => _myMaxPlayersValue = Mathf.Clamp(value, 0, 10);
    }

    private string _myLobbyNameValue = "";
    [CreateProperty]
    public string MyLobbyNameValue
    {
        get => _myLobbyNameValue;
        set => _myLobbyNameValue = value.Trim();
    }

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;

        _refreshButton = _root.Q("RefreshButton") as Button;
        _lobbyListView = _root.Q("LobbyListView") as ListView;
        _lobbyNameTextField = _root.Q("LobbyNameTextField") as TextField;
        _lobbyButton1 = _root.Q("LobbyButton1") as Button;
        _lobbyButton2 = _root.Q("LobbyButton2") as Button;
        _startMatchButton = _root.Q("StartMatchButton") as Button;
        _maxPlayersNum = _root.Q("MaxPlayersNum") as IntegerField;
        _friendOnlyToggle = _root.Q("FriendOnlyToggle") as Toggle;

        _onRequestLobbyList = new(OnRequestLobbyList);
        _onCreateLobby = new(OnCreateLobby);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;

        // Setup RefreshButton.
        _refreshButton.RegisterCallback<ClickEvent>((evt) =>
        {
            _lobbyEntries.Clear();
            _onRequestLobbyList.Set(SteamMatchmaking.RequestLobbyList());
        });

        // Setup LobbyListView.
        _lobbyListView.itemsSource = _lobbyEntries;
        _lobbyListView.makeItem = () =>
        {
            var entry = LobbyListEntryAsset.Instantiate();
            entry.userData = new LobbyListEntryController(entry);
            return entry;
        };
        _lobbyListView.bindItem = (item, index) =>
        {
            (item.userData as LobbyListEntryController)?.UpdateElements(_lobbyEntries[index]);
        };

        // Setup LobbyName.
        _lobbyNameTextField.SetBinding("value", new DataBinding
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(MyLobbyNameValue))
        });

        // Setup MaxPlayersNum.
        _maxPlayersNum.SetBinding("value", new DataBinding
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(MyMaxPlayersValue))
        });

        // Request lobby list.
        _onRequestLobbyList.Set(SteamMatchmaking.RequestLobbyList());

        // Setup button callback.
        SetLobbyButtonCallbacks(true);
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        }
    }

    private void OnClientStopped(bool isHost)
    {
        // TODO: 방 나갔음
    }

    private void SetLobbyButtonCallbacks(bool isCreateLobbyMode)
    {
        // Reset callbacks
        _lobbyButton1.UnregisterCallback<ClickEvent>(OnClickCreateLobbyButton);
        _lobbyButton2.UnregisterCallback<ClickEvent>(OnClickDeleteLobbyButton);
        // _lobbyButton1.UnregisterCallback<ClickEvent>(OnClickJoinLobbyButton);
        // _lobbyButton2.UnregisterCallback<ClickEvent>(OnClickLeaveLobbyButton);
        _startMatchButton.UnregisterCallback<ClickEvent>(OnClickStartMatchButton);

        if (isCreateLobbyMode)
        {
            _lobbyButton1.RegisterCallback<ClickEvent>(OnClickCreateLobbyButton);
            _lobbyButton2.RegisterCallback<ClickEvent>(OnClickDeleteLobbyButton);
            _startMatchButton.RegisterCallback<ClickEvent>(OnClickStartMatchButton);
        }
        else
        {
            // _lobbyButton1.RegisterCallback<ClickEvent>(OnClickJoinLobbyButton);
            // _lobbyButton2.RegisterCallback<ClickEvent>(OnClickLeaveLobbyButton);
        }
    }

    private void OnClickCreateLobbyButton(ClickEvent evt)
    {
        if (NetworkManager.Singleton.StartHost())
        {
            _maxPlayersNum.SetEnabled(false);
            _friendOnlyToggle.SetEnabled(false);

            var lobbyType = _friendOnlyToggle.value ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic;
            _onCreateLobby.Set(SteamMatchmaking.CreateLobby(lobbyType, MyMaxPlayersValue));
        }
        else
        {
            Debug.LogError("StartHost failed.");
        }
    }

    private void OnClickDeleteLobbyButton(ClickEvent evt)
    {
        LobbyManager.Singleton.LeaveLobby();
        NetworkManager.Singleton.Shutdown();

        _maxPlayersNum.SetEnabled(true);
        _friendOnlyToggle.SetEnabled(true);
    }

    private void OnClickStartMatchButton(ClickEvent evt)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("StartMatchButton: Start.");
            NetworkManager.Singleton.SceneManager.LoadScene(Scenes.TestMap, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("StartMatchButton: You are not the host.");
        }
    }

    private void OnRequestLobbyList(LobbyMatchList_t arg, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("OnRequestLobbyList IOFailure.");
            return;
        }

        var foundLobbiesCount = arg.m_nLobbiesMatching;
        Debug.Log($"Lobbies: {foundLobbiesCount}.");
        for (var i = 0; i < foundLobbiesCount; ++i)
        {
            var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            var lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "LobbyName");
            var lobbyOwnerName = SteamMatchmaking.GetLobbyData(lobbyId, "LobbyOwnerName");

            if (!string.IsNullOrEmpty(lobbyName))
            {
                Debug.Log($"Lobby Name: {lobbyName}.");
                _lobbyEntries.Add(new LobbyListEntryData
                {
                    LobbyId = lobbyId,
                    LobbyName = lobbyName,
                    LobbyOwnerName = lobbyOwnerName,
                });
            }
        }
        _lobbyListView.RefreshItems();
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

            var lobbyId = new CSteamID(arg.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(lobbyId, "LobbyName", MyLobbyNameValue);
            SteamMatchmaking.SetLobbyData(lobbyId, "LobbyOwnerName", SteamFriends.GetPersonaName());
        }
        else
        {
            Debug.Log("Failed to create a lobby.");
        }
    }
}
