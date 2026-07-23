using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomLike.Interactables
{
    public class PlayerInteractionAgent : InteractionAgentBase
    {
        [SerializeField] private InteractionEventSO InteractableFocusedEvent;
        [SerializeField] private InteractionEventSO InteractableUnfocusedEvent;

        protected override void OnInteractableFocused(IInteractable interactable)
        {
            base.OnInteractableFocused(interactable);
            
            if (InteractableFocusedEvent == null)
                return;
            
            InteractableFocusedEvent.Raise(new InteractionPayload
            {
                Interactable = interactable,
                Interactor = this
            });
        }

        protected override void OnInteractableUnfocused(IInteractable interactable)
        {
            base.OnInteractableUnfocused(interactable);
            
            if (InteractableUnfocusedEvent == null)
                return;
            
            InteractableUnfocusedEvent.Raise(new InteractionPayload
            {
                Interactable = interactable,
                Interactor = this
            });
        }

        private void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Interact();
            }
        }
    }
}