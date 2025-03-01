using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace PantheonStorageAsBagMod.Components
{
    /// <summary>
    /// This allows us to change the images of buttons when hovered over with mouse
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class ButtonHoverSpriteSwap : MonoBehaviour
    {
        #region Private Fields

        private Image? _buttonImage;
        private bool _isHovered;
        private RectTransform? _rectTransform;
        private string _buttonType = null!;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Should be called immediately after adding this component to a GameObject
        /// This will set up the sprites to be used on hover/normal
        /// </summary>
        /// <param name="buttonType"></param>
        public void Initialize(string buttonType)
        {
            _buttonType = buttonType;
            SetButtonImage();
        }

        #endregion Public Methods

        #region Private Methods

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _buttonImage = GetComponent<Image>();
        }

        /// <summary>
        /// This swaps our button sprites around based on if the mouse is over the button or not
        /// </summary>
        private void SetButtonImage()
        {
            if (_buttonImage == null)
                return;

            switch (_buttonType)
            {
                case "Blue":
                    {
                        if (_isHovered)
                        {
                            var blueButtonHoveredTexture = Global.LoadImageToTexture2d("GenericButtonBlueHover.png");
                            _buttonImage.sprite = Sprite.Create(blueButtonHoveredTexture,
                                new Rect(0, 0, blueButtonHoveredTexture.width, blueButtonHoveredTexture.height),
                                new Vector2(0.5f, 0.5f),
                                100, 0, SpriteMeshType.Tight, new Vector4(7, 7, 7, 7));

                        }
                        else
                        {
                            var blueButtonNormalTexture = Global.LoadImageToTexture2d("GenericButtonBlueNormal.png");
                            _buttonImage.sprite = Sprite.Create(blueButtonNormalTexture,
                                new Rect(0, 0, blueButtonNormalTexture.width, blueButtonNormalTexture.height),
                                new Vector2(0.5f, 0.5f),
                                100, 0, SpriteMeshType.Tight, new Vector4(7, 7, 7, 7));
                        }

                        break;
                    }
                case "Close":
                    {
                        if (_isHovered)
                        {
                            var closeButtonHoveredTexture = Global.LoadImageToTexture2d("CodexCloseBtn_Hover.png");
                            _buttonImage.sprite = Sprite.Create(closeButtonHoveredTexture,
                                new Rect(0, 0, closeButtonHoveredTexture.width, closeButtonHoveredTexture.height),
                                new Vector2(0.5f, 0.5f));

                        }
                        else
                        {
                            var closeButtonNormalTexture = Global.LoadImageToTexture2d("CodexCloseBtn.png");
                            _buttonImage.sprite = Sprite.Create(closeButtonNormalTexture,
                                new Rect(0, 0, closeButtonNormalTexture.width, closeButtonNormalTexture.height),
                                new Vector2(0.5f, 0.5f));
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// We check to see if the mouse is within the bounds of the button and if it is we swap the image
        /// </summary>
        private void Update()
        {
            var mouseOverButton = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition);

            if (_isHovered && !mouseOverButton)
            {
                _isHovered = false;
                SetButtonImage();
            }

            if (!_isHovered && mouseOverButton)
            {
                _isHovered = true;
                SetButtonImage();
            }
        }

        #endregion Private Methods
    }
}