using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVM : MonoBehaviour
{
    #region instancevariable
    [SerializeField]
    private GameObject canvasOnline;
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
