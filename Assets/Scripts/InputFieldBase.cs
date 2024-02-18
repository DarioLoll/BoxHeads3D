using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldBase : MonoBehaviour
{
    
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI inputFieldText;
    private RectTransform _inputFieldRect;
    [SerializeField] private Image inputFieldOutline;
    public Image symbolImage;

    // Start is called before the first frame update
    void Awake()
    {
        _inputFieldRect = inputField.GetComponent<RectTransform>();
    }

    public void OnInputFieldGotFocus()
    {
        UIManager ui = UIManager.Instance;
        LeanTween.value(inputFieldOutline.gameObject, ui.transparent, ui.PrimaryBackgroundColor, ui.FadeBaseDuration)
            .setOnUpdateColor(color => inputFieldOutline.color = color);
        LeanTween.value(symbolImage.gameObject, ui.baseForeground, ui.PrimaryBackgroundColor, ui.FadeBaseDuration)
            .setOnUpdateColor(color => symbolImage.color = color);
        if (!string.IsNullOrEmpty(inputField.text))
        {
            LeanTween.value(inputField.gameObject, ui.baseForeground, ui.highlightedForeground, ui.FadeBaseDuration)
                .setOnUpdateColor(color => inputFieldText.color = color);
        }
    }
    
    public void OnInputFieldLostFocus()
    {
        UIManager ui = UIManager.Instance;
        LeanTween.value(inputFieldOutline.gameObject, ui.PrimaryBackgroundColor, ui.transparent, ui.FadeBaseDuration)
            .setOnUpdateColor(color => inputFieldOutline.color = color);
        LeanTween.value(symbolImage.gameObject, ui.PrimaryBackgroundColor, ui.baseForeground, ui.FadeBaseDuration)
            .setOnUpdateColor(color => symbolImage.color = color);
        if (!string.IsNullOrEmpty(inputField.text))
        {
            LeanTween.value(inputField.gameObject, ui.highlightedForeground, ui.baseForeground, ui.FadeBaseDuration)
                .setOnUpdateColor(color => inputFieldText.color = color);
        }
    }
    
}
