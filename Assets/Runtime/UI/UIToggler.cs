using UnityEngine;
using UnityEngine.InputSystem;
using Weaver.UI.Navigation;

namespace Weaver
{
    public class UIToggler : MonoBehaviour
    {
        private WeaverInput _weaverInput = null!;

        [SerializeField]
        private NavigationController _navigationController = null!;

        [SerializeField]
        private string _transitionInto = string.Empty;

        [SerializeField]
        private GameObject? _debugUI;
        
        private void Awake()
        {
            _weaverInput = new WeaverInput();
            _weaverInput.Main.Enable();
            _weaverInput.Main.ToggleUI.performed += ToggleUIPerformed;
        }

        private void ToggleUIPerformed(InputAction.CallbackContext _)
        {
            if (_navigationController.HasPanel)
            {
                _navigationController.Hide(true);
                if (_debugUI != null)
                    _debugUI.SetActive(true);
            }
            else
            {
                _navigationController.gameObject.SetActive(true);
                _navigationController.NavigateTo(_transitionInto);
                if (_debugUI != null)
                    _debugUI.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _weaverInput.Main.Disable();
            _weaverInput.Dispose();
        }
    }
}