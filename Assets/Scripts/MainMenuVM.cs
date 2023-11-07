using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVM : MonoBehaviour
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

    public async void StartOnlineGame()
    {
        try
        {
            await UnityServices.InitializeAsync();
            canvasOnline.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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