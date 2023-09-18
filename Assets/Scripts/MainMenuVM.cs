using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuVM : MonoBehaviour
{
    [SerializeField]
    public GameObject canvasMenu;

    [SerializeField]
    public GameObject canvasOnline;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartOfflineGame()
    {
        SceneManager.LoadScene("Match");
    }

    public void StartOnlineGame()
    {
        canvasMenu.SetActive(false);
        canvasOnline.SetActive(true);
    }

    public void Back()
    {
        canvasMenu.SetActive(true);
        canvasOnline.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
