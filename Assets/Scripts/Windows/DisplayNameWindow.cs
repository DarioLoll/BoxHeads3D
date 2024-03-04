using System;
using Managers;
using Models;
using TMPro;
using UI;
using UnityEngine;

namespace Windows
{
    public class DisplayNameWindow : AnimatableWindow
    {
        [SerializeField] private TMP_InputField displayName;
        [SerializeField] private LoadingButton submitButtonContainer;

        public void SetDisplayName()
        {
            if (IsBusy) return;
            PlayFabManager.Instance.RequestFailed += OnRequestFailed;
            PlayFabManager.Instance.DisplayNameUpdated += OnDisplayNameUpdated;
            bool success = PlayFabManager.Instance.UpdateDisplayName(displayName.text);
            if (!success)
            {
                PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
                PlayFabManager.Instance.DisplayNameUpdated -= OnDisplayNameUpdated;
                return;
            }
            OnRequestSent(submitButtonContainer);
        }
        
        protected override void OnRequestProcessed()
        {
            PlayFabManager.Instance.RequestFailed -= OnRequestFailed;
            PlayFabManager.Instance.DisplayNameUpdated -= OnDisplayNameUpdated;
            base.OnRequestProcessed();
        }

        private void OnDisplayNameUpdated(PlayFabPlayer player)
        {
            OnRequestProcessed();
            UIManager.Instance.OnDisplayNameUpdated(player);
        }
    }
}
