using UnityEngine;

namespace Weaver.Visuals.Utilities
{   
    public sealed class DisableGameObjectOnStart : MonoBehaviour
    {
        [SerializeField]
        private GameObject _gameObject = null!;

        private void Start()
        {
            if (_gameObject)
                _gameObject.SetActive(false);
        }
    }
}