using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base class for objects that can be interacted with by IInteractors.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Filtering")]
        [SerializeField] private LayerMask _interactionLayer = ~0;

        [Header("Focus Conditions")]
        [SerializeField] private List<InteractionConditionSO> _focusConditions = new();

        [Header("Interaction Conditions")]
        [SerializeField] private List<InteractionConditionSO> _interactConditions = new();

        [Header("Behavior")]
        [SerializeField] private bool _autoInteract;
        [SerializeField] private bool _useOnce;

        [Header("Events")]
        [SerializeField] private UnityEvent<IInteractor> _onFocus = new();
        [SerializeField] private UnityEvent<IInteractor> _onUnfocus = new();
        [SerializeField] private UnityEvent<IInteractor> _onInteract = new();

        /// <summary>Raised right after <see cref="OnFocus"/> runs (only if overrides call base).</summary>
        public UnityEvent<IInteractor> OnFocusEvent => _onFocus;

        /// <summary>Raised right after <see cref="OnUnfocus"/> runs (only if overrides call base).</summary>
        public UnityEvent<IInteractor> OnUnfocusEvent => _onUnfocus;

        /// <summary>Raised right after a successful interaction, always fires regardless of subclass.</summary>
        public UnityEvent<IInteractor> OnInteractEvent => _onInteract;

        private readonly List<IInteractor> _interactorsInRange = new();
        private Collider _collider;
        private void Awake()
        {
            TryGetComponent(out _collider);
        }

        private bool _hasBeenUsed;

        /// <summary>
        /// Determines whether the interactor can focus this interactable.
        /// </summary>
        /// <param name="interactor">The interactor being evaluated.</param>
        /// <returns>True when the interactor can focus this interactable.</returns>
        public virtual bool CanFocus(IInteractor interactor)
        {
            if (_useOnce && _hasBeenUsed) return false;
            return EvaluateConditions(_focusConditions, interactor);
        }

        /// <summary>
        /// Determines whether the interactor can interact with this interactable.
        /// </summary>
        /// <param name="interactor">The interactor being evaluated.</param>
        /// <returns>True when the interactor can interact with this interactable.</returns>
        public virtual bool CanInteract(IInteractor interactor)
        {
            if (interactor == null)
                return false;

            if (_useOnce && _hasBeenUsed)
                return false;

            return EvaluateConditions(_interactConditions, interactor);
        }

        /// <summary>
        /// Attempts to interact with this interactable.
        /// </summary>
        /// <param name="interactor">The interactor attempting the interaction.</param>
        public void Interact(IInteractor interactor)
        {
            if (!CanInteract(interactor))
                return;

            OnInteract(interactor);
            _onInteract.Invoke(interactor);

            if (!_useOnce)
                return;

            _hasBeenUsed = true;
            
            RefreshInteractorCandidates();
            
            if (_collider) _collider.enabled = false;
        }

        /// <summary>
        /// Called when an interactor begins focusing this interactable.
        /// </summary>
        /// <param name="interactor">The interactor focusing this interactable.</param>
        public virtual void OnFocus(IInteractor interactor)
        {
            _onFocus.Invoke(interactor);
        }

        /// <summary>
        /// Called when an interactor stops focusing this interactable.
        /// </summary>
        /// <param name="interactor">The interactor no longer focusing this interactable.</param>
        public virtual void OnUnfocus(IInteractor interactor)
        {
            _onUnfocus.Invoke(interactor);
        }

        /// <summary>
        /// Re-evaluates whether all interactors currently in range
        /// can still consider this interactable as a candidate.
        /// </summary>
        public void RefreshInteractorCandidates()
        {
            for (int i = _interactorsInRange.Count - 1; i >= 0; i--)
            {
                IInteractor interactor = _interactorsInRange[i];

                if (interactor == null || interactor.InteractorObject == null)
                {
                    _interactorsInRange.RemoveAt(i);
                    continue;
                }

                if (interactor is IInteractionCandidate candidate)
                    candidate.RefreshInteractionCandidates();
            }
        }

        /// <summary>
        /// Performs the actual interaction behavior.
        /// </summary>
        /// <param name="interactor">The interactor performing the interaction.</param>
        protected abstract void OnInteract(IInteractor interactor);

        private bool EvaluateConditions(
            List<InteractionConditionSO> conditions,
            IInteractor interactor)
        {
            if (interactor == null)
                return false;

            foreach (InteractionConditionSO condition in conditions)
            {
                if (condition == null)
                    continue;

                if (!condition.Evaluate(interactor, this))
                    return false;
            }

            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _interactionLayer) == 0)
                return;
            
            IInteractor interactor = other.GetComponentInChildren<IInteractor>();

            if (interactor == null)
            {
                interactor = other.GetComponentInParent<IInteractor>();
            }

            if (interactor == null && other.transform.parent)
            {
               interactor = other.transform.parent.GetComponentInChildren<IInteractor>();
            }
            
            if (interactor == null) return;

            if (_interactorsInRange.Contains(interactor))
                return;

            _interactorsInRange.Add(interactor);

            if (interactor is IInteractionCandidate candidate)
                candidate.AddInteractionCandidate(this);

            if (_autoInteract)
                Interact(interactor);
        }

        private void OnTriggerExit(Collider other)
        {
            IInteractor interactor = other.GetComponentInChildren<IInteractor>();

            if (interactor == null)
            {
                interactor = other.GetComponentInParent<IInteractor>();
            }

            if (interactor == null && other.transform.parent)
            {
                interactor = other.transform.parent.GetComponentInChildren<IInteractor>();
            }
            
            if (interactor == null) return;

            if (!_interactorsInRange.Remove(interactor))
                return;

            if (interactor is IInteractionCandidate candidate)
                candidate.RemoveInteractionCandidate(this);
        }

        /// <summary>
        /// Clears all tracked interactors.
        /// </summary>
        protected virtual void OnDisable()
        {
            ClearInteractors();
        }

        private void ClearInteractors()
        {
            for (int i = _interactorsInRange.Count - 1; i >= 0; i--)
            {
                IInteractor interactor = _interactorsInRange[i];

                if (interactor is IInteractionCandidate candidate)
                    candidate.RemoveInteractionCandidate(this);
            }

            _interactorsInRange.Clear();
        }
    }
}