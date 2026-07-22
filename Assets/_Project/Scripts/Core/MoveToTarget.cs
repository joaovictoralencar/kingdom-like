using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Core
{
    /// <summary>
    /// Generic follow-to-target behaviour. Designed to be safely reused with pooled GameObjects.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class MoveToTarget : MonoBehaviour
    {
        #region Movement

        [Header("Movement")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _minDistance = 0.3f;
        [SerializeField] private Vector3 _targetOffset = Vector3.zero;

        [Tooltip("Time to wait after SetTarget() before movement starts.")]
        [SerializeField] private float _startDelay = 0f;

        #endregion

        #region Speed Curve (Juice)

        [Header("Speed Curve (Juice)")]
        [Tooltip("Speed multiplier over time since launch. X = 0..1 (normalized time), Y = multiplier.")]
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);

        [Tooltip("Time it takes to reach the end of the speed curve. After this, speed stays at the curve's final value.")]
        [SerializeField] private float _speedCurveDuration = 1f;

        #endregion

        #region Scale Down (Juice)

        [Header("Scale Down (Juice)")]
        [Tooltip("Distance from target at which scale-down starts.")]
        [SerializeField] private float _scaleActivationRadius = 0.5f;

        [Tooltip("Scale multiplier relative to the original scale.")]
        [SerializeField] [Range(0f, 1f)] private float _endScaleRatio = 0.3f;

        #endregion

        #region Events

        [Header("Events")]
        [SerializeField] private UnityEvent _onTargetReached;

        #endregion

        #region Runtime

        private Transform _target;
        private Vector3 _originalScale;
        private bool _isMoving;
        private bool _isDelaying;
        private float _delayTimer;
        private float _elapsedSinceLaunch;

        #endregion

        #region Properties

        public UnityEvent OnTargetReached => _onTargetReached;
        public bool IsMoving => _isMoving;
        public Transform Target => _target;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            _originalScale = transform.localScale;
        }

        private void Update()
        {
            if (_isDelaying)
            {
                _delayTimer -= Time.deltaTime;

                if (_delayTimer <= 0f)
                {
                    _isDelaying = false;
                    _isMoving = true;
                }

                return;
            }

            if (!_isMoving || _target == null)
                return;

            Vector3 targetPosition = _target.position + _targetOffset;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            ApplyScaleDown(distanceToTarget);

            _elapsedSinceLaunch += Time.deltaTime;
            float curveT = _speedCurveDuration > 0f ? Mathf.Clamp01(_elapsedSinceLaunch / _speedCurveDuration) : 1f;
            float currentSpeed = _speed * _speedCurve.Evaluate(curveT);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= _minDistance)
            {
                _isMoving = false;
                _onTargetReached?.Invoke();
            }
        }

        #endregion

        #region Target Control

        public void SetTarget(Transform target)
        {
            if (target == null)
            {
                Stop();
                return;
            }

            _target = target;
            _isDelaying = _startDelay > 0f;
            _delayTimer = _startDelay;
            _isMoving = !_isDelaying;
            _elapsedSinceLaunch = 0f;
            ResetScale();
        }

        public void Stop()
        {
            _isMoving = false;
            _isDelaying = false;
            _delayTimer = 0f;
            _elapsedSinceLaunch = 0f;
            _target = null;
            ResetScale();
        }

        #endregion

        #region Scale Juice

        private void ResetScale()
        {
            transform.localScale = _originalScale;
        }

        private void ApplyScaleDown(float distanceToTarget)
        {
            if (_scaleActivationRadius <= 0f)
            {
                transform.localScale = _originalScale * _endScaleRatio;
                return;
            }

            if (distanceToTarget > _scaleActivationRadius)
            {
                ResetScale();
                return;
            }

            float t = 1f - Mathf.Clamp01(distanceToTarget / _scaleActivationRadius);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            float scaleRatio = Mathf.Lerp(1f, _endScaleRatio, easedT);
            transform.localScale = _originalScale * scaleRatio;
        }

        #endregion
    }
}