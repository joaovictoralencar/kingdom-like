using System;
using HelloDev.Events;
using KingdomLike.Currency.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Currency.Events
{
    [CreateAssetMenu(
        fileName = "SO_CurrencySpawnRequestEvent_",
        menuName = "KingdomLike/Scriptable Objects/Events/Currency Spawn Request Event")]
    public class CurrencySpawnRequestEventSO :
        GameEvent_SO<CurrencySpawnRequest>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR

        [Button]
        public void RaiseTest(
            CurrencySpawnRequest parameter)
        {
            Raise(parameter);
        }

#endif
    }

    [Serializable]
    public struct CurrencySpawnRequest
    {
        public CurrencyDataSO CurrencyData;

        public Vector3 Position;

        public Quaternion Rotation;

        public CurrencySpawnRequest(
            CurrencyDataSO currencyData,
            Vector3 position,
            Quaternion rotation)
        {
            CurrencyData = currencyData;
            Position = position;
            Rotation = rotation;
        }
    }
}