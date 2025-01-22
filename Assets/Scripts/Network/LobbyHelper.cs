using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Netcode;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyHelper : Singleton<LobbyHelper> {
    #region DATA QUERY RATE
    public const int RATE_QUERY = 1200;
    public const int RATE_HEARTBEAT = 25000;
    public const int RATE_GET = 1200;
    #endregion

    #region DATA KEY
    private const string KEY_RELAY_CODE = "Relay Code";
    #endregion

    [SerializeField] private Sprite _LoadBattleImg;
    private ILobbyService _LobbyService;
    private ILobbyServiceSDK _LobbySDK;
    private NetworkManager _Network;
    private RelayHelper _RelayHelper;
    private AutoBeatLobby _JoinedLobby;

    public BindableProperty<RoomToolUI.Status> RoomToolStatus 
        { get; } = new();

    public Lobby JoinedLobby {
        get => _JoinedLobby?.Value;
        private set {
            if (value == null) _JoinedLobby.StopSync();
            else _JoinedLobby.StartSync(value, AuthHelper.id_unity == value.HostId);  
        }
    }

    public string HostUnityId 
        => JoinedLobby?.HostId;

    public string ClientUnityId 
        => JoinedLobby?.Players.First(player => player.Id != JoinedLobby.HostId).Id;

    public void Init() {
        _LobbyService = LobbyService.Instance;
        _LobbySDK = Lobbies.Instance;
        _Network = NetworkManager.Singleton;
        _RelayHelper = new();
        _JoinedLobby = new(LobbyService.Instance);

        RoomToolStatus.Value = RoomToolUI.Status.None;
    }

    private async void OnClientConnect(ulong id) {
        Debug.LogWarning("Client connect nè :" + _Network.ConnectedClients.Count);

        if (_Network.ConnectedClients.Count != 2) return;
        _Network.OnClientConnectedCallback -= OnClientConnect;

        if (_Network.IsHost) {
            VibrateHelper.Vibrate();
            LoadSceneHelper.ShowImage(_LoadBattleImg);

            JoinedLobby = await _LobbyService.GetLobbyAsync(JoinedLobby.Id);
        }

        LoadBattleScene();

        await DeleteHostedLobby();
    }

    private async Task<string> StartHostNetwork() {
        _Network.OnClientConnectedCallback -= OnClientConnect;
        _Network.OnClientConnectedCallback += OnClientConnect;

        return await _RelayHelper.CreateRelay();
    }

    private async Task StartClientNetwork(string relayCode) {
        _Network.OnClientConnectedCallback -= OnClientConnect;
        _Network.OnClientConnectedCallback += OnClientConnect;
        
        LoadSceneHelper.ShowImage(_LoadBattleImg);

        await _RelayHelper.JoinRelay(relayCode);
    }
    
    private void StopNetwork() {
        _Network.OnClientConnectedCallback -= OnClientConnect;
        _Network.Shutdown();
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false) {
        try {
            RoomToolStatus.Value = RoomToolUI.Status.Creating;
            
            string relayCode = await StartHostNetwork();

            JoinedLobby = await _LobbyService.CreateLobbyAsync(lobbyName, maxPlayers, new() {
                IsPrivate = isPrivate,
                Data = new() {
                    { KEY_RELAY_CODE, new(DataObject.VisibilityOptions.Member, relayCode) }
                },
            });

            RoomToolStatus.Value = RoomToolUI.Status.Waiting;
            VibrateHelper.Vibrate();

            Debug.Log($"Created lobby {JoinedLobby.Name} with code = {JoinedLobby.LobbyCode}");
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }

    public async Task JoinLobbyByCode(string lobbyCode) {
        try {
            JoinedLobby = await _LobbyService.JoinLobbyByCodeAsync(lobbyCode);

            await StartClientNetwork(JoinedLobby.Data[KEY_RELAY_CODE].Value);

            Debug.Log($"Joined lobby {JoinedLobby.Name}");
        } catch (ArgumentNullException) {
            PopupFactory.ShowSimplePopup("Vui lòng nhập mã phòng");
        } catch (LobbyServiceException e) {
            PopupFactory.ShowSimplePopup(e.Reason switch {
                LobbyExceptionReason.InvalidJoinCode or
                LobbyExceptionReason.ValidationError or
                LobbyExceptionReason.ValidationError => "Mã phòng không hợp lệ",
                LobbyExceptionReason.LobbyNotFound => $"Không tìm thấy phòng {lobbyCode}",
                _ => e.Reason + ": " + e.Message,
            });
        }
    }

    public async Task JoinLobbyById(string lobbyId) {
        try {
            JoinedLobby = await _LobbyService.JoinLobbyByIdAsync(lobbyId);

            Debug.LogWarning("Lấy lobby xong nè");

            await StartClientNetwork(JoinedLobby.Data[KEY_RELAY_CODE].Value);

            Debug.LogWarning("start network xong nè");


            Debug.Log($"Joined lobby {JoinedLobby.Name}");
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }
    
    public async Task DeleteHostedLobby() {
        if (JoinedLobby == null) {
            Debug.LogWarning("You are not in any lobby");
            return;
        }
        
        if (JoinedLobby.HostId != AuthenticationService.Instance.PlayerId) {
            Debug.LogWarning("You dont have permission to delete current lobby");
            return;
        }

        try {
            await _LobbyService.DeleteLobbyAsync(JoinedLobby.Id);

            Debug.Log($"Deleted lobby {JoinedLobby.Name}");

            JoinedLobby = null;

            RoomToolStatus.Value = RoomToolUI.Status.None;
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }

    public async Task DeleteHostedLobbyAndNetwork() {
        await DeleteHostedLobby();
        StopNetwork();
    }
    
    public async Task<List<Lobby>> QueryLobbies(bool tryHard = false) {
        List<Lobby> listLobby;

        try {
            listLobby = (await _LobbySDK.QueryLobbiesAsync()).Results;

            Debug.Log(listLobby.Count + " lobbies found");
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
            if (!tryHard) return null;
            await Task.Delay(RATE_QUERY);
            return await QueryLobbies();
        }

        return listLobby;
    }

    private void LoadBattleScene() {
        Debug.LogWarning("Load battle secene nè");

        DataHelper.SceneBoostData.battle.battleMode = BattleMode.Player_Player;
        LoadSceneHelper.LoadScene(
            sceneName: "BattleScene",
            loadStyle: LoadSceneHelper.LoadStyle.Image,
            imageToShow: _LoadBattleImg,
            delayDisapear: null,
            delayApear: new Task(async () => {
                while (BattleConnector.Instance == null)
                    await Task.Delay(33);
                await BattleConnector.Instance.WaitForBothReadyToPlay();
            }),
            manualShowImage: true);
    }

    private async void OnDisable() {
        await DeleteHostedLobbyAndNetwork();
    }
}
