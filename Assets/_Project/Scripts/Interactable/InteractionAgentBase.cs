using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base interaction agent.
    ///
    /// The agent owns the interaction state transition:
    ///
    /// Locked target:
    ///     IUnlockable.Unlock()
    ///
    /// Unlocked target:
    ///     IInteractable.ExecuteInteraction()
    /// </summary>
    public abstract class InteractionAgentBase : MonoBehaviour, IInteractionCandidate, IFocusableInteractor
    {
        [Header("Interaction")]
        [SerializeField] private InteractionSelectorSO _interactionSelector;

        private readonly List<IInteractionTarget> _interactionCandidates = new();

        private IInteractionTarget _focusedTarget;

        public GameObject InteractorObject => gameObject;

        public IInteractionTarget FocusedTarget => _focusedTarget;

        protected virtual void Awake()
        {
            if (_interactionSelector == null)
            {
                Debug.LogError($"{name} has no interaction selector assigned.", this);
            }
        }

        public void AddInteractionTarget(IInteractionTarget target)
        {
            if (target == null)
                return;

            if (_interactionCandidates.Contains(target))
                return;

            _interactionCandidates.Add(target);

            RefreshInteractionCandidates();
        }

        public void RemoveInteractionTarget(IInteractionTarget target)
        {
            if (target == null)
                return;

            if (!_interactionCandidates.Remove(target))
                return;

            if (_focusedTarget == target)
                ClearFocusedTarget(target);

            RefreshInteractionCandidates();
        }

        public void RefreshInteractionCandidates()
        {
            if (_interactionSelector == null)
            {
                ClearFocusedTarget(_focusedTarget);
                return;
            }

            IInteractionTarget selectedTarget = _interactionSelector.Select(this, _interactionCandidates);

            if (selectedTarget == _focusedTarget)
                return;

            if (_focusedTarget != null)
                ClearFocusedTarget(_focusedTarget);

            if (selectedTarget != null)
                SetFocusedTarget(selectedTarget);
        }

        public void SetFocusedTarget(IInteractionTarget target)
        {
            if (target == null)
                return;

            if (_focusedTarget == target)
                return;

            if (!target.CanFocus(this))
                return;

            if (_focusedTarget != null) ClearFocusedTarget(_focusedTarget);

            _focusedTarget = target;

            _focusedTarget.OnFocus(this);

            OnInteractionTargetFocused(_focusedTarget);
        }

        public void ClearFocusedTarget(IInteractionTarget target)
        {
            if (target == null)
                return;

            if (_focusedTarget != target)
                return;

            target.OnUnfocus(this);

            OnInteractionTargetUnfocused(target);

            _focusedTarget = null;
        }

        public virtual void Interact()
        {
            IInteractionTarget target = _focusedTarget;

            if (target == null)
                return;

            if (!TryExecuteTarget(target))
                return;

            ClearFocusedTarget(target);
            RefreshInteractionCandidates();
        }

        private bool TryExecuteTarget(IInteractionTarget target)
        {
            if (target == null)
                return false;

            IUnlockable unlockable = FindCapability<IUnlockable>(target);

            if (unlockable != null && !unlockable.IsUnlocked)
                return unlockable.Unlock(this);

            IInteractable interactable = FindCapability<IInteractable>(target);

            if (interactable == null)
                return false;

            if (!interactable.CanInteract(this))
                return false;

            interactable.ExecuteInteraction(this);

            return true;
        }

        private static T FindCapability<T>(IInteractionTarget target) where T : class
        {
            if (target is T capability)
                return capability;

            if (target.InteractorObject == null)
                return null;

            return target.InteractorObject.GetComponent<T>();
        }

        protected virtual void OnDisable()
        {
            if (_focusedTarget != null)
                ClearFocusedTarget(_focusedTarget);

            _interactionCandidates.Clear();
        }

        protected virtual void OnInteractionTargetFocused(IInteractionTarget target)
        {
        }

        protected virtual void OnInteractionTargetUnfocused(IInteractionTarget target)
        {
        }
    }
}