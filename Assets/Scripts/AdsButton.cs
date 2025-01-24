using DG.Tweening;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsButton : MonoBehaviour {
    [SerializeField] private RectTransform Shine;
    [SerializeField] private RectTransform Gift;
    [SerializeField] private ClickyButton Button;

    private const string adUnitId = "ca-app-pub-5586839358803881/4941290356";
    private RewardedAd rewardedAd;
    

    private void LoadAds() {
        // clean up
        rewardedAd?.Destroy();
        rewardedAd = null;

        Debug.Log("Start load rewarded ad");

        RewardedAd.Load(adUnitId, new(), (ad, error) => {
            if (error != null || ad == null) {
                Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                return;
            }

            Debug.Log("Done load rewarded ad");

            rewardedAd = ad;
        });
    }

    private void OnClickAds() {
        if (rewardedAd?.CanShowAd() != true) {
            PopupFactory.ShowPopup_ManualBuild()
                .WithTitle("Quảng cáo đang tải, thử lại sau ít phút");
            return;
        }

        rewardedAd.OnAdFullScreenContentClosed += LoadAds;
        rewardedAd.OnAdFullScreenContentFailed += error => {
            Debug.LogError(error);
            LoadAds();
        };

        rewardedAd.Show(reward => {
            PopupFactory.ShowPopup_ManualBuild()
            .WithTitle("Chúc mừng bạn đã nhận được không gì cả")
            .WithButton(new() {
                backgroundColor = Color.green,
                content = "Tuyệt vời"
            }, true);
        });
    }

    private void Start() {
        MobileAds.Initialize(_ => { });
        LoadAds();
        Button.OnAfterClick.AddListener(OnClickAds);
        DoAnimation();
    }
    
    private void DoAnimation() {
        var pos = Gift.position;
        Sequence seq = DOTween.Sequence();
        seq.Append(Gift
            .DOMove(pos + new Vector3(0, 0.6f, 0), 0.4f)
            .SetEase(Ease.OutCubic));
        seq.Append(Gift
            .DOMove(pos, 0.4f)
            .SetEase(Ease.OutBounce));
        seq.AppendInterval(2);
        seq.SetLoops(-1);

        Shine
            .DORotate(new Vector3(0, 0, 180), 1)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }
}
