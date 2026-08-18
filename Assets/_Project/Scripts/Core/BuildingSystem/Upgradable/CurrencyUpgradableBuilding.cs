using System;
using Cysharp.Threading.Tasks;
using HelloDev.Utils;
using KingdomLike.Core.Interactables;
using KingdomLike.Currency.Data;
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
    }

    [Serializable]
    public struct UpgradeData
    {
        public GameObject Visual;
        public int Cost;
        public CurrencyDataSO Currency;
    }
}