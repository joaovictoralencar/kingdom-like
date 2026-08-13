using System;
using KingdomLike.Building;
using UnityEngine;

namespace KingdomLike.Upgradable
{
    public class UpgradableBuilding : MonoBehaviour, IUpgradable
    {
        [field: SerializeField] public int CurrentLevel { get; set; }

        [field: SerializeField] public int MaxLevel { get; set; }

        public event Action<int> OnUpgrade;
        public event Action<int> OnDowngrade;
        public event Action OnReset;
        public event Action OnUpgradeMax;

        public void Upgrade()
        {
            if (!CanUpgrade())
                return;

            CurrentLevel++;

            OnUpgrade?.Invoke(CurrentLevel);
        }

        public void Downgrade()
        {
            if (!CanDowngrade())
                return;

            CurrentLevel--;

            OnDowngrade?.Invoke(CurrentLevel);
        }

        public bool CanDowngrade()
        {
            return CurrentLevel > 0;
        }

        public void Reset()
        {
            CurrentLevel = 0;

            OnReset?.Invoke();
        }

        public void UpgradeMax()
        {
            CurrentLevel = MaxLevel;

            OnUpgradeMax?.Invoke();
        }

        public bool CanUpgrade()
        {
            return CurrentLevel < MaxLevel;
        }
    }
}