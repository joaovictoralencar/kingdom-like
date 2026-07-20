using System;
using System.Threading.Tasks;
using HelloDev.Saving;
using KingdomLike.Currency.Data;
using KingdomLike.Currency.Interfaces;
using KingdomLike.Events;
using UnityEngine;

namespace KingdomLike.Core.Components
{
    public class PlayerCurrencyComponent : BootstrappedSaveableSystem<CurrencySnapshot>
    {
        [Header("Data")]
        [SerializeField]
        private CurrencyDataSO _currencyData;

        [SerializeField]
        private CurrencyValueChangedEventSO _currencyValueChangedEvent;

        private ICurrency _currency;

        public CurrencyDataSO CurrencyData => _currencyData;

        public ICurrency Currency => _currency;

        public override string SystemKey => "Currency";

        private void Start()
        {
            //TEMP
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            RaiseValueChangedEvent(_currency.Min, _currency.Value);
        }

        public override Task InitializeAsync()
        {
            _currency = _currencyData.CreateCurrency();
            return base.InitializeAsync();
        }

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
                Debug.LogError(
                    $"Currency data is not assigned on {name}.",
                    this);

                return;
            }
            
            if (_currency == null)
            {
                Debug.LogError(
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

        protected override CurrencySnapshot Capture()
        {
            if (_currency == null)
            {
                Debug.LogError(
                    $"Cannot save currency because it has not been initialized.",
                    this);

                return new CurrencySnapshot();
            }

            HelloDev.Logging.Logger.LogVerbose(
                "Save",
                $"Saving currency snapshot {_currency.Value}",
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
                Debug.LogError(
                    $"Cannot restore currency because it has not been initialized.",
                    this);

                return false;
            }

            HelloDev.Logging.Logger.LogVerbose(
                "Save",
                $"Loading currency snapshot. {snapshot.Value}",
                this);

            _currency.Set(snapshot.Value);

            return true;
        }

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_currency != null)
            {
                _currency.OnValueChanged -= OnValueChanged;
            }
        }
    }

    [Serializable]
    public class CurrencySnapshot
    {
        public int Value;
    }
}