using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

public class LobbyHelper : Singleton<LobbyHelper> {
    #region DATA QUERY RATE
    public const int RATE_QUERY = 1500;
    public const int RATE_HEARTBEAT = 1500;
    public const int RATE_GET = 1500;
    #endregion

    #region DATA KEY
    private const string KEY_RELAY_CODE = "Relay Code";
    #endregion

    public BindableProperty<RoomToolUI.Status> RoomToolStatus { get; } = new();
    public RealtimeLobby JoinedLobby { get; private set; }
    public RelayHelper RelayHelper { get; private set; }

    private ILobbyService _LobbyService;
    private ILobbyServiceSDK _LobbySDK;
    private CancellableTask _WaitForOpponentTask;
    private CancellableTask _WaitForHostStartGameTask;

    public void Init() {
        _LobbyService = LobbyService.Instance;
        _LobbySDK = Lobbies.Instance;
        RelayHelper = new();
        JoinedLobby = new(LobbyService.Instance);

        RoomToolStatus.Value = RoomToolUI.Status.None;
    }

    private async Task WaitForOpponent(CancellationToken tokken) {
        while (!tokken.IsCancellationRequested) {
            if (JoinedLobby.Value.Players.Count != 2) {
                await Task.Delay(RATE_GET);
                continue;
            }

            VibrateHelper.Vibrate();

            string relayCode = await RelayHelper.CreateRelay();

            UpdateLobbyOptions options = new() {
                Data = new() {
                    { KEY_RELAY_CODE, new(DataObject.VisibilityOptions.Member, relayCode) }
                },
            };

            await _LobbyService.UpdateLobbyAsync(JoinedLobby.Value.Id, options);

            JoinGame();

            return;
        }
    }

    private async Task WaitForHostStartGame(CancellationToken tokken) {
        while (!tokken.IsCancellationRequested) {
            string relayCode = JoinedLobby.Value.Data[KEY_RELAY_CODE].Value;
            if (relayCode == null) {
                await Task.Delay(RATE_GET);
                continue;
            }

            await RelayHelper.JoinRelay(relayCode);

            JoinGame();

            return;
        }
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false) {
        try {
            RoomToolStatus.Value = RoomToolUI.Status.Creating;


            CreateLobbyOptions options = new() {
                IsPrivate = isPrivate,
                Data = new() {
                    { KEY_RELAY_CODE, new(DataObject.VisibilityOptions.Member, null) }
                },
            };

            Lobby lobby = await _LobbyService.CreateLobbyAsync(lobbyName, maxPlayers, options);

            Debug.Log($"Created lobby {lobbyName} with code = {lobby.LobbyCode}");

            JoinedLobby.StartSync(lobby, true);

            RoomToolStatus.Value = RoomToolUI.Status.Waiting;
            VibrateHelper.Vibrate();

            _WaitForOpponentTask = new(WaitForOpponent);
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }

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

    public async Task JoinLobbyByCode(string lobbyCode) {
        try {
            Lobby lobby = await _LobbyService.JoinLobbyByCodeAsync(lobbyCode);

            Debug.Log($"Joined lobby {lobby.Name}");

            JoinedLobby.StartSync(lobby, false);

            _WaitForHostStartGameTask = new(WaitForHostStartGame);
        } catch (ArgumentNullException e) {
            PopupFactory.Instance.ShowSimplePopup("Vui lòng nhập mã phòng");
        } catch (LobbyServiceException e) {
            PopupFactory.Instance.ShowSimplePopup(e.Reason switch {
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
            Lobby lobby = await _LobbyService.JoinLobbyByIdAsync(lobbyId);

            Debug.Log($"Joined lobby {lobby.Name}");

            JoinedLobby.StartSync(lobby, false);

            _WaitForHostStartGameTask = new(WaitForHostStartGame);
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

            _WaitForHostStartGameTask?.Cancel();
            _WaitForHostStartGameTask = null;

            RoomToolStatus.Value = RoomToolUI.Status.None;
        } catch (LobbyServiceException e) {
            Debug.LogError(e.Message);
        }
    }

    public void JoinGame() {
        JoinedLobby.StopSync();
        ToBattleScene();
    }

    private void ToBattleScene() {
        SceneManager.LoadScene("BattleScene");
    }

    private void OnDisable() {
        _ = DeleteHostedLobby();
    }
}
