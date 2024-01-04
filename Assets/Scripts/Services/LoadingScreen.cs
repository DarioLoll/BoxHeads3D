using UnityEngine;

namespace Services
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI text;
        [SerializeField] private Transform animatedDots;

        private TextAnimator _textAnimator; 

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
            DontDestroyOnLoad(gameObject);
            Instance = this;
            GetComponent<Canvas>().enabled = true;
            CloseLoadingScreen();
        }
    
        public void DisplayLoadingScreen(string loadingText)
        {
            gameObject.SetActive(true);
            Text = loadingText;
            _textAnimator.StartAnimation();
        }
    
        public void CloseLoadingScreen()
        {
            _textAnimator.StopAnimation();
            gameObject.SetActive(false);
        }
    }
}
