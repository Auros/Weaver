using UnityEngine;

namespace Weaver.UI.Navigation
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class NavigationPanel : MonoBehaviour
    {
        [field: SerializeField]
        public string Id { get; private set; } = string.Empty;

        public CanvasGroup Canvas { get; private set; } = null!;
        
        private void Awake()
        {
            Canvas = GetComponent<CanvasGroup>();
        }
    }
}