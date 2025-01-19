using System.Threading.Tasks;
using UnityEngine;

public class BattleToolUI : Singleton<BattleToolUI> {
    [SerializeField] private ButtonField _ExitBtn;
    [SerializeField] private ButtonField _ProfileOfOtherBtn;
    
    private void OnClickExit() {
        PopupFactory.ShowPopup_YesNo(
            "Bạn có chắc muốn thoát trận?",
            "Bạn sẽ bị coi như đã thua!",
            new() {
                content = "Thoát",
                callback = async () => {
                    print("Bắt đầu thoát");
                    BattleConnector.Instance.Surrender();

                    print("Đầu hàng xong");
                    await Task.WhenAll(
                        BattleConnector.Instance.WaitForOpponentHandleResult(),
                        BattleConnector.Instance.WaitForMeHandleResult());

                    print("đợi đối phương đầu hàng xong");

                    BattleConnector.Instance.Exit();

                    print("thoát xong");
                },
                backgroundColor = Color.red,
                foregroundColor = Color.yellow,
            },
            new() {
                content = "Thôi chơi tiếp",
                backgroundColor = Color.green, 
            });
    }

    private async void OnClickProfileOfOther() {
        await BattleConnector.Instance.WaitForDoneStart();

        UserData data = await DataHelper.LoadUserDataAsync(BattleConnector.Instance.OpponentIdFirebase);

        PopupFactory.ShowProfileOfOther(data);
    }

    private void Start() {
        _ExitBtn.WithCallback(OnClickExit); 
        _ProfileOfOtherBtn.WithCallback(OnClickProfileOfOther);
    }
}