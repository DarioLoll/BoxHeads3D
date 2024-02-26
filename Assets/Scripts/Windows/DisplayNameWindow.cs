using System;
using Managers;
using Models;
using TMPro;

namespace Windows
{
    public class DisplayNameWindow : AnimatableWindow
    {
        public TMP_InputField displayName;

        public void SetDisplayName() => PlayFabManager.Instance.UpdateDisplayName(displayName.text);
    }
}
