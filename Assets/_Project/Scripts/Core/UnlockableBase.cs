using System;
using Cysharp.Threading.Tasks;
using KingdomLike.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Logger = HelloDev.Logging.Logger;

namespace KingdomLike.Interactables
{
    public abstract class UnlockableBase : InteractionTargetBase, IUnlockable, IInteractionCostDisplayer
    {
        [FoldoutGroup("Unlockable")] [Header("Unlock")] [SerializeField]
        private bool _initiallyUnlocked;

        [FoldoutGroup("Unlockable")] [SerializeField]
        private Transform _costDisplayTarget;

        [FoldoutGroup("Unlockable")] [Header("Events")] [SerializeField]
        private UnityEvent _onUnlocked = new();

        [FoldoutGroup("Unlockable")] [SerializeField]
        private UnityEvent _onLocked = new();

        public bool IsUnlocked { get; private set; }

        public event Action<bool> OnUnlockedStateChanged;

        public Transform UICostDisplayTarget => _costDisplayTarget;

        protected virtual void Start()
        {
            if (loaded)
            {
                return;
            }

            SetUnlockedState(_initiallyUnlocked, true);
        }

        public virtual bool CanUnlock(IInteractor interactor)
        {
            if (interactor == null)
                return false;

            if (IsUnlocked)
                return false;

            return true;
        }

        public void Lock()
        {
            SetUnlockedState(false);
        }

        public bool Unlock(IInteractor interactor)
        {
            if (!CanUnlock(interactor))
                return false;

            if (!TryUnlock(interactor))
                return false;

            SetUnlockedState(true);

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

        protected void SetUnlockedState(bool unlocked)
        {
            SetUnlockedState(unlocked, false);
        }

        private void SetUnlockedState(bool unlocked, bool force)
        {
            if (IsUnlocked == unlocked && !force)
                return;

            IsUnlocked = unlocked;
            if (unlocked) _onUnlocked?.Invoke();
            else _onLocked?.Invoke();
            OnUnlockedStateChanged?.Invoke(unlocked);
            Logger.Log("Unlockable", $"Unlocked {unlocked} for [{gameObject.name}]");

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
            SetUnlockedState(true);
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