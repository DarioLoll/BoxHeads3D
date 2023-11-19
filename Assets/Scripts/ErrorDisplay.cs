using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    public static ErrorDisplay Instance;

    public GameObject errorPopupPrefab;
    
    public GameObject mainCanvas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void DisplayError(string errorMessage, Action onClose = null)
    {
        var errorPopup = Instantiate(errorPopupPrefab.transform, mainCanvas.transform);
        var errorPopupVm = errorPopup.GetComponent<ErrorPopupVm>();
        errorPopupVm.OnClose = onClose;
        errorPopupVm.DisplayError(errorMessage);
    }

    public void DisplayInfo(string infoMessage, Action onClose = null)
    {
        var errorPopup = Instantiate(errorPopupPrefab.transform, mainCanvas.transform);
        var errorPopupVm = errorPopup.GetComponent<ErrorPopupVm>();
        errorPopupVm.OnClose = onClose;
        errorPopupVm.DisplayInfo(infoMessage);
    }
    
    
    public void DisplayLobbyError(LobbyServiceException exception)
    {
        switch (exception.Reason)
        {
            case LobbyExceptionReason.AlreadySubscribedToLobby:
                DisplayError("You are already in this lobby.\n Try restarting your game.");
                break;
            case LobbyExceptionReason.AlreadyUnsubscribedFromLobby:
                DisplayError("You already left the lobby.\n Try restarting your game.");
                break;
            case LobbyExceptionReason.LobbyConflict:
            case LobbyExceptionReason.Conflict:
                DisplayError("An unknown error occured.\n Try restarting your game");
                break;
            case LobbyExceptionReason.EntityNotFound:
                DisplayError("The lobby you are trying to join does not exist.");
                break;
            case LobbyExceptionReason.IncorrectPassword:
                DisplayError("The password you entered is incorrect.");
                break;
            case LobbyExceptionReason.ValidationError:
            case LobbyExceptionReason.InvalidJoinCode:
                DisplayError("The lobby code you entered is invalid.");
                break;
            case LobbyExceptionReason.LobbyAlreadyExists:
                DisplayError("A lobby with this name already exists.");
                break;
            case LobbyExceptionReason.LobbyFull:
                DisplayError("The lobby you are trying to join is full.");
                break;
            case LobbyExceptionReason.LobbyNotFound:
                DisplayError("The lobby you are trying to join does not exist.");
                break;
            case LobbyExceptionReason.NoOpenLobbies:
                DisplayError("There are no open lobbies at the moment.\n Try creating one.");
                break;
            case LobbyExceptionReason.NetworkError:
                DisplayError("Couldn't connect to the server.\n Check your internet connection and try again.");
                break;
            case LobbyExceptionReason.RateLimited:
                DisplayError("You are sending too many requests to the server.\n Try again in a few seconds.");
                break;
            default:
                DisplayError("An unknown error occured.\n Try restarting your game.");
                break;
        }
    }
}
