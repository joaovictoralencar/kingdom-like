using System;
using KingdomLike.Currency.Interfaces;

namespace KingdomLike.Currency
{
    public class Currency : ICurrency
    {
        public int Value { get; private set; }

        public int Min { get; private set; }

        public int Max { get; private set; } = int.MaxValue;

        public bool IsEmpty => Value <= Min;

        public bool IsFull => Value >= Max;

        public event Action<int, int> OnValueChanged;

        public Currency(int min = 0, int max = int.MaxValue)
        {
            Min = min;
            Max = max;
        }

        public void Set(int value)
        {
            int clampedValue = Math.Clamp(value, Min, Max);

            if (Value == clampedValue)
                return;

            int previousValue = Value;

            Value = clampedValue;

            OnValueChanged?.Invoke(previousValue, Value);
        }

        public void Add(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Amount cannot be negative.");

            Set(Value + amount);
        }

        public void Remove(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Amount cannot be negative.");

            Set(Value - amount);
        }

        public bool TryAdd(int amount)
        {
            if (amount < 0)
                return false;

            if (Value + amount > Max)
                return false;

            Add(amount);
            return true;
        }

        public bool TryRemove(int amount)
        {
            if (amount < 0)
                return false;

            if (Value - amount < Min)
                return false;

            Remove(amount);
            return true;
        }

        public void SetMax(int max)
        {
            if (max < Min)
                throw new ArgumentOutOfRangeException(
                    nameof(max),
                    "Max cannot be lower than Min.");

            Max = max;

            Set(Value);
        }

        public void SetMin(int min)
        {
            if (min > Max)
                throw new ArgumentOutOfRangeException(
                    nameof(min),
                    "Min cannot be higher than Max.");

            Min = min;

            Set(Value);
        }
    }
}