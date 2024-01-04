using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services
{
    public static class SceneLoader
    {
        public static async Task LoadSceneAsync(Scenes scene)
        {
            await SceneManager.LoadSceneAsync(scene.ToString());
        }

        public static void LoadScene(Scenes scene)
        {
            SceneManager.LoadScene(scene.ToString());
        }

        public static void LoadSceneOnNetwork(Scenes scene)
        {
            if(NetworkManager.Singleton != null)
                NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
            else
                SceneManager.LoadScene(scene.ToString());
        }
    }

    public enum Scenes
    {
        Authentication,
        MainMenu,
        Lobby,
        Game
    }
}