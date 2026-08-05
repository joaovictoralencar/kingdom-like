using System;
using Cysharp.Threading.Tasks;
using HelloDev.Saving.Core;
using HelloDev.Saving.Interfaces;
using KingdomLike.Currency;
using KingdomLike.Currency.Data;
using KingdomLike.Currency.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;

namespace KingdomLike.Core.Components
{
    public class EntityCurrencyComponent : SavableMonoBehaviour<CurrencySnapshot>
    {
        private const string LogId = "Player.Currency";

        #region Data

        [FoldoutGroup("Data"), Required, SerializeField]
        private CurrencyDataSO _currencyData;

        [FoldoutGroup("Data"), Required, SerializeField]
        private CurrencyValueChangedEventSO _currencyValueChangedEvent;

        #endregion

        #region Runtime

        private ICurrency _currency;

        #endregion

        #region Properties

        public override string ModuleId { get; protected set; } = "Currency";
        public override IUnifiedSaveManager SaveManager => UnifiedSaveManagerBehaviour.Instance.Manager;

        public CurrencyDataSO CurrencyData => _currencyData;
        public ICurrency Currency => _currency;

        #endregion

        #region Unity Lifecycle

        protected override void OnDestroy()
        {
            if (_currency != null)
                _currency.OnValueChanged -= OnCurrencyValueChanged;

            base.OnDestroy();
        }

        #endregion

        #region Save

        protected override CurrencySnapshot SaveState()
        {
            if (!HasCurrency())
                return new CurrencySnapshot();

            Logger.LogVerbose(LogId, $"Saving currency snapshot. Value: {_currency.Value}.", this);

            return new CurrencySnapshot
            {
                Value = _currency.Value
            };
        }

        protected override UniTask LoadState(CurrencySnapshot state)
        {
            if (!HasCurrency())
                return UniTask.CompletedTask;

            Logger.LogVerbose(LogId, $"Loading currency snapshot. Value: {state.Value}.", this);

            _currency.Set(state.Value);

            // Notify listeners immediately after the initial value is restored.
            PublishValueChanged(_currency.Value, _currency.Value);

            return UniTask.CompletedTask;
        }

        protected override UniTask OnBeforeRegisterAsync()
        {
            InitializeCurrency();
            return UniTask.CompletedTask;
        }

        #endregion

        #region Initialization

        private void InitializeCurrency()
        {
            if (_currency != null)
                return;

            if (_currencyData == null)
            {
                Logger.LogError(LogId, $"CurrencyData is not assigned on '{name}'.", this);
                return;
            }

            _currency = _currencyData.CreateCurrency();

            if (_currency == null)
            {
                Logger.LogError(LogId, "Failed to create currency instance.", this);
                return;
            }

            _currency.OnValueChanged += OnCurrencyValueChanged;
        }

        #endregion

        #region Currency Operations

        public bool CanReceive(CurrencyDataSO currencyData) => currencyData == _currencyData;

        public void Add(int amount) => _currency.Add(amount);

        public bool TryAdd(int amount) => _currency.TryAdd(amount);

        public void Remove(int amount) => _currency.Remove(amount);

        public bool TryRemove(int amount) => _currency.TryRemove(amount);

        #endregion

        #region Validation

        private bool HasCurrency()
        {
            if (_currency != null)
                return true;

            Logger.LogError(LogId, "Currency has not been initialized.", this);
            return false;
        }

        #endregion

        #region Events

        private void OnCurrencyValueChanged(int previousValue, int newValue)
        {
            PublishValueChanged(previousValue, newValue);
        }

        private void PublishValueChanged(int previousValue, int newValue)
        {
            if (_currencyValueChangedEvent == null)
                return;

            _currencyValueChangedEvent.Raise(new CurrencyValueChangedEvent
            {
                CurrencyData = _currencyData,
                PreviousValue = previousValue,
                Value = newValue
            });
        }

        #endregion
    }

    [Serializable]
    public class CurrencySnapshot
    {
        public int Value;
    }
}