using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class RoomListUI : SceneSingleton<RoomListUI> {
    [SerializeField] private GameObject _ElementObj;
    private Transform _ElementHolder;
    private Dictionary<string, RoomElementUI> _ElementDict = new();
    private CancellableTask _SyncRoomTask;

    protected override void Awake() {
        base.Awake();

        _ElementHolder = GetComponent<ScrollRect>().content;

        if (ConnectionChecker.CachedInternetCheckResult)
            _SyncRoomTask = new(StartSyncRoomList);
    }
     
    public async Task RefreshList() {
        var taskQuery = LobbyHelper.Instance.QueryLobbies();

        List<Lobby> listRoom = await taskQuery;
        if (listRoom == null) return;

        List<Lobby> addRoom = new();
        List<string> removeRoom = new();

        listRoom.RemoveAll(lobby => lobby.HostId == AuthenticationService.Instance.PlayerId);

        foreach (Lobby room in listRoom)
            if (!_ElementDict.ContainsKey(room.Id))
                addRoom.Add(room);

        foreach (var (id, elementUI) in _ElementDict)
            if (!listRoom.Any(room => room.Id == id))
                removeRoom.Add(id);
        
        foreach (string id in removeRoom) Remove(id);
        await Task.WhenAll(addRoom.Select(room => Add(room)));
    }

    private async Task StartSyncRoomList(CancellationToken token) {
        while (!token.IsCancellationRequested) await Task.WhenAll(
            RefreshList(),
            Task.Delay(LobbyHelper.RATE_QUERY));
    }

    private async Task Add(Lobby room) {
        RoomElementUI elementUI = Instantiate(_ElementObj, _ElementHolder).GetComponent<RoomElementUI>();
        elementUI
            .WithProfile(await DataHelper.LoadUserDataAsync(await DataHelper.UnityToFirebase(room.HostId)))
            .WithPlayCallback(async () => await LobbyHelper.Instance.JoinLobbyById(room.Id));
        _ElementDict.Add(room.Id, elementUI);
    }

    private void Remove(string roomId) {
        Destroy(_ElementDict[roomId].gameObject);
        _ElementDict.Remove(roomId);
    }

    private void OnDisable() {
        _SyncRoomTask?.Cancel();
    }
}
