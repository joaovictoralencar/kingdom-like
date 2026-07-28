using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base class for entities capable of detecting, selecting,
    /// focusing, and interacting with interactables.
    /// </summary>
    public abstract class InteractionAgentBase : MonoBehaviour, IInteractionCandidate, IFocusableInteractor
    {
        [Header("Interaction")] [SerializeField]
        private InteractionSelectorSO _interactionSelector;

        private readonly List<IInteractable> _interactionCandidates = new();

        private IInteractable _focusedInteractable;

        public GameObject InteractorObject => gameObject;

        public IInteractable FocusedInteractable => _focusedInteractable;

        protected virtual void Awake()
        {
            if (_interactionSelector == null)
            {
                Debug.LogError(
                    $"{name} has no interaction selector assigned.",
                    this);
            }
        }

        public void AddInteractionCandidate(IInteractable interactable)
        {
            if (interactable == null)
                return;

            if (_interactionCandidates.Contains(interactable))
                return;

            _interactionCandidates.Add(interactable);

            RefreshInteractionCandidates();
        }

        public void RemoveInteractionCandidate(IInteractable interactable)
        {
            if (interactable == null)
                return;

            if (!_interactionCandidates.Remove(interactable))
                return;

            if (_focusedInteractable == interactable)
                ClearFocusedInteractable(interactable);

            RefreshInteractionCandidates();
        }

        public void RefreshInteractionCandidates()
        {
            if (_interactionSelector == null)
            {
                ClearFocusedInteractable(_focusedInteractable);
                return;
            }

            IInteractable selectedInteractable = _interactionSelector.Select(this, _interactionCandidates);

            if (selectedInteractable == _focusedInteractable) return;

            if (_focusedInteractable != null) ClearFocusedInteractable(_focusedInteractable);

            if (selectedInteractable != null) SetFocusedInteractable(selectedInteractable);
        }

        public void SetFocusedInteractable(IInteractable interactable)
        {
            if (interactable == null)
                return;

            if (_focusedInteractable == interactable) return;
            
            if (!interactable.CanFocus(this)) return;

            if (_focusedInteractable != null) ClearFocusedInteractable(_focusedInteractable);

            _focusedInteractable = interactable;

            _focusedInteractable.OnFocus(this);
            OnInteractableFocused(_focusedInteractable);
        }

        public void ClearFocusedInteractable(IInteractable interactable)
        {
            if (interactable == null)
                return;

            if (_focusedInteractable != interactable)
                return;

            _focusedInteractable.OnUnfocus(this);

            OnInteractableUnfocused(_focusedInteractable);

            _focusedInteractable = null;
        }

        public virtual void Interact()
        {
            if (_focusedInteractable == null)
                return;

            _focusedInteractable.Interact(this);
            ClearFocusedInteractable(_focusedInteractable);
            RefreshInteractionCandidates();
        }

        protected virtual void OnDisable()
        {
            ClearAllInteractionCandidates();
        }

        private void ClearAllInteractionCandidates()
        {
            if (_focusedInteractable != null) ClearFocusedInteractable(_focusedInteractable);

            _interactionCandidates.Clear();
        }

        protected virtual void OnInteractableFocused(IInteractable interactable)
        {
        }

        protected virtual void OnInteractableUnfocused(IInteractable interactable)
        {
        }
    }
}