using System;
using Cysharp.Threading.Tasks;
using HelloDev.Saving.Core;
using HelloDev.Saving.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Core.Upgradable
{
    public class UpgradableBuilding : SavableMonoBehaviour<UpgradableBuildingState>, IUpgradable
    {
        [FoldoutGroup("Upgradable")]
        [Header("Level")]
        [SerializeField, ReadOnly] private int currentLevel = 1;
        public int CurrentLevel { get => currentLevel; set => currentLevel = value; }

        [FoldoutGroup("Upgradable")]
        [SerializeField] private int maxLevel = 1;
        public int MaxLevel { get => maxLevel; set => maxLevel = value; }

        public event Action<int> OnUpgrade;
        public event Action<int> OnDowngrade;
        public event Action OnReset;
        public event Action OnUpgradeMax;

        [Button]
        public void Upgrade()
        {
            if (!CanUpgrade())
                return;

            CurrentLevel++;

            OnUpgrade?.Invoke(CurrentLevel);

            if (CurrentLevel == MaxLevel)
            {
                OnUpgradeMax?.Invoke();
            }
        }

        [Button]
        public void Downgrade()
        {
            if (!CanDowngrade())
                return;

            CurrentLevel--;

            OnDowngrade?.Invoke(CurrentLevel);
        }

        public bool CanDowngrade()
        {
            return CurrentLevel > 1;
        }

        [Button]
        public void Reset()
        {
            bool hasReset = false;
            while (CurrentLevel > 1)
            {
                Downgrade();
                hasReset = true;
            }
            if (hasReset) OnReset?.Invoke();
        }

        [Button]
        public void UpgradeMax()
        {
            while (CanUpgrade())
            {
                Upgrade();
            }
        }

        public virtual bool CanUpgrade()
        {
            return CurrentLevel < MaxLevel;
        }

        #region Save

        protected override UpgradableBuildingState SaveState()
        {
            return new UpgradableBuildingState()
            {
                CurrentLevel = CurrentLevel,
                MaxLevel = MaxLevel,
            };
        }

        protected override UniTask LoadState(UpgradableBuildingState state)
        {
            CurrentLevel = state.CurrentLevel;
            MaxLevel = state.MaxLevel;
            return UniTask.CompletedTask;
        }

        public override string ModuleId { get; protected set; } = "UpgradableBuilding";
        public override IUnifiedSaveManager SaveManager => UnifiedSaveManagerBehaviour.Instance.Manager;

        #endregion
    }

    public class UpgradableBuildingState
    {
        public int CurrentLevel;
        public int MaxLevel;
    }
}