using UnityEngine;

namespace Managers
{
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager Instance;
    
        // Start is called before the first frame update
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
