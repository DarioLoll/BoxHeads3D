using System;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class DaytimeManager : NetworkBehaviour
    {
        public static DaytimeManager Instance { get; private set; }
        
        
        [SerializeField] private Light sun;
        
        [SerializeField, Range(0f, 24f)] private float timeOfDay;
        
        public float TimeOfDay
        {
            get => timeOfDay;
            set
            {
                timeOfDay = value;
                UpdateSun();
                UpdateLighting();
            }
        }
        
        [SerializeField] private float sunRotationSpeed = 1;
        
        [SerializeField] private Gradient sunColor;
        [SerializeField] private Gradient skyColor;
        [SerializeField] private Gradient equatorColor;
        [SerializeField] private Gradient fogColor;
        [SerializeField] private AnimationCurve fogDensity;

        [SerializeField] private bool isOn = true;

        public bool IsOn
        {
            get => isOn;
            set => isOn = value;
        }
        
        // Start is called before the first frame update
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isOn) return;
            if(!IsServer && !IsHost) return;
            
            timeOfDay += Time.deltaTime * sunRotationSpeed;
            timeOfDay %= 24;
            
            UpdateClientRpc(timeOfDay);
        }

        [ClientRpc]
        private void UpdateClientRpc(float time)
        {
            TimeOfDay = time;
        }

        private void UpdateSun()
        {
            float sunAngle = Mathf.Lerp(-90, 270, timeOfDay / 24);
            sun.transform.localRotation = Quaternion.Euler(sunAngle, 0, 0);
        }

        private void UpdateLighting()
        {
            RenderSettings.ambientLight = skyColor.Evaluate(timeOfDay / 24);
            RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeOfDay / 24);
            sun.color = sunColor.Evaluate(timeOfDay / 24);
            RenderSettings.fogColor = fogColor.Evaluate(timeOfDay / 24);
            RenderSettings.fogDensity = fogDensity.Evaluate(timeOfDay / 24);
        }

        /*private void OnValidate()
        {
            if (sunRotationSpeed <= 0)
            {
                sunRotationSpeed = 0.00001f;
            }
            if (sun != null)
            {
                UpdateSun();
                UpdateLighting();
            }
        }*/
    }
}
