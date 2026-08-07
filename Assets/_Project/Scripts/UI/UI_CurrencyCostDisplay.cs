using System.Collections;
using KingdomLike.Interactables;
using PrimeTween;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomLike.UI
{
    public class UI_CurrencyCostDisplay : MonoBehaviour
    {
        [SerializeField] Image _currencyImage;
        [SerializeField] TextMeshProUGUI _costText;
        [SerializeField] Image fillImage;

        [Header("Visuals")] [SerializeField]
        private RectTransform _visualRoot; // child element used for shake/scale visuals

        private Transform _target;

        private Vector3 _visualOriginalScale = Vector3.one;
        private Vector2 _visualOriginalAnchoredPos = Vector2.zero;

        private Coroutine _holdCoroutine;
        private Coroutine _restoreCoroutine;

        private bool _holdingVisualsActive;
        private float _holdTargetScale = 1.15f;
        private float _holdShakeMagnitude = 4f;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                // try to resolve a RectTransform on a child or fallback to this object's RectTransform
                _visualRoot = GetComponent<RectTransform>();
            }

            if (_visualRoot == null)
                _visualRoot = transform as RectTransform;

            if (_visualRoot != null)
            {
                _visualOriginalScale = _visualRoot.localScale;
                _visualOriginalAnchoredPos = _visualRoot.anchoredPosition;
            }
        }

        private void OnEnable()
        {
            Tween.Scale(transform, Vector3.zero, Vector3.one, 0.2f, Ease.OutBack);
            CinemachineCore.CameraUpdatedEvent.AddListener(UpdatePosition);
        }

        private void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePosition);
        }

        public void SetCurrencyCost(ICurrencyCost currencyCost, Transform target)
        {
            _currencyImage.sprite = currencyCost.CurrencyType.Icon;
            if (currencyCost.CurrencyType.HasColor())
                _currencyImage.color = currencyCost.CurrencyType.Color;
            _costText.text = currencyCost.RequiredAmount.ToString();
            _target = target;
            fillImage.fillAmount = 0f;
        }

        public void SetProgress(float progress)
        {
            fillImage.fillAmount = Mathf.Clamp01(progress);

            if (_holdingVisualsActive && _visualRoot != null)
            {
                // scale based on progress between original and target
                float clamped = Mathf.Clamp01(progress);
                Vector3 desired = _visualOriginalScale * Mathf.Lerp(1f, _holdTargetScale, clamped);
                // smooth towards desired for nicer visuals
                _visualRoot.localScale = Vector3.Lerp(_visualRoot.localScale, desired, Time.deltaTime * 18f);
            }
        }

        public void StartHoldVisuals(float targetScale = 1.15f, float shakeMagnitude = 4f)
        {
            // stop any pending restore
            if (_restoreCoroutine != null)
            {
                StopCoroutine(_restoreCoroutine);
                _restoreCoroutine = null;
            }

            // ensure previous hold coroutine stopped
            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

            _holdTargetScale = targetScale;
            _holdShakeMagnitude = shakeMagnitude;
            _holdingVisualsActive = true;

            _holdCoroutine = StartCoroutine(ShakeCoroutine());
        }

        public void StopHoldVisuals(bool resetToOriginal = true)
        {
            _holdingVisualsActive = false;

            if (_holdCoroutine != null)
            {
                StopCoroutine(_holdCoroutine);
                _holdCoroutine = null;
            }

            // stop shake and restore scale if requested
            if (resetToOriginal && _visualRoot != null)
            {
                if (_restoreCoroutine != null)
                {
                    StopCoroutine(_restoreCoroutine);
                }

                _restoreCoroutine = StartCoroutine(RestoreVisualCoroutine(0.12f));
            }
        }

        private IEnumerator ShakeCoroutine()
        {
            if (_visualRoot == null)
                yield break;

            // Keep shaking until stopped. Position handled via anchoredPosition
            while (true)
            {
                Vector2 offset = Random.insideUnitCircle * (_holdShakeMagnitude * 0.5f);
                _visualRoot.anchoredPosition = _visualOriginalAnchoredPos + offset;
                yield return new WaitForSeconds(0.03f);
            }
        }

        private IEnumerator RestoreVisualCoroutine(float duration)
        {
            if (_visualRoot == null)
                yield break;

            float elapsed = 0f;
            Vector3 startScale = _visualRoot.localScale;
            Vector2 startPos = _visualRoot.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // ease in quad
                t = 1f - (1f - t) * (1f - t);

                _visualRoot.localScale = Vector3.Lerp(startScale, _visualOriginalScale, t);
                _visualRoot.anchoredPosition = Vector2.Lerp(startPos, _visualOriginalAnchoredPos, t);

                yield return null;
            }

            _visualRoot.localScale = _visualOriginalScale;
            _visualRoot.anchoredPosition = _visualOriginalAnchoredPos;
            _restoreCoroutine = null;
        }

        private void UpdatePosition(CinemachineBrain brain)
        {
            if (_target == null)
                return;

            Vector3 screenPos = brain.OutputCamera.WorldToScreenPoint(_target.position);

            bool isOffScreen =
                screenPos.z < 0f ||
                screenPos.x < 0f ||
                screenPos.x > Screen.width ||
                screenPos.y < 0f ||
                screenPos.y > Screen.height;

            if (!isOffScreen)
                transform.position = screenPos;
        }

        public void HideAndDestroy()
        {
            Tween.Scale(transform, Vector3.zero, 0.2f, Ease.InBack).OnComplete(() => { Destroy(gameObject); });
        }
    }
}