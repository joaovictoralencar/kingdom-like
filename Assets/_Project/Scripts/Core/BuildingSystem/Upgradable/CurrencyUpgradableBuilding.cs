using System;
using Cysharp.Threading.Tasks;
using HelloDev.Utils;
using KingdomLike.Core.Components;
using KingdomLike.Core.Interactables;
using KingdomLike.Currency.Data;
using KingdomLike.Interactables;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Core.Upgradable
{
    public class CurrencyUpgradableBuilding : UpgradableBuilding
    {
        private CurrencyInteractable _currencyInteractable;

        [FoldoutGroup("Upgradable")] 
        [SerializeField] private Transform VisualsHolder;
        [FoldoutGroup("Upgradable")]
        [Required(InfoMessageType.Error)]
        [SerializeField] private UpgradeData[] _upgrades;

        protected override void Awake()
        {
            base.Awake();
            _currencyInteractable = GetComponent<CurrencyInteractable>();
        }

        private void OnEnable()
        {
            OnUpgrade += HandleUpgrade;
        }

        private void OnDisable()
        {
            OnUpgrade -= HandleUpgrade;
        }

        private void HandleUpgrade(int upgradeLevel)
        {
            SetupUpgradable();
        }

        protected override void OnAfterLoadState()
        {
            base.OnAfterLoadState();
            SetupUpgradable();
        }

        public void SetupUpgradable()
        {
            if (_currencyInteractable == null)
                return;

            _currencyInteractable.UpdateRequiredAmount(_upgrades[CurrentLevel - 1].Cost);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            VisualsHolder.DestroyAllChildren();
            GameObject newVisual = Instantiate(_upgrades[CurrentLevel - 1].Visual, VisualsHolder);
            newVisual.transform.localPosition = Vector3.zero;
            Tween.PunchScale(newVisual.transform, Vector3.one * 0.2f, 0.5f, 2);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (_upgrades == null)
                _upgrades = new UpgradeData[MaxLevel];
            else if (_upgrades.Length != MaxLevel)
                Array.Resize(ref _upgrades, MaxLevel);
        }
#endif
        private static EntityCurrencyComponent FindCurrencyComponent(IInteractor interactor)
        {
            if (interactor == null)
                return null;

            GameObject interactorObject = interactor.InteractorObject;

            if (interactorObject == null)
                return null;

            EntityCurrencyComponent currency = interactorObject.GetComponentInParent<EntityCurrencyComponent>();

            if (currency != null)
                return currency;

            currency = interactorObject.GetComponentInChildren<EntityCurrencyComponent>();

            if (currency != null)
                return currency;

            if (interactorObject.transform.parent != null)
            {
                currency = interactorObject.transform.parent.GetComponentInChildren<EntityCurrencyComponent>();
            }

            return currency;
        }

        public bool CanUpgrade(IInteractor interactor)
        {
            EntityCurrencyComponent currency = FindCurrencyComponent(interactor);
            int requiredAmount = _upgrades[CurrentLevel - 1].Cost;
            CurrencyDataSO currencyType = _upgrades[CurrentLevel - 1].Currency;
            if (currency == null)
                return false;

            if (currencyType == null)
                return false;

            if (!currency.CanReceive(currencyType))
                return false;

            if (currency.Currency == null)
                return false;

            return base.CanUpgrade() && currency.Currency.Has(requiredAmount);
        }
    }

    [Serializable]
    public struct UpgradeData
    {
        public GameObject Visual;
        public int Cost;
        public CurrencyDataSO Currency;
    }
}