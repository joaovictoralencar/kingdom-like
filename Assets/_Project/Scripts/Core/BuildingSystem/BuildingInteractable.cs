using KingdomLike.Core;
using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    public abstract class BuildingInteractable : InteractableBase, IUnlockable
    {
        [Header("Unlock")]
        [SerializeField] private CurrencyUnlockable _unlockable;

        [Header("Building Action")]
        [SerializeField] private UnityEvent _onBuildingAction = new();

        public bool IsUnlocked => _unlockable != null && _unlockable.IsUnlocked;

        protected override void Awake()
        {
            base.Awake();

            if (_unlockable == null)
                TryGetComponent(out _unlockable);

            if (_unlockable == null)
            {
                Debug.LogError($"{name} requires {nameof(CurrencyUnlockable)}.", this);
            }
        }

        public override bool CanFocus(IInteractor interactor)
        {
            if (!base.CanFocus(interactor))
                return false;

            if (!IsUnlocked)
                return _unlockable != null && _unlockable.CanUnlock(interactor);

            return true;
        }

        public bool CanUnlock(IInteractor interactor)
        {
            return _unlockable != null && _unlockable.CanUnlock(interactor);
        }

        public bool Unlock(IInteractor interactor)
        {
            return _unlockable != null && _unlockable.Unlock(interactor);
        }

        public override bool CanInteract(IInteractor interactor)
        {
            if (!IsUnlocked)
                return false;

            return base.CanInteract(interactor);
        }

        public override bool TryGetInteractionCost(IInteractor interactor, out ICurrencyCost currencyCost)
        {
            if (!IsUnlocked)
            {
                if (_unlockable != null && _unlockable.TryGetInteractionCost(interactor, out currencyCost))
                {
                    return true;
                }

                currencyCost = null;
                return false;
            }
            return base.TryGetInteractionCost(interactor, out currencyCost);
        }

        protected sealed override void OnExecuteInteraction(IInteractor interactor)
        {
            ExecuteBuildingAction(interactor);
            _onBuildingAction.Invoke();
        }

        protected abstract void ExecuteBuildingAction(IInteractor interactor);
    }
}