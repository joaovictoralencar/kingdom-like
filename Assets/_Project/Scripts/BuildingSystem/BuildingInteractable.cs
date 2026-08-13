using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Interactables
{
    /// <summary>
    /// Object that must first be unlocked and can then execute
    /// its actual gameplay interaction.
    ///
    /// The unlock and interaction responsibilities remain separate.
    /// </summary>
    [RequireComponent(typeof(CurrencyUnlockable))]
    public class BuildingInteractable : InteractableBase, IUnlockable
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
                _unlockable = GetComponent<CurrencyUnlockable>();

            if (_unlockable == null)
            {
                Debug.LogError($"{name} requires a CurrencyUnlockable component.", this);
            }
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

        protected override void OnExecuteInteraction(IInteractor interactor)
        {
            _onBuildingAction.Invoke();

            ExecuteBuildingAction(interactor);
        }

        protected virtual void ExecuteBuildingAction(IInteractor interactor)
        {
        }
    }
}