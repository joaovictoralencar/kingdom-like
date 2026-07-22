using KingdomLike.Currency.Interfaces;
using KingdomLike.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;

namespace KingdomLike.Currency.Data
{
    [CreateAssetMenu(
        fileName = "SO_CurrencyData_",
        menuName = "KingdomLike/Scriptable Objects/CurrencyDataSO")]
    public class CurrencyDataSO : ScriptableObjectWithID
    {
        [SerializeField]
        [FoldoutGroup("Data")]
        [PreviewField(64, ObjectFieldAlignment.Left)]
        private Sprite _icon;

        [SerializeField]
        [FoldoutGroup("Data")]
        [MinValue(1)]
        private int _amount = 1;

        [SerializeField]
        [FoldoutGroup("Localization")]
        private LocalizedString _localizedName;

        [ShowIf("@_localizedName.IsEmpty")]
        [SerializeField]
        [FoldoutGroup("Localization")]
        private string _displayName;

        [SerializeReference]
        [FoldoutGroup("Factory Settings")]
        private CurrencyFactory _currencyFactory;

        public Sprite Icon => _icon;

        public LocalizedString LocalizedName => _localizedName;

        public string DisplayName =>
            _localizedName.IsEmpty
                ? _displayName
                : _localizedName.GetLocalizedString();

        public int Amount => _amount;

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