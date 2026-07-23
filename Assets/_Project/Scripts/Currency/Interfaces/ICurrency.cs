using System;

namespace KingdomLike.Currency.Interfaces
{
    public interface ICurrency
    {
        int Value { get; }
        int Min { get; }
        int Max { get; }

        bool IsEmpty { get; }
        bool IsFull { get; }

        event Action<int, int> OnValueChanged;

        void Set(int value);

        void Add(int amount);
        void Remove(int amount);

        bool TryAdd(int amount);
        bool TryRemove(int amount);

        void SetMax(int max);
        void SetMin(int min);
        
        bool Has(int requiredAmount);
    }
}