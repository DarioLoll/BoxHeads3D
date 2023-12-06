using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for handling player's rotation (looking around)
/// </summary>
public class PlayerRotator : MonoBehaviour
{
    /// <summary>
    /// The camera attached to the player
    /// </summary>
    [SerializeField]
    Camera cam;

    public float mouseSensitivityX;
    public float mouseSensitivityY;

    private float _xRotation = 0;
    private float _yRotation = 0;

    /// <summary>
    /// Handles the mouse movement for the player making him able to look around
    /// </summary>
    /// <param name="input">A normalized vector (both x and y must be between -1 and 1)</param>
    public void Look(Vector2 input)
    {
        //input.x = looking left/right = rotation around the y axis
        //input.y = looking up/down = rotation around the x axis
        
        //Looking down (input.y is negative) => xRotation increases
        //Looking up (input.y is positive) => xRotation decreases
        _xRotation -= input.y * mouseSensitivityY;
        //Looking left (input.x is negative) => yRotation decreases
        //Looking right (input.x is positive) => yRotation increases
        _yRotation += input.x * mouseSensitivityX;

        //Clamping the xRotation to prevent the player from looking too far up or down
        _xRotation = Mathf.Clamp(_xRotation, -80, 80);
        
        //For looking left/right, we should also rotate the player around the y axis
        //For looking up/down, we should only rotate the camera around the x axis
        cam.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        transform.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
