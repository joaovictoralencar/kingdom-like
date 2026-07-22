using KingdomLike.Currency.Data;
using KingdomLike.Currency;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace KingdomLike.UI
{
    public class UI_CurrencyCounter : MonoBehaviour
    {
        [FoldoutGroup("Data")]
        [SerializeField]
        private CurrencyValueChangedEventSO _currencyValueChangedEvent;

        [FoldoutGroup("Data")]
        [SerializeField]
        private CurrencyDataSO _currencyData;

        [FoldoutGroup("References")]
        [SerializeField]
        private Image _currencyImage;

        [FoldoutGroup("References")]
        [SerializeField]
        private LocalizeStringEvent _currencyNameLocalizeStringEvent;

        [FoldoutGroup("References")]
        [SerializeField]
        private TextMeshProUGUI _currencyText;

        private void OnEnable()
        {
            if (_currencyValueChangedEvent != null)
            {
                _currencyValueChangedEvent.AddListener(OnCurrencyValueChanged);
            }

            SetupUI();
        }

        private void OnDisable()
        {
            if (_currencyValueChangedEvent != null)
            {
                _currencyValueChangedEvent.RemoveListener(OnCurrencyValueChanged);
            }
        }

        private void SetupUI()
        {
            if (_currencyData == null)
                return;

            if (_currencyImage != null)
            {
                _currencyImage.sprite = _currencyData.Icon;
            }

            if (_currencyNameLocalizeStringEvent == null)
                return;

            if (_currencyData.LocalizedName.IsEmpty)
            {
                _currencyNameLocalizeStringEvent.GetComponent<TextMeshProUGUI>().text = _currencyData.DisplayName;
            }
            else
            {
                _currencyNameLocalizeStringEvent.StringReference = _currencyData.LocalizedName;
            }
        }

        private void OnCurrencyValueChanged(CurrencyValueChangedEvent currencyEvent)
        {
            if (currencyEvent.CurrencyData != _currencyData)
                return;

            if (_currencyText == null)
                return;

            _currencyText.text = currencyEvent.Value.ToString();
        }
    }
}