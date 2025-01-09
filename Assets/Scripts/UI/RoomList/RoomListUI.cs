using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        _SyncRoomTask = new(StartSyncRoomList);
    }
     
    public async Task RefreshList() {
        var taskQuery = LobbyHelper.Instance.QueryLobbies();

        List<Lobby> listRoom = await taskQuery;
        List<Lobby> addRoom = new();
        List<string> removeRoom = new();

        foreach (Lobby room in listRoom)
            if (_ElementDict.ContainsKey(room.Id))
                _ElementDict[room.Id].UpdateFull(room.Name);
            else addRoom.Add(room);

        foreach (var (id, elementUI) in _ElementDict)
            if (!listRoom.Any(room => room.Id == id))
                removeRoom.Add(id);
        
        foreach (string id in removeRoom) Remove(id);
        foreach (Lobby room in addRoom) Add(room);
    }

    private async Task StartSyncRoomList(CancellationToken token) {
        await LobbyHelper.Instance.WaitForInit();

        while (!token.IsCancellationRequested) await Task.WhenAll(
            RefreshList(),
            Task.Delay(LobbyHelper.RATE_QUERY));
    }

    private void Add(Lobby room) {
        RoomElementUI elementUI = Instantiate(_ElementObj, _ElementHolder).GetComponent<RoomElementUI>();
        elementUI.BuildFull(room.Name, async () => await LobbyHelper.Instance.JoinLobbyById(room.Id));
        _ElementDict.Add(room.Id, elementUI);
    }

    private void Remove(string roomId) {
        Destroy(_ElementDict[roomId].gameObject);
        _ElementDict.Remove(roomId);
    }

    private void OnDisable() {
        _SyncRoomTask.Cancel();
    }
}
