using KingdomLike.Core.Interactables;

namespace KingdomLike.Core.Upgradable
{
    public class CurrencyUpgradableBuilding : UpgradableBuilding
    {
        private CurrencyInteractable _currencyInteractable;

        protected override void Awake()
        {
            base.Awake();
            _currencyInteractable = GetComponent<CurrencyInteractable>();
        }

        private void OnEnable()
        {
            OnUpgrade += HandleUpgrade;
            UpdateCostByLevel();
        }

        private void OnDisable()
        {
            OnUpgrade -= HandleUpgrade;
            UpdateCostByLevel();
        }

        private void HandleUpgrade(int obj)
        {
        }

        void UpdateCostByLevel()
        {
            if (_currencyInteractable == null)
                return;

            _currencyInteractable.UpdateRequiredAmount((CurrentLevel + 1) * _currencyInteractable.RequiredAmount);
        }
    }
}