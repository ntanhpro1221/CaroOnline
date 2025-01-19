using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class LoadSceneHelper : Singleton<LoadSceneHelper> {
    [SerializeField] private RectTransform _ImageObj;
    [SerializeField] private Image _BlankImg;
    [SerializeField] private float _Duration = 0.2f;
    private static LoadStyle _CurLoadStyle;

    protected override void Awake() {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private static RectTransform FillRect(RectTransform transform) {
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.one;
        transform.offsetMin = Vector2.zero;
        transform.offsetMax = Vector2.zero;
        transform.position = new(Screen.width / 2, Screen.height * 1.5f);
        return transform;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        switch (_CurLoadStyle) {
            case LoadStyle.Fade:
                Instance._BlankImg
                    .DOFade(0, Instance._Duration);
                break;
            case LoadStyle.Image:
               Instance._ImageObj
                    .DOMove(new(Screen.width / 2, Screen.height * 1.5f), Instance._Duration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Instance._ImageObj.gameObject.SetActive(false));
                break;
        }
    }
    
    public static void LoadScene(string sceneName, LoadStyle loadStyle = LoadStyle.Fade, Sprite imageToShow = null) {
        // SET LOAD STYLE
        _CurLoadStyle = loadStyle;
        
        // START LOAD SCENE ASYNC BUT DONT ALLOW CHANGE SCENE
        AsyncOperation loadTask = SceneManager.LoadSceneAsync(sceneName);
        loadTask.allowSceneActivation = false;

        // DO SOME THING WHEN LOAD SCENE
        Sequence sequence = DOTween.Sequence();
        switch (loadStyle) {
            case LoadStyle.Fade:
                sequence.Append(Instance._BlankImg
                    .DOFade(1, Instance._Duration));
                break;
            case LoadStyle.Image:
                FillRect(Instance._ImageObj).gameObject.SetActive(true);
                sequence.Append(Instance._ImageObj
                    .DOMove(new(Screen.width / 2, Screen.height / 2), Instance._Duration)
                    .SetEase(Ease.OutBack));
                if (imageToShow != null) Instance._ImageObj.GetComponent<Image>().sprite = imageToShow;
                break;
        }
        
        // ALLOW CHANGE SCENE AFTER DONE
        sequence.AppendCallback(() => loadTask.allowSceneActivation = true);
    }

    public enum LoadStyle {
        None,
        Fade,
        Image,
    }
}
