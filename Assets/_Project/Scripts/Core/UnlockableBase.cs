using System;
using Cysharp.Threading.Tasks;
using KingdomLike.Core;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    public abstract class UnlockableBase : InteractionTargetBase, IUnlockable, IInteractionCostDisplayer
    {
        [Header("Unlock")] [SerializeField] private bool _initiallyUnlocked;

        [SerializeField] private Transform _costDisplayTarget;

        [Header("Events")] [SerializeField] private UnityEvent _onUnlocked = new();

        public bool IsUnlocked { get; private set; }

        public event Action OnUnlocked;

        public Transform UICostDisplayTarget => _costDisplayTarget;


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
        
        protected override UniTask LoadState(InteractionTargetState state)
        {
            if (state is not UnlockableState unlockableState)
                return UniTask.CompletedTask;

            IsUnlocked = unlockableState.IsUnlocked;
            return UniTask.CompletedTask;
        }

        protected override InteractionTargetState SaveState()
        {
            return new UnlockableState(IsUnlocked);
        }

        protected override void OnAfterLoadState()
        {
            base.OnAfterLoadState();

            if (IsUnlocked || !_initiallyUnlocked) return;
            IsUnlocked = true;
        }
        
    }

    [Serializable]
    public class UnlockableState : InteractionTargetState
    {
        public bool IsUnlocked;

        public UnlockableState(bool isUnlocked) : base()
        {
            IsUnlocked = isUnlocked;
        }
    }
}