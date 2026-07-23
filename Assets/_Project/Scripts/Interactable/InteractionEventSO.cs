using System;
using HelloDev.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KingdomLike.Interactables
{
    [CreateAssetMenu(
        fileName = "SO_Event_Interaction",
        menuName = "KingdomLike/Scriptable Objects/Events/Interaction Event")]
    public class InteractionEventSO : GameEvent_SO<InteractionPayload>
    {
#if UNITY_EDITOR && ODIN_INSPECTOR

        [Button]
        public void RaiseTest(InteractionPayload parameter)
        {
            Raise(parameter);
        }
#endif
    }
    [Serializable]
    public struct InteractionPayload
    {
        public IInteractable Interactable;
        public IInteractor Interactor;
    }
}