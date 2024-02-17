using Services;
using UnityEngine;

namespace ViewModels
{
    public class StartLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SceneLoader.LoadScene(Scenes.Menu);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
