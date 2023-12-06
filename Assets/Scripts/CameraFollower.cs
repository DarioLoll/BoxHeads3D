using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollower : MonoBehaviour
{
    
    [SerializeField] private Transform followObject;

    private void LateUpdate()
    {
        transform.position = followObject.position;
    }
}
