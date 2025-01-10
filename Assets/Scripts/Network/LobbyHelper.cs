using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using QFSW.QC;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LobbyHelper : Singleton<LobbyHelper> {
    public const int RATE_QUERY = 1500;
    public const int RATE_HEARTBEAT = 1500;
    public const int RATE_GET = 1500;

    private const string KEY_RELAY_CODE = "Relay Code";

    public BindableProperty<RoomToolUI.Status> RoomToolStatus { get; } = new();
    public RealtimeLobby JoinedLobby { get; private set; }
    public RelayHelper RelayHelper { get; private set; }

    private bool isInitialized = false;
    private ILobbyService _LobbyService;
    private ILobbyServiceSDK _LobbySDK;
    private CancellableTask _WaitForOpponentTask;

    protected override async void Awake() {
        base.Awake();

        await UnityServices.InitializeAsync();

        try {
            AuthenticationService.Instance.SignedIn += () => {
                print("Signed in: " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        } catch (AuthenticationException e) {
            Debug.LogError(e.Message);
        }

        _LobbyService = LobbyService.Instance;
        _LobbySDK = Lobbies.Instance;
        RelayHelper = new();
        JoinedLobby = new(LobbyService.Instance);

        RoomToolStatus.Value = RoomToolUI.Status.None;

        isInitialized = true;
    }
    
    public async Task WaitForInit() {
        while (!isInitialized)
            await Task.Delay(200);
    }
    
    private async Task WaitForOpponent(CancellationToken tokken) {
        while (!tokken.IsCancellationRequested) {
            if (JoinedLobby.Value.Players.Count == 2) {
                StartGame();
                return;
            }
            await Task.Delay(RATE_GET);
        }
    }

    [Command]
    public async Task CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false) {
        try {
            RoomToolStatus.Value = RoomToolUI.Status.Creating;

            string relayCode = await RelayHelper.CreateRelay();

            CreateLobbyOptions options = new() {
                IsPrivate = isPrivate,
                Data = new() {
                    { KEY_RELAY_CODE, new(DataObject.VisibilityOptions.Member, relayCode) }
                },
            };

            Lobby lobby = await _LobbyService.CreateLobbyAsync(lobbyName, maxPlayers, options);
        
            Debug.Log($"Created lobby {lobbyName} with code = {lobby.LobbyCode}");

            JoinedLobby.StartSync(lobby, true);

            RoomToolStatus.Value = RoomToolUI.Status.Waiting;

            _WaitForOpponentTask = new(WaitForOpponent);
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }

    [Command]
    public async Task<List<Lobby>> QueryLobbies() {
        List<Lobby> listLobby = null;

        try {
            listLobby = (await _LobbySDK.QueryLobbiesAsync()).Results;

            Debug.Log(listLobby.Count + " lobbies found");
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
            await Task.Delay(RATE_QUERY);
            return await QueryLobbies();
        }

        return listLobby;
    }

    [Command]
    public async Task JoinLobbyByCode(string lobbyCode) {
        try {
            Lobby lobby = await _LobbyService.JoinLobbyByCodeAsync(lobbyCode);

            Debug.Log($"Joined lobby {lobby.Name}");

            JoinedLobby.StartSync(lobby, false);

            await JoinGame(JoinedLobby.Value.Data[KEY_RELAY_CODE].Value);
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }

    [Command]
    public async Task JoinLobbyById(string lobbyId) {
        try {
            Lobby lobby = await _LobbyService.JoinLobbyByIdAsync(lobbyId);

            Debug.Log($"Joined lobby {lobby.Name}");

            JoinedLobby.StartSync(lobby, false);

            await JoinGame(JoinedLobby.Value.Data[KEY_RELAY_CODE].Value);
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }
    
    public async Task DeleteHostedLobby() {
        if (JoinedLobby.Value == null) {
            Debug.LogWarning("You are not in any lobby");
            return;
        }
        
        if (JoinedLobby.Value.HostId != AuthenticationService.Instance.PlayerId) {
            Debug.LogWarning("You dont have permission to delete current lobby");
            return;
        }

        try {
            await _LobbyService.DeleteLobbyAsync(JoinedLobby.Value.Id);

            Debug.Log($"Deleted lobby {JoinedLobby.Value.Name}");

            JoinedLobby.StopSync();
            
            _WaitForOpponentTask?.Cancel();
            _WaitForOpponentTask = null;

            RoomToolStatus.Value = RoomToolUI.Status.None;
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }
    
    public void StartGame() {
        JoinedLobby.StopSync();
        ToBattleScene();
    }

    [Command]
    public async Task JoinGame(string relayCode) {
        JoinedLobby.StopSync();
        await RelayHelper.JoinRelay(relayCode);
        ToBattleScene();
    }

    private void ToBattleScene() {
        SceneManager.LoadScene("BattleScene");
    }

    private void OnDisable() {
        _ = DeleteHostedLobby();
    }
}
