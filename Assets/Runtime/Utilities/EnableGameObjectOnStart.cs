using UnityEngine;

namespace Weaver.Visuals.Utilities
{
    public class EnableGameObjectOnStart : MonoBehaviour
    {
        [SerializeField]
        private GameObject _gameObject = null!;

        private void Start()
        {
            if (_gameObject)
                _gameObject.SetActive(true);
        }
    }
}