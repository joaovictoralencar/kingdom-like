using System;
using KingdomLike.Core;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    public abstract class UnlockableBase : InteractionTargetBase, IUnlockable, IInteractionCostDisplayer
    {
        [Header("Unlock")]
        [SerializeField] private bool _initiallyUnlocked;

        [SerializeField] private Transform _costDisplayTarget;

        [Header("Events")]
        [SerializeField] private UnityEvent _onUnlocked = new();

        public bool IsUnlocked { get; private set; }

        public event Action OnUnlocked;

        public Transform UICostDisplayTarget => _costDisplayTarget;

        protected override void Awake()
        {
            base.Awake();

            IsUnlocked = _initiallyUnlocked;

            if (IsUnlocked)
                OnRestoredAsUnlocked();
        }

        public virtual bool CanUnlock(IInteractor interactor)
        {
            if (interactor == null)
                return false;

            if (IsUnlocked)
                return false;

            return true;
        }

        public bool Unlock(IInteractor interactor)
        {
            if (!CanUnlock(interactor))
                return false;

            if (!TryUnlock(interactor))
                return false;

            SetUnlocked();

            return true;
        }

        public virtual bool TryGetInteractionCost(IInteractor interactor, out ICurrencyCost currencyCost)
        {
            currencyCost = null;

            if (IsUnlocked)
                return false;

            if (this is not ICurrencyCost cost)
                return false;

            currencyCost = cost;
            return true;
        }

        protected abstract bool TryUnlock(IInteractor interactor);

        protected void SetUnlocked()
        {
            if (IsUnlocked)
                return;

            IsUnlocked = true;

            OnUnlocked?.Invoke();
            _onUnlocked.Invoke();

            RefreshInteractorCandidates();
        }

        protected virtual void OnRestoredAsUnlocked()
        {
        }

        public UnlockableState CaptureUnlockState()
        {
            return new UnlockableState
            {
                IsUnlocked = IsUnlocked
            };
        }

        public void RestoreUnlockState(UnlockableState state)
        {
            bool wasUnlocked = IsUnlocked;

            IsUnlocked = state.IsUnlocked;

            if (!wasUnlocked && IsUnlocked)
            {
                OnRestoredAsUnlocked();
                RefreshInteractorCandidates();
            }
        }
    }

    [Serializable]
    public struct UnlockableState
    {
        public bool IsUnlocked;
    }
}