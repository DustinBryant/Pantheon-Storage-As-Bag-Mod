using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace PantheonStorageAsBagMod.Components
{
    /// <summary>
    /// Component to show a specific denomination of currency
    /// </summary>
    [RegisterTypeInIl2Cpp]
    internal class CurrencyDisplay : MonoBehaviour
    {
        #region Private Fields

        private string _amount = "";
        private TextMeshProUGUI? _currencyTextGui;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Set the amount of currency to display.
        /// </summary>
        public string Amount
        {
            get => _amount;
            set
            {
                if (_amount == value)
                    return;

                _amount = value;

                if (_currencyTextGui == null)
                    return;

                _currencyTextGui.text = Amount;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Initialize(string iconImageFileName)
        {
            // Set up the currency display to have an icon and a text for value
            gameObject.AddComponent<RectTransform>();
            var currencyIcon = new GameObject("Icon");
            currencyIcon.transform.SetParent(gameObject.transform, false);
            var currencyIconRect = currencyIcon.AddComponent<RectTransform>();
            currencyIconRect.sizeDelta = new Vector2(10, 10);
            var currencyIconImage = currencyIcon.AddComponent<Image>();
            currencyIconImage.sprite = Sprite.Create(Global.LoadImageToTexture2d(iconImageFileName), new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
            var currencyText = new GameObject("AmountText");
            currencyText.transform.SetParent(gameObject.transform, false);
            var currencyTextRect = currencyText.AddComponent<RectTransform>();
            currencyTextRect.sizeDelta = new Vector2(20, 10);
            _currencyTextGui = currencyText.AddComponent<TextMeshProUGUI>();
            _currencyTextGui.fontSize = 10;
            _currencyTextGui.text = "0";
            _currencyTextGui.alignment = TextAlignmentOptions.Left;
            var currencyLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            currencyLayoutGroup.childControlHeight = false;
            currencyLayoutGroup.childControlWidth = false;
            currencyLayoutGroup.childForceExpandHeight = false;
            currencyLayoutGroup.childForceExpandWidth = false;
            currencyLayoutGroup.spacing = 3;
            currencyLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        }

        #endregion Public Methods
    }
}