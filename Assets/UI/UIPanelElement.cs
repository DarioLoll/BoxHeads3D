using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelElement : MonoBehaviour
    {

        private EventSystem _eventSystem;
        public Selectable firstSelected;
        public Button submitButton;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
            {
                try
                {
                    _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp().Select();
                }
                catch (Exception e)
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
                catch (Exception e)
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
            firstSelected.Select();
        }
        
    }
}
