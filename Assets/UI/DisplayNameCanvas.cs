using System.Collections;
using System.Collections.Generic;
using Managers;
using Models;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UI;
using UnityEngine;

public class DisplayNameCanvas : MonoBehaviour
{
    public TMP_InputField displayName;

    public void SetDisplayName() => PlayFabManager.Instance.UpdateDisplayName(displayName.text);
}
