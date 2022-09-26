using System;
using System.Linq;
using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using JetBrains.Annotations;
using UnityEngine;

namespace Weaver.UI.Navigation
{
    public class NavigationController : MonoBehaviour
    {
        [SerializeField]
        private NavigationPanel[] _panels = Array.Empty<NavigationPanel>();

        [SerializeField, Min(0.02f)]
        private float _transitionDuration = 0.5f;
        
        [SerializeField]
        private EaseType _transitionEasing = EaseType.CubicOut;

        [SerializeField]
        private string _defaultPanelId = string.Empty;
        
        private NavigationPanel? _activePanel;

        private void Start()
        {
            for (int i = 0; i < _panels.Length; i++)
            {
                var panel = _panels[i];
                var isDefault = panel.Id == _defaultPanelId;

                panel.Canvas.alpha = isDefault ? 1f : 0f;
                panel.Canvas.interactable = isDefault;
                panel.gameObject.SetActive(isDefault);

                if (isDefault)
                    _activePanel = panel;
            }
        }

        [UsedImplicitly]
        public void NavigateTo(string panelId)
        {
            // If the active panel is the panel we're trying to switch to, exit early. There's nothing to do.
            if (_activePanel != null && _activePanel.Id == panelId)
                return;
            
            // Get the panel that we want to switch to.
            var panel = _panels.FirstOrDefault(p => p.Id == panelId);
            if (panel == null)
                return;

            var activePanel = _activePanel;
            
            // If we have a panel that's already active, animate it out first.
            if (activePanel != null)
            {
                AnimatePanelOut(activePanel, () =>
                {
                    activePanel.gameObject.SetActive(false);
                    AnimatePanelIn(panel);
                });
            }
            else
            {
                // Animate in the requested panel.
                AnimatePanelIn(panel);
            }

            _activePanel = panel;
        }

        public void Hide()
        {
            // There's nothing to hide is there's nothing active.
            if (_activePanel == null)
                return;

            var activePanel = _activePanel;
            
            AnimatePanelOut(activePanel, () =>
            {
                activePanel.gameObject.SetActive(false);
            });
        }

        private void AnimatePanelIn(NavigationPanel panel)
        {
            panel.gameObject.SetActive(true);
            panel.Canvas.interactable = true;
            panel.Canvas
                .TweenCanvasGroupAlpha(1f, _transitionDuration * 0.5f)
                .SetEase(_transitionEasing);
        }
        
        private void AnimatePanelOut(NavigationPanel panel, Action onComplete)
        {
            panel.Canvas.interactable = false;
            panel.Canvas
                .TweenCanvasGroupAlpha(0f, _transitionDuration * 0.5f)
                .SetEase(_transitionEasing)
                .SetOnComplete(onComplete);
        }
    }
}