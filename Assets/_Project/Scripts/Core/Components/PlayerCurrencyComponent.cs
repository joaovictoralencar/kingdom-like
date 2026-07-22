using System;
using System.Threading.Tasks;
using HelloDev.Saving;
using KingdomLike.Currency.Data;
using KingdomLike.Currency.Interfaces;
using KingdomLike.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Core.Components
{
    public class PlayerCurrencyComponent : BootstrappedSaveableSystem<CurrencySnapshot>
    {
        private const string LogId = "Player.Currency";

        #region Data

        [FoldoutGroup("Data")]
        [Required]
        [SerializeField]
        private CurrencyDataSO _currencyData;

        [FoldoutGroup("Data")]
        [Required]
        [SerializeField]
        private CurrencyValueChangedEventSO _currencyValueChangedEvent;

        #endregion

        #region Runtime

        private ICurrency _currency;

        #endregion

        #region Properties

        public override string SystemKey => "Currency";

        public CurrencyDataSO CurrencyData => _currencyData;

        public ICurrency Currency => _currency;

        #endregion

        #region Initialization

        public override Task InitializeAsync()
        {
            if (_currencyData == null)
            {
                HelloDev.Logging.Logger.LogError(
                    LogId,
                    $"Currency data is not assigned on {name}.",
                    this);

                return base.InitializeAsync();
            }

            _currency = _currencyData.CreateCurrency();

            return base.InitializeAsync();
        }

        #endregion

        #region Save Lifecycle

        public override void OnBeforeSave()
        {
        }

        public override void OnAfterSave(bool success)
        {
        }

        public override void OnBeforeLoad()
        {
            if (_currencyData == null)
            {
                HelloDev.Logging.Logger.LogError(
                    LogId,
                    $"Currency data is not assigned on {name}.",
                    this);

                return;
            }

            if (_currency == null)
            {
                HelloDev.Logging.Logger.LogError(
                    LogId,
                    $"Failed to create currency on {name}.",
                    this);

                return;
            }

            _currency.OnValueChanged += OnValueChanged;
        }

        public override void OnAfterLoad(bool success)
        {
            if (!success || _currency == null)
                return;

            RaiseValueChangedEvent(
                _currency.Value,
                _currency.Value);
        }

        protected override CurrencySnapshot Capture()
        {
            if (_currency == null)
            {
                HelloDev.Logging.Logger.LogError(
                    LogId,
                    "Cannot save currency because it has not been initialized.",
                    this);

                return new CurrencySnapshot();
            }

            HelloDev.Logging.Logger.LogVerbose(
                LogId,
                $"Saving currency snapshot {_currency.Value}.",
                this);

            return new CurrencySnapshot
            {
                Value = _currency.Value
            };
        }

        protected override bool Restore(CurrencySnapshot snapshot)
        {
            if (_currency == null)
            {
                HelloDev.Logging.Logger.LogError(
                    LogId,
                    "Cannot restore currency because it has not been initialized.",
                    this);

                return false;
            }

            HelloDev.Logging.Logger.LogVerbose(
                LogId,
                $"Loading currency snapshot. Value: {snapshot.Value}.",
                this);

            _currency.Set(snapshot.Value);

            return true;
        }

        #endregion

        #region Currency Operations

        public bool CanReceive(CurrencyDataSO currencyData)
        {
            return currencyData == _currencyData;
        }

        public void Add(int amount)
        {
            _currency.Add(amount);
        }

        public bool TryAdd(int amount)
        {
            return _currency.TryAdd(amount);
        }

        public void Remove(int amount)
        {
            _currency.Remove(amount);
        }

        public bool TryRemove(int amount)
        {
            return _currency.TryRemove(amount);
        }

        #endregion

        #region Events

        private void OnValueChanged(
            int previousValue,
            int newValue)
        {
            RaiseValueChangedEvent(
                previousValue,
                newValue);
        }

        private void RaiseValueChangedEvent(
            int previousValue,
            int newValue)
        {
            if (_currencyValueChangedEvent == null)
                return;

            _currencyValueChangedEvent.Raise(
                new CurrencyValueChangedEvent
                {
                    CurrencyData = _currencyData,
                    PreviousValue = previousValue,
                    Value = newValue
                });
        }

        #endregion

        #region Cleanup

        protected override void OnDestroy()
        {
            if (_currency != null)
                _currency.OnValueChanged -= OnValueChanged;

            base.OnDestroy();
        }

        #endregion
    }

    [Serializable]
    public class CurrencySnapshot
    {
        public int Value;
    }
}