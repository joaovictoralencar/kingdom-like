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

        private Transform _target;

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