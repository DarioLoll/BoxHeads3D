using System;
using Managers;
using Services;
using UnityEngine;

namespace ViewModels
{
    public class StartLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            PlayFabManager.Instance.LoadingComplete += LoadMenu;
        }

        private void LoadMenu()
        {
            //SceneLoader.LoadScene(Scenes.Menu);
        }

        private void OnDestroy()
        {
            PlayFabManager.Instance.LoadingComplete -= LoadMenu;
        }
    }
}
