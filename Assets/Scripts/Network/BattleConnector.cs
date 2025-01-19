﻿using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class BattleConnector : SceneSingleton<BattleConnector> {
    public bool IsStarted { get; private set; } = false;

    public string OpponentIdFirebase { get; private set; }

    private async void Start() {
        NetworkManager net = NetworkManager.Singleton;
        net.OnClientConnectedCallback += OnClientConnectedCallback;
        if (AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
            OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetClientUnityId());
            net.StartHost();
        } else {
            OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetHostUnityId());
            net.StartClient();
        }
    }

    private async void OnClientConnectedCallback(ulong id) {
        NetworkManager net = NetworkManager.Singleton;
        if (net.ConnectedClients.Count == 2) {
            IsStarted = true;
            if (net.IsHost) {
                await LobbyHelper.Instance.DeleteHostedLobby();
            }
            net.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    public async Task WaitForDoneStart() {
        while (!IsStarted) await Task.Delay(200);
    }

    public void HandleResult(bool win) {
        SoundHelper.Play(win ? SoundType.Victory : SoundType.Lose);
        PopupFactory.ShowPopup_YesNo(
            win ? "BẠN ĐÃ THẮNG" : "BẠN ĐÃ THUA",
            win ? "+500 điểm danh vọng!" : "-500 điểm danh vọng",
            new() {
                content = "Về sảnh chính",
                callback = Exit,
                backgroundColor = Color.red,
                foregroundColor = Color.yellow,
            },
            new() {
                content = "Làm lại",
                callback = () => { print("Gạ gạ gạ"); },
                backgroundColor = Color.green,
            }
        );
    }
    
    public void Exit() {
        LobbyHelper.Instance.RelayHelper.Shutdown();
        LoadSceneHelper.LoadScene("LobbyScene");
    }

    private void OnDisable() {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    }
}
