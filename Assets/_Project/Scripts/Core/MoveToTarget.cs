using UnityEngine;
using UnityEngine.Events;

namespace KingdomLike.Core
{
    /// <summary>
    /// Generic follow-to-target behaviour. Reusable for anything that needs
    /// to chase a Transform and report when it gets close enough.
    /// Example: currency.GetComponent<MoveToTarget>().SetTarget(player.transform);
    /// </summary>
    public class MoveToTarget : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _minDistance = 0.3f;
        [SerializeField] private Vector3 _targetOffset = Vector3.zero;

        [SerializeField] [Tooltip("Time to wait after SetTarget() before movement starts.")]
        private float _startDelay = 0f;

        [Header("Speed Curve (Juice)")]
        [Tooltip("Speed multiplier over time since launch. X = 0..1 (normalized time), Y = multiplier.")]
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);

        [Tooltip("Time it takes to reach the end of the speed curve. After this, speed stays at the curve's final value.")]
        [SerializeField] private float _speedCurveDuration = 1f;

        [Header("Scale Down (Juice)")]
        [Tooltip("Distance from target at which scale-down starts. No shrinking happens outside this radius.")]
        [SerializeField] private float _scaleActivationRadius = 0.5f;

        [Tooltip("Scale multiplier (relative to original scale) once it reaches the target. 0.3 = shrinks to 30%.")]
        [SerializeField] [Range(0f, 1f)] private float _endScaleRatio = 0.3f;

        [Header("Events")]
        [SerializeField] private UnityEvent _onTargetReached;
        public UnityEvent OnTargetReached => _onTargetReached;

        private Transform _target;
        private Vector3 _originalScale;
        private bool _isMoving;
        private bool _isDelaying;
        private float _delayTimer;
        private float _elapsedSinceLaunch;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void SetTarget(Transform target)
        {
            _target = target;

            if (_target == null)
            {
                _isMoving = false;
                _isDelaying = false;
                return;
            }

            _isDelaying = _startDelay > 0f;
            _delayTimer = _startDelay;
            _isMoving = !_isDelaying;
            _elapsedSinceLaunch = 0f;
            transform.localScale = _originalScale;
        }

        public void Stop()
        {
            _isMoving = false;
            _isDelaying = false;
            _target = null;
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
            float curveT = _speedCurveDuration > 0f
                ? Mathf.Clamp01(_elapsedSinceLaunch / _speedCurveDuration)
                : 1f;
            float currentSpeed = _speed * _speedCurve.Evaluate(curveT);

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                currentSpeed * Time.deltaTime);

            if (distanceToTarget <= _minDistance)
            {
                _isMoving = false;
                _onTargetReached?.Invoke();
            }
        }

        private void ApplyScaleDown(float distanceToTarget)
        {
            if (distanceToTarget > _scaleActivationRadius)
            {
                transform.localScale = _originalScale;
                return;
            }

            float t = 1f - Mathf.Clamp01(distanceToTarget / _scaleActivationRadius);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            float scaleRatio = Mathf.Lerp(1f, _endScaleRatio, easedT);

            transform.localScale = _originalScale * scaleRatio;
        }
    }
}