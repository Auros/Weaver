using UnityEngine;

namespace Weaver.UI.Navigation
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class NavigationPanel : MonoBehaviour
    {
        [field: SerializeField]
        public string Id { get; private set; } = string.Empty;

        private CanvasGroup _canvasGroup = null!;

        public CanvasGroup Canvas
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }
    }
}