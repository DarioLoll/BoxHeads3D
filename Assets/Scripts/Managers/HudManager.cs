using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Managers
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] private List<Image> hearts;
        [SerializeField] private Slider hungerSlider;
        [SerializeField] private Slider thirstSlider;
        [SerializeField] private TextMeshProUGUI hitText;
        [SerializeField] private CanvasGroup gameOverCanvas;
        [SerializeField] private GameObject interactElement;
        [SerializeField] private Image interactIcon;
        [SerializeField] private TextMeshProUGUI interactText;
        
        public Sprite WaterIcon { get; private set; }
        

        public const float HitTextRotationRange = 45;
        public const float HitTextPositionRange = 300;
        public const float HitTextDuration = 0.5f;

        public static HudManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("There can only be one HudManager.");
            }
            Instance = this;
            LoadIcons();
        }

        private void LoadIcons()
        {
            WaterIcon = Resources.Load<Sprite>("water-transparent");
        }

        private void Start()
        {
            gameOverCanvas.gameObject.SetActive(false);
        }

        public void UpdateHud(float health, float hungerPoints, float thirstPoints)
        {
            UpdateHp(health);
            hungerSlider.value = 1 - hungerPoints / GamePlayer.MaxHungerAndThirstPoints;
            thirstSlider.value = 1 - thirstPoints / GamePlayer.MaxHungerAndThirstPoints;
        }
        
        public void UpdateHp(float health)
        {
            float healthLeft = health;
            float healthPerHeart = GamePlayer.MaxHealth / hearts.Count;
            for (int i = 0; i < hearts.Count; i++)
            {
                if (healthLeft >= healthPerHeart)
                {
                    hearts[i].fillAmount = 1;
                    healthLeft -= healthPerHeart;
                }
                else if (healthLeft > 0)
                {
                    hearts[i].fillAmount = healthLeft / healthPerHeart;
                    healthLeft = 0;
                }
                else
                {
                    hearts[i].fillAmount = 0;
                }
            }
        }

        public void DisplayHit(int damage)
        {
            LeanTween.cancel(hitText.gameObject);
            hitText.text = damage.ToString();
            hitText.gameObject.SetActive(true);
            var randomRotation = Random.Range(-HitTextRotationRange, HitTextRotationRange);
            hitText.transform.rotation = Quaternion.Euler(0, 0, randomRotation);
            var randomX = Random.Range(-HitTextPositionRange, HitTextPositionRange);
            var randomY = Random.Range(-HitTextPositionRange, HitTextPositionRange);
            hitText.transform.localPosition = new Vector3(randomX, randomY, 0);
            
            var randomMovementIn150Radius = new Vector3(Random.Range(-HitTextPositionRange, HitTextPositionRange),
                Random.Range(-HitTextPositionRange, HitTextPositionRange), 0);
            LeanTween.moveLocal(hitText.gameObject, randomMovementIn150Radius, HitTextDuration)
                .setEaseOutExpo()
                .setOnComplete(() => hitText.gameObject.SetActive(false));
        }
        
        public void ShowInteractable(string text, Sprite icon)
        {
            interactElement.SetActive(true);
            interactText.text = text;
            interactIcon.sprite = icon;
        }
        
        public void HideInteractable()
        {
            interactElement.SetActive(false);
        }
        
        public void DisplayGameOver(Action callback = null)
        {
            Cursor.lockState = CursorLockMode.None;
            gameOverCanvas.gameObject.SetActive(true);
            gameOverCanvas.alpha = 0;
            LeanTween.alphaCanvas(gameOverCanvas, 1, 1f).setOnComplete(callback);
        }
        
        public void OnRespawnClicked()
        {
            GamePlayer.ThisPlayer!.RespawnServerRpc();
            Cursor.lockState = CursorLockMode.Locked;
            LeanTween.alphaCanvas(gameOverCanvas, 0, 1f).setOnComplete(() =>
            {
                gameOverCanvas.gameObject.SetActive(false);
            });
        }

    }
}
