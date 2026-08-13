using KingdomLike.Interactables;
using UnityEngine;

namespace KingdomLike.Upgradable
{
    public class UpgradableBuildingInteractable : BuildingInteractable
    {
        private UpgradableBuilding _upgradableBuilding;

        protected override void Awake()
        {
            base.Awake();

            _upgradableBuilding = GetComponent<UpgradableBuilding>();

            if (_upgradableBuilding == null)
            {
                Debug.LogError($"{name} requires {nameof(UpgradableBuilding)}.", this);
            }
        }

        protected override void ExecuteBuildingAction(IInteractor interactor)
        {
            if (_upgradableBuilding == null)
                return;

            if (!_upgradableBuilding.CanUpgrade())
                return;

            _upgradableBuilding.Upgrade();
        }
    }
}