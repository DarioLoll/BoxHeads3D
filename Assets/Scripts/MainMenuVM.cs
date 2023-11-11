using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVm : MonoBehaviour
{
    #region fields

    [SerializeField] private GameObject canvasOnline;

    [SerializeField] private GameObject canvasLobby;

    #endregion

    #region methods

    public void StartOfflineGame()
    {
        SceneManager.LoadScene("Match");
    }

    public void StartOnlineGame()
    {
        canvasOnline.SetActive(true);
    }

    public void CreateLobby()
    {
        canvasOnline.SetActive(false);
        canvasLobby.SetActive(true);
    }

    public void BackToCanvasOnline()
    {
        canvasOnline.SetActive(true);
        canvasLobby.SetActive(false);
    }

    public void Back()
    {
        canvasOnline.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion
}