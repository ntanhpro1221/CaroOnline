using System.Threading.Tasks;
using UnityEngine;

public class BattleToolUI : SceneSingleton<BattleToolUI> {
    [SerializeField] private ButtonField _ExitBtn;
    [SerializeField] private ButtonField _ProfileOfOtherBtn;

    private bool _IsPlayerVSPlayer
        => DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Player;
    private bool _IsPlayerVSBot
        => DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Bot;

    private void OnClickExit() {
        if (_IsPlayerVSPlayer) {
            PopupFactory.ShowPopup_YesNo(
                "Bạn có chắc muốn thoát trận?",
                "Bạn sẽ bị coi như đã thua!",
                new() {
                    content = "Thoát",
                    callback = async () => {
                        BattleConnector.Instance.Surrender();

                        await Task.WhenAll(
                            BattleConnector.Instance.WaitForOpponentHandleResult(),
                            BattleConnector.Instance.WaitForMeHandleResult());

                        BattleConnector.Instance.Exit();
                    },
                    backgroundColor = Color.red,
                    foregroundColor = Color.yellow,
                },
                new() {
                    content = "Thôi chơi tiếp",
                    backgroundColor = Color.green,
                });
        } else if (_IsPlayerVSBot) {
            PopupFactory.ShowPopup_YesNo(
                "Bạn có chắc muốn thoát trận?",
                null,
                new() {
                    content = "Thoát",
                    callback = BattleConnector.Instance.Exit,
                    backgroundColor = Color.red,
                    foregroundColor = Color.yellow,
                },
                new() {
                    content = "Thôi chơi tiếp",
                    backgroundColor = Color.green,
                });
        }
    }

    private async void OnClickProfileOfOther() {
        await BattleConnector.Instance.WaitForDoneStart();

        UserData data = await DataHelper.LoadUserDataAsync(BattleConnector.Instance.OpponentIdFirebase);

        PopupFactory.ShowProfileOfOther(data);
    }

    private void Start() {
        _ExitBtn.WithCallback(OnClickExit);
        if (_IsPlayerVSPlayer) {
            _ProfileOfOtherBtn.gameObject.SetActive(true);
            _ProfileOfOtherBtn.WithCallback(OnClickProfileOfOther);
        } else if (_IsPlayerVSBot) {
            _ProfileOfOtherBtn.gameObject.SetActive(false);
        }
    }
}