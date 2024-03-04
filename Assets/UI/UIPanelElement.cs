using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelElement : MonoBehaviour
    {

        private EventSystem _eventSystem;
        [SerializeField] private Selectable firstSelected;
        [SerializeField] private Button submitButton;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
            {
                try
                {
                    _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp().Select();
                }
                catch (Exception)
                {
                    // no need to handle
                }
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                try
                {
                    _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown().Select();
                }
                catch (Exception)
                {
                    // no need to handle
                }
            }
            else if (Input.GetKeyDown(KeyCode.Return)) 
                submitButton.onClick.Invoke();
        
        }

        private void Start()
        {
            _eventSystem = EventSystem.current;
            if(firstSelected != null)
                firstSelected.Select();
        }
        
    }
}
