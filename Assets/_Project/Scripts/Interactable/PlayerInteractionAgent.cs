using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomLike.Interactables
{
    public class PlayerInteractionAgent : InteractionAgentBase
    {
        [SerializeField] private InteractionEventSO InteractableFocusedEvent;
        [SerializeField] private InteractionEventSO InteractableUnfocusedEvent;

        private InteractionHoldController _holdController;

        [SerializeField] private InputActionReference _interactAction;

        protected override void Awake()
        {
            base.Awake();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;

            TryGetComponent(out _holdController);
        }

        private void OnEnable()
        {
            if (_interactAction != null && _interactAction.action != null)
            {
                _interactAction.action.started += OnInteractStarted;
                _interactAction.action.canceled += OnInteractCanceled;
                _interactAction.action.Enable();
            }
        }

        protected override void OnDisable()
        {
            if (_interactAction != null && _interactAction.action != null)
            {
                _interactAction.action.started -= OnInteractStarted;
                _interactAction.action.canceled -= OnInteractCanceled;
                _interactAction.action.Disable();
            }
        }

        protected override void OnInteractableFocused(IInteractable interactable)
        {
            base.OnInteractableFocused(interactable);
            
            if (InteractableFocusedEvent == null)
                return;
            
            InteractableFocusedEvent.Raise(new InteractionPayload
            {
                Interactable = interactable,
                Interactor = this,
                IsFocused = true,
                IsInteracting = false
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
                Interactor = this,
                IsFocused = false,
                IsInteracting = false
            });
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            // Prefer hold controller if present. If not present, fall back to immediate interact.
            if (_holdController != null)
            {
                _holdController.TryStartHold(FocusedInteractable);
            }
            else
            {
                Interact();
            }
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            if (_holdController != null)
                _holdController.StopHold();
        }
    }
}