using UnityEngine;

namespace UI
{
    public class LoadingButton : MonoBehaviour
    {
        [SerializeField] private GameObject logo;
        private Animator _logoAnimator;

        private void Awake()
        {
            _logoAnimator = logo.GetComponent<Animator>();
        }
    
        public void StartLoading() => logo.SetActive(true);

        public void StopLoading() => logo.SetActive(false);
    }
}
