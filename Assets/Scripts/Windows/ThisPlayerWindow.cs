using System;
using Managers;
using Models;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Windows
{
    public class ThisPlayerWindow : AnimatableWindow
    {
        public LobbyPlayer thisPlayer;

        private void OnEnable()
        {
            thisPlayer.SetPlayer(LobbyManager.Instance.ThisPlayer);
        }

        public void ChangeColor()
        {
            Color newColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            thisPlayer.SetColor(newColor);
        }
        
        public void ToggleReady() => thisPlayer.ToggleReady();
    }
}
