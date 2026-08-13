using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Base class for objects that participate in the interaction system.
    ///
    /// Contains interaction plumbing only:
    /// range detection, candidate registration and focus handling.
    ///
    /// Contains no unlocking or gameplay action logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class InteractionTargetBase : MonoBehaviour, IInteractionTarget
    {
        [Header("Filtering")]
        [SerializeField] private LayerMask _interactionLayer = ~0;

        private readonly List<IInteractor> _interactorsInRange = new();

        private Collider _collider;

        public GameObject InteractorObject => gameObject;

        protected virtual void Awake()
        {
            TryGetComponent(out _collider);
        }

        public virtual bool CanFocus(IInteractor interactor)
        {
            return interactor != null;
        }

        public virtual void OnFocus(IInteractor interactor)
        {
        }

        public virtual void OnUnfocus(IInteractor interactor)
        {
        }

        protected void RegisterInteractor(IInteractor interactor)
        {
            if (interactor == null)
                return;

            if (_interactorsInRange.Contains(interactor))
                return;

            _interactorsInRange.Add(interactor);

            if (interactor is IInteractionCandidate candidate)
                candidate.AddInteractionTarget(this);
        }

        protected void UnregisterInteractor(IInteractor interactor)
        {
            if (interactor == null)
                return;

            if (!_interactorsInRange.Remove(interactor))
                return;

            if (interactor is IInteractionCandidate candidate)
                candidate.RemoveInteractionTarget(this);
        }

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

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _interactionLayer) == 0)
                return;

            IInteractor interactor = FindInteractor(other);

            if (interactor == null)
                return;

            RegisterInteractor(interactor);
        }

        private void OnTriggerExit(Collider other)
        {
            IInteractor interactor = FindInteractor(other);

            if (interactor == null)
                return;

            UnregisterInteractor(interactor);
        }

        private static IInteractor FindInteractor(Collider other)
        {
            IInteractor interactor = other.GetComponentInChildren<IInteractor>();

            if (interactor != null)
                return interactor;

            interactor = other.GetComponentInParent<IInteractor>();

            if (interactor != null)
                return interactor;

            if (other.transform.parent)
                interactor = other.transform.parent.GetComponentInChildren<IInteractor>();

            return interactor;
        }

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
                    candidate.RemoveInteractionTarget(this);
            }

            _interactorsInRange.Clear();
        }

        protected Collider InteractionCollider => _collider;
    }
}