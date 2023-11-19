using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TextAnimator : MonoBehaviour
{
    /// <summary>
    /// The text field that will be animated
    /// </summary>
    private TextMeshProUGUI _textField;
    
    /// <summary>
    /// List of all keyframes (points) in the animation
    /// </summary>
    public List<string> keyFrames = new List<string>();
    
    /// <summary>
    /// What the text should be when the animation is not running
    /// </summary>
    public string textOnIdle;
    
    /// <summary>
    /// How long each frame should be displayed
    /// </summary>
    public float timeBetweenFrames;
    
    private bool _isAnimating = false;
    private float _timer;
    private int _currentFrame = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _textField = GetComponent<TextMeshProUGUI>();
    }
    
    public void StartAnimation()
    {
        if (_isAnimating) return;
        _timer = timeBetweenFrames;
        _currentFrame = 0;
        _isAnimating = true;
    }

    public void StopAnimation()
    {
        if (!_isAnimating) return;
        _isAnimating = false;
        _textField.text = textOnIdle;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isAnimating) return;
        if (_timer <= 0)
        {
            _timer = timeBetweenFrames;
            if (_currentFrame >= keyFrames.Count - 1) _currentFrame = 0;
            else _currentFrame++;
            _textField.text = keyFrames[_currentFrame];
        }
        else _timer -= Time.deltaTime;
    }
}
