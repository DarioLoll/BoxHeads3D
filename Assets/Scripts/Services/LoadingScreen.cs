using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Services
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private GameObject textContainer;
        [SerializeField] private TMPro.TextMeshProUGUI text;
        [SerializeField] private Transform animatedDots;
        [SerializeField] private Animator loadingAnimator;
        [SerializeField] private RectTransform backgroundLeftPart;
        [SerializeField] private RectTransform backgroundRightPart;

        private RectTransform _textRectTransform;
        private const float TextYPosition = -400f;

        private TextAnimator _textAnimator;
        private static readonly int StartLoadingTrigger = Animator.StringToHash("StartLoading");
        private static readonly int ExitTrigger = Animator.StringToHash("Exit");
        private const float AnimationTime = 1f;
        private const float TextHiddenYPosition = 500;
        private const float BackgroundPartWidth = 2000;

        private bool _isDisplaying;
        /// <summary>
        /// The callback to be executed after the loading screen is closed. <br/>
        /// </summary>
        private Action _afterDisplayCallback;

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public static LoadingScreen Instance { get; private set; }

    
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _textAnimator = animatedDots.GetComponent<TextAnimator>();
            _textRectTransform = textContainer.GetComponent<RectTransform>();
            DontDestroyOnLoad(gameObject);
            Instance = this;
            GetComponent<Canvas>().enabled = true;
            DisplayLoadingScreen("Starting");
        }
    
        public void DisplayLoadingScreen(string loadingText, Action callback = null)
        {
            DisplayLoadingScreen(loadingText, () =>
            {
                callback?.Invoke();
                return Task.CompletedTask;
            });
        }
        
        public void DisplayLoadingScreen(string loadingText, Func<Task> callback)
        {
            if (gameObject.activeSelf) return;
            _isDisplaying = true;
            Text = loadingText;
            gameObject.SetActive(true);
            AnimateBackground(true);
            AnimateText(true, () =>
            {
                _isDisplaying = false;
                _textAnimator.StartAnimation();
                callback?.Invoke();
                _afterDisplayCallback?.Invoke();
            });
        }
    
        public void CloseLoadingScreen(Action callback = null)
        {
            if (!gameObject.activeSelf)
            {
                callback?.Invoke();
                return;
            }
            if (_isDisplaying)
            {
                _afterDisplayCallback = () => CloseAfterDisplaying(callback);
                return;
            }
            _textAnimator.StopAnimation();
            loadingAnimator.SetTrigger(ExitTrigger);
            AnimateBackground(false);
            AnimateText(false, () =>
            {
                gameObject.SetActive(false);
                callback?.Invoke();
            });
        }
        
        private void CloseAfterDisplaying(Action callback = null)
        {
            _isDisplaying = false;
            CloseLoadingScreen(callback);
        }
        

        private void AnimateText(bool enter, Action callback = null)
        {
            var from = enter ? TextHiddenYPosition : TextYPosition;
            var to = enter ? TextYPosition : TextHiddenYPosition;
            var textPosition = _textRectTransform.anchoredPosition;
            LeanTween.value(_textRectTransform.gameObject, from, to, AnimationTime * 1.5f)
                .setEaseOutCubic()
                .setOnUpdate((float value) => _textRectTransform.anchoredPosition = new Vector2(textPosition.x, value))
                .setOnComplete(() => callback?.Invoke());
        }
        
        private void AnimateBackground(bool enter)
        {
            float from = enter ? BackgroundPartWidth : 0;
            float to = enter ? 0 : BackgroundPartWidth;
            float y = backgroundLeftPart.anchoredPosition.y;
            LeanTween.value(backgroundLeftPart.gameObject, from, to, AnimationTime / 2f)
                .setEaseOutCubic()
                .setOnUpdate((float value) =>
                {
                    backgroundLeftPart.anchoredPosition = new Vector2(-value, y);
                    backgroundRightPart.anchoredPosition = new Vector2(value, y);
                });
        }
        
        private void StartLoading() => loadingAnimator.SetTrigger(StartLoadingTrigger);
        
        private void ExitLoading() => loadingAnimator.SetTrigger(ExitTrigger);
    }
}
