using System;

namespace KingdomLike.Building
{
    public interface IUpgradable
    {
        public void Upgrade();
        public void Downgrade();
        public void Reset();
        public void UpgradeMax();
        public bool CanUpgrade();
        public bool CanDowngrade();
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        
        public event Action<int> OnUpgrade;
        public event Action<int> OnDowngrade;
        public event Action OnReset;
        public event Action OnUpgradeMax;
    }
}