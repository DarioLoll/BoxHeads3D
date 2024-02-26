using System;
using System.Collections.Generic;
using Managers;
using Models;
using TMPro;
using UI;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Windows
{
    public class LobbyOptionsWindow : AnimatableWindow
    {
        [SerializeField] private AnimatableWindow leftWindow;
        [SerializeField] private AnimatableWindow rightWindow;
        [SerializeField] private TMP_InputField lobbyCode;
        [SerializeField] private ButtonBase _lobbyElement;
        [SerializeField] private GameObject _lobbyElementContainer;
        [SerializeField] private GameObject _noLobbiesFoundPrefab;
        
        private float _refreshTimer = RefreshInterval;
        private const float RefreshInterval = 2.0f;
        
        private void Update()
        {
            if (_refreshTimer > 0)
            {
                _refreshTimer -= Time.deltaTime;
                return;
            }
            _refreshTimer = RefreshInterval;
            RefreshLobbyList();
        }
        
        
        public override void Enter(Action onComplete = null)
        {
            gameObject.SetActive(true);
            RefreshLobbyList();
            UIManager.Instance.ChangeMainTitle("Lobby Options");
            leftWindow.Enter();
            rightWindow.Enter(onComplete);
        }
        
        public override void Exit(Action onComplete = null)
        {
            Action callback = () =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            };
            UIManager.Instance.ChangeMainTitle("Celestial Echo");
            leftWindow.Exit();
            rightWindow.Exit(callback);
            
        }

        public async void JoinLobbyByCode()
        {
            await LobbyManager.Instance.JoinLobbyByCode(lobbyCode.text);
        }

        public async void RefreshLobbyList()
        {
            List<Lobby> lobbies = await LobbyManager.Instance.GetOpenLobbies();
            DestroyAllLobbyElements();
            if (lobbies.Count == 0)
            {
                Instantiate(_noLobbiesFoundPrefab, _lobbyElementContainer.transform);
                return;
            }
            AddLobbyElements(lobbies);
        }
        
        private void DestroyAllLobbyElements()
        {
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = 0; i < _lobbyElementContainer.transform.childCount; i++)
            {
                toDestroy.Add(_lobbyElementContainer.transform.GetChild(i).gameObject);
            }
            toDestroy.ForEach(Destroy);
        }

        private void AddLobbyElements(List<Lobby> lobbies)
        {
            for (int i = 0; i < lobbies.Count; i++)
            {
                Lobby lobby = lobbies[i];
                LobbyElement lobbyElement = Instantiate(_lobbyElement, _lobbyElementContainer.transform).GetComponent<LobbyElement>();
                lobbyElement.SetLobby(lobby);
            }
        }
    }
}
