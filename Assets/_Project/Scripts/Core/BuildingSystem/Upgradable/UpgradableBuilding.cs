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
        [FoldoutGroup("Upgradable")] [Header("Level")]
        [field: ShowInInspector, ReadOnly] public int CurrentLevel { get; set; } = 1;
        [FoldoutGroup("Upgradable")]
        [field: SerializeField] public int MaxLevel { get; set; } = 1;

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
            for (int i = 0; i < CurrentLevel - MaxLevel; i++)
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