using Managers;
using UnityEngine;

namespace Models
{
    public abstract class RefresherBase : MonoBehaviour, IRefreshable
    {
        protected bool Initialized;
        public abstract void Refresh(float animationDuration = default);
        
        protected virtual void Initialize()
        {
            UIManager.Instance.ThemeChanged += _ => Refresh();
            Refresh();
            Initialized = true;
        }
        
        protected virtual void OnEnable()
        {
            if (!Initialized) return;
            Refresh();
        }
        
        protected virtual void Start()
        {
            if (UIManager.Instance == null)
            {
                UIManager.Initialized += Initialize;
            }
            else Initialize();
        }
    }
}
