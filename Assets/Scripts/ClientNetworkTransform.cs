using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{
    #region method
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
    #endregion
}
