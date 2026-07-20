using KingdomLike.Currency.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;

namespace KingdomLike.Currency.Data
{
    [CreateAssetMenu(
        fileName = "SO_CurrencyData_",
        menuName = "KingdomLike/Scriptable Objects/CurrencyDataSO")]
    public class CurrencyDataSO : ScriptableObject
    {
        [PreviewField(64, ObjectFieldAlignment.Left)]
        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private LocalizedString _localizedName;

        [ShowIf("@_localizedName.IsEmpty")]
        [SerializeField]
        private string _displayName;

        [SerializeReference]
        private CurrencyFactory _currencyFactory;

        public Sprite Icon => _icon;

        public LocalizedString LocalizedName => _localizedName;

        public string DisplayName =>
            _localizedName.IsEmpty
                ? _displayName
                : _localizedName.GetLocalizedString();

        public ICurrency CreateCurrency()
        {
            if (_currencyFactory == null)
            {
                Debug.LogError(
                    $"Currency factory is not assigned on {name}.",
                    this);

                return null;
            }

            return _currencyFactory.Create();
        }
    }
}