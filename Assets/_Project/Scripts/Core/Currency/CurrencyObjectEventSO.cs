using HelloDev.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Currency
{
    [CreateAssetMenu(
        fileName = "SO_CurrencyObjectEvent_",
        menuName = "KingdomLike/Scriptable Objects/Events/Currency Object Event")]
    public class CurrencyObjectEventSO :
        GameEvent_SO<CurrencyComponent>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR

        [Button]
        public void RaiseTest(
            CurrencyComponent parameter)
        {
            Raise(parameter);
        }

#endif
    }
}