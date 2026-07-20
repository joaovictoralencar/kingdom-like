using System;
using HelloDev.Events;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Events
{
    [CreateAssetMenu(
        fileName = "SO_CurrencyValueChangedEvent_",
        menuName = "KingdomLike/Scriptable Objects/Events/Currency Value Changed Event")]
    public class CurrencyValueChangedEventSO : GameEvent_SO<CurrencyValueChangedEvent>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Button]
        public void RaiseTest(CurrencyValueChangedEvent parameter)
        {
            Raise(parameter);
        }
#endif
    }

    [Serializable]
    public abstract class CurrencyEventUpdate : GameEvent_SO<CurrencyValueChangedEvent>
    {
    }

    [Serializable]
    public struct CurrencyValueChangedEvent
    {
        public CurrencyDataSO CurrencyData;
        public int PreviousValue;
        public int Value;
    }
}