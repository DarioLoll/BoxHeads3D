using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizer : MonoBehaviour
{
    private Transform _body;
    private SpriteRenderer _bodySpriteRenderer;
    
    public Color PlayerColor
    {
        get => _bodySpriteRenderer.color;
        set => _bodySpriteRenderer.color = value;
    }
    
    void Awake()
    {
        _body = transform.GetChild(1);
        _bodySpriteRenderer = _body.GetComponent<SpriteRenderer>();
    }
    
}
