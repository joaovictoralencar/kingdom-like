using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomLike.Interactables
{
    public class PlayerInteractionAgent : InteractionAgentBase
    {
        [Header("Interaction Events")] [SerializeField]
        private InteractionEventSO _interactableFocusedEvent;

        [SerializeField] private InteractionEventSO _interactableUnfocusedEvent;

        [Header("Input")] [SerializeField] private InputActionReference _interactAction;

        private InteractionHoldController _holdController;

        protected override void Awake()
        {
            base.Awake();

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;

            TryGetComponent(out _holdController);
        }

        private void OnEnable()
        {
            if (_interactAction == null || _interactAction.action == null)
                return;

            _interactAction.action.started += OnInteractStarted;
            _interactAction.action.canceled += OnInteractCanceled;
            _interactAction.action.Enable();
        }

        protected override void OnDisable()
        {
            if (_interactAction != null && _interactAction.action != null)
            {
                _interactAction.action.started -= OnInteractStarted;
                _interactAction.action.canceled -= OnInteractCanceled;
                _interactAction.action.Disable();
            }

            _holdController?.StopHold();

            base.OnDisable();
        }

        protected override void OnInteractionTargetFocused(IInteractionTarget target)
        {
            base.OnInteractionTargetFocused(target);

            _interactableFocusedEvent?.Raise(
                new InteractionPayload
                {
                    Target = target,
                    Interactor = this,
                    IsFocused = true,
                    IsInteracting = false
                });
        }

        protected override void OnInteractionTargetUnfocused(IInteractionTarget target)
        {
            base.OnInteractionTargetUnfocused(target);

            _interactableUnfocusedEvent?.Raise(
                new InteractionPayload
                {
                    Target = target,
                    Interactor = this,
                    IsFocused = false,
                    IsInteracting = false
                });
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            IInteractionTarget target = FocusedTarget;

            if (target == null)
                return;

            if (_holdController != null && _holdController.TryStartHold(target))
            {
                return;
            }

            Interact();
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            _holdController?.StopHold();
        }
    }
}