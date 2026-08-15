using KingdomLike.Core.Upgradable;
using KingdomLike.Interactables;
using UnityEngine;

namespace KingdomLike.Core.Interactables

{
    [RequireComponent(typeof(CurrencyUpgradableBuilding))]
    public class UpgradableCurrencyInteractable : CurrencyInteractableUnlockable
    {
        private CurrencyUpgradableBuilding _upgradableBuilding;

        protected override void Awake()
        {
            base.Awake();

            _upgradableBuilding = GetComponent<CurrencyUpgradableBuilding>();

            if (_upgradableBuilding == null)
            {
                Debug.LogError($"{name} requires {nameof(CurrencyUpgradableBuilding)}.", this);
            }
        }

        public override bool CanFocus(IInteractor interactor)
        {
            if (_upgradableBuilding.CurrentLevel == _upgradableBuilding.MaxLevel)
                return false;
            return base.CanFocus(interactor);
        }

        protected override void OnInteract(IInteractor interactor)
        {
            if (_upgradableBuilding == null)
                return;

            if (_upgradableBuilding.CanUpgrade())
            {
                _upgradableBuilding.Upgrade();
            }
        }
    }
}