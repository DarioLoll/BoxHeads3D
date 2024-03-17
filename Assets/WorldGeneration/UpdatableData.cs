using UnityEngine;

namespace WorldGeneration
{
    public class UpdatableData : ScriptableObject
    {
        public event System.Action OnValuesUpdated;
        public bool autoUpdate;

        protected virtual void OnValidate()
        {
            if (autoUpdate)
            {
                NotifyOfUpdatedValues();
            }
        }

        public void NotifyOfUpdatedValues()
        {
            OnValuesUpdated?.Invoke();
        }
    }
}
