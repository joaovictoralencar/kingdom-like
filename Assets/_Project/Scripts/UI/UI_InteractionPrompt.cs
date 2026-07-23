using System;
using HelloDev.UI.Default;
using KingdomLike.Interactables;
using UnityEngine;

namespace KingdomLike.UI
{
    public class UI_InteractionPrompt : MonoBehaviour
    {
        [SerializeField] private UIContainer Container;
        
        [SerializeField] private InteractionEventSO InteractableFocusedEvent;
        [SerializeField] private InteractionEventSO InteractableUnfocusedEvent;

        private void Awake()
        {
            InteractableFocusedEvent.AddListener(OnInteractableFocused);
            InteractableUnfocusedEvent.AddListener(OnInteractableUnfocused);
        }
        
        private void OnDestroy()
        {
            InteractableFocusedEvent.RemoveListener(OnInteractableFocused);
            InteractableUnfocusedEvent.RemoveListener(OnInteractableUnfocused);
        }

        private void OnInteractableFocused(InteractionPayload payload)
        {
            Container.Show();
        }

        private void OnInteractableUnfocused(InteractionPayload payload)
        {
            Container.Hide();
        }
    }
}