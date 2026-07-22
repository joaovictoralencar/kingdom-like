using MalbersAnimations;
using UnityEngine;

namespace KingdomLike.Core
{
    public class InitialCameraZoom : MonoBehaviour
    {
        [SerializeField] private ThirdPersonFollowZoom _followZoom;
        [SerializeField] private ThirdPersonFollowTarget _followTarget;


        [SerializeField] private float _initialZoom = 12f;
        [SerializeField] private float _initialDistance = 12f;

        private void Start()
        {
            if (_followZoom) _followZoom.SetZoom(_initialZoom);
            if (_followTarget) _followTarget.SetCameraDistance(_initialDistance);
        }
    }
}