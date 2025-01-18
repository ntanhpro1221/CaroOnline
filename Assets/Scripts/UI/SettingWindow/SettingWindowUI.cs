using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BasePopup))]
public class SettingWindowUI : MonoBehaviour {
    [Header("---NAME----")]
    [SerializeField] private TextField _NameTxt;
    [SerializeField] private ButtonField _NameEditBtn;
    [SerializeField] private LabeledInputField _NameInput;
    [SerializeField] private ButtonField _NameSaveBtn;
    private bool _IsEditingName = false;
    private bool IsEditingName {
        get => _IsEditingName;
        set {
            _IsEditingName = value;
            _NameTxt.gameObject.SetActive(!value);
            _NameEditBtn.gameObject.SetActive(!value);
            _NameInput.gameObject.SetActive(value);
            _NameSaveBtn.gameObject.SetActive(value);
        }
    }

    [Space]
    [Header("--SETTING--")]
    [SerializeField] private ToggleField _MusicToggle;
    [SerializeField] private ToggleField _SoundToggle;
    [SerializeField] private ToggleField _VibrationToggle;

    [Space]
    [Header("---INFO----")]
    [SerializeField] private TextField _RankTxt;
    [SerializeField] private TextField _EloTxt;

    [Space]
    [Header("--SIGN OUT-")]
    [SerializeField] private ButtonField _SignOutBtn;

    public void Init() {
        UserData data = DataHelper.UserData;

        // Name field
        IsEditingName = false;
        _NameTxt.WithContent(data.name);
        _NameEditBtn.WithCallback(() => {
            IsEditingName = true;
            _NameInput.WithText(data.name);
        });
        _NameSaveBtn.WithCallback(async () => {
            IsEditingName = false;
            _NameTxt.WithContent(_NameInput.Text);
            DataHelper.UserData.name = _NameInput.Text;
            await DataHelper.SaveCurrentUserDataAsync();
        });

        // Setting
        _MusicToggle.WithValue(data.setting.music);
        _MusicToggle.WithCallback(async isOn => {
            DataHelper.UserData.setting.music = isOn;
            await DataHelper.SaveCurrentUserDataAsync();
        });
        _SoundToggle.WithValue(data.setting.sound);
        _SoundToggle.WithCallback(async isOn => {
            DataHelper.UserData.setting.sound = isOn;
            await DataHelper.SaveCurrentUserDataAsync();
        });
        _VibrationToggle.WithValue(data.setting.vibration);
        _VibrationToggle.WithCallback(async isOn => {
            DataHelper.UserData.setting.vibration = isOn;
            await DataHelper.SaveCurrentUserDataAsync();
        });

        // Info
        _RankTxt.WithContent("The licking");
        _EloTxt.WithContent("Elo: " + data.elo);
            
        // Sign out
        _SignOutBtn.WithCallback(() => {
            AsyncOperation operation = SceneManager.LoadSceneAsync("SignInScene");
            operation.allowSceneActivation = false;
            AuthHelper.SignOut(() => {
                PopupFactory.ShowSimplePopup("Đã đăng xuất");
                operation.allowSceneActivation = true;
            });
        });
    }
}
