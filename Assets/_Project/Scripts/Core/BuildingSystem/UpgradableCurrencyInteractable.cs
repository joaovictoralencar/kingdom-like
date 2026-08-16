using HelloDev.Loader;
using HelloDev.Utils;
using KingdomLike.Core.Upgradable;
using KingdomLike.Interactables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KingdomLike.Core.Interactables

{
    [RequireComponent(typeof(CurrencyUpgradableBuilding))]
    public class UpgradableCurrencyInteractable : CurrencyInteractableUnlockable
    {
        [TabGroup("Unlockable")]
        [SerializeField] private Transform _lockedPrefabHolder;
        [TabGroup("Unlockable")]
        [SerializeField] private AssetReferenceGameObject _lockedPrefab;
        private CurrencyUpgradableBuilding _upgradableBuilding;

        private GameObject _lockedGameObject;

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

        protected override void OnLock()
        {
            base.OnLock();
            _lockedPrefabHolder.DestroyAllChildren();
            Loader.Instantiate(_lockedPrefab, _lockedPrefabHolder, onComplete: (obj) => _lockedGameObject = obj);
        }
        
        protected override void OnUnlock()
        {
            base.OnUnlock();
            Destroy(_lockedGameObject);
        }
    }
}