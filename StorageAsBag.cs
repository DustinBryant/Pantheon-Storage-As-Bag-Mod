using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using PantheonStorageAsBagMod.Components;
using UnityEngine;
using UnityEngine.UI;

namespace PantheonStorageAsBagMod
{
    [HarmonyPatch(typeof(UICharacterPanel), nameof(UICharacterPanel.Awake))]
    internal class StorageAsBag
    {
        #region Private Fields

        private static CanvasGroup? _characterPanelOpenAsBagOption;
        private static CanvasGroup? _characterPanelReturnStorage;
        private static CurrencyDisplay? _currencyCopper;
        private static CurrencyDisplay? _currencyGold;
        private static CurrencyDisplay? _currencyMithril;
        private static CurrencyDisplay? _currencyPlatinum;
        private static CurrencyDisplay? _currencySilver;
        private static Image? _positionLockImage;
        private static UIWindowPanel? _settingsUiWindowPanel;
        private static Transform? _storage;
        private static GameObject? _storageBag;
        private static UICharacterPanel? _uiCharacterPanel;
        private static UIDraggable? _uiDraggable;
        private static UIWindowPanel? _uiWindowPanel;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Closes the storage bag and returns the storage to the character panel
        /// </summary>
        public static void CloseBag()
        {
            Global.StorageAsBagCategory.SetAndSavePreference(Global.StorageAsBagCategoryBagEnabled, false);

            if (_uiWindowPanel != null && _storageBag != null)
            {
                // Save the anchoredPosition of the bag so that we can put it back where it was when reopened
                Global.StorageAsBagCategory.SetAndSavePreference(Global.StorageAsBagCategoryBagAnchoredPosition, _storageBag.GetComponent<RectTransform>().anchoredPosition);
                _uiWindowPanel.Hide();
            }

            if (_uiCharacterPanel == null || _storage == null)
                return;

            var overViewPanel = _uiCharacterPanel.transform.Find("LeftTabs").Find("Tabs Content").Find("Adventuring").Find("[Overview Panel]");

            // Move the games storage back home
            _storage.transform.SetParent(overViewPanel, false);
            _storage.GetComponent<RectTransform>().anchoredPosition = Global.StorageAsBagCategory.GetEntry<Vector2>(Global.StorageAsBagCategoryStorageAnchorPosition).Value;
            ShowHideOpenAsBagButton(true);
        }

        public static void CurrencyChanged(Currency currency)
        {
            // When currency has changed we update all our text values in the money panel on the bag
            if (_currencyMithril != null)
                _currencyMithril.Amount = currency.Mithril.ToString();

            if (_currencyPlatinum != null)
                _currencyPlatinum.Amount = currency.Platinum.ToString();

            if (_currencyGold != null)
                _currencyGold.Amount = currency.Gold.ToString();

            if (_currencySilver != null)
                _currencySilver.Amount = currency.Silver.ToString();

            if (_currencyCopper != null)
                _currencyCopper.Amount = currency.Copper.ToString();
        }

        /// <summary>
        /// Opens the storage bag and moves the storage to the bag
        /// </summary>
        public static void OpenBag()
        {
            Global.StorageAsBagCategory.SetAndSavePreference(Global.StorageAsBagCategoryBagEnabled, true);

            if (_uiCharacterPanel == null || _storageBag == null || _storage == null)
                return;

            _uiWindowPanel?.Show();

            var rectTransform = _storageBag.GetComponent<RectTransform>();
            // Set our anchored position to what is stored
            rectTransform.anchoredPosition = Global.StorageAsBagCategory.GetEntry<Vector2>(Global.StorageAsBagCategoryBagAnchoredPosition).Value;

            var storageRect = _storage.GetComponent<RectTransform>();

            // Move the current storage to the new bag
            _storage.transform.SetParent(_storageBag.transform, false);
            storageRect.anchoredPosition = new Vector2(1, -9);
            EnableUiDraggable(Global.StorageAsBagCategory.GetEntry<bool>(Global.StorageAsBagCategoryLockPosition).Value);

            ShowHideOpenAsBagButton(false);
        }

        /// <summary>
        /// This shows/hides the panel at the bottom that allows you to open the storage as a bag
        /// </summary>
        /// <param name="show"></param>
        public static void ShowHideOpenAsBagButton(bool show)
        {
            if (_characterPanelOpenAsBagOption == null)
                return;

            // Show/Hide the button to open the storage as a bag
            _characterPanelOpenAsBagOption.alpha = show ? 1 : 0;
            _characterPanelOpenAsBagOption.interactable = show;
            _characterPanelOpenAsBagOption.blocksRaycasts = show;

            if (_characterPanelReturnStorage == null)
                return;

            // Show/Hide the cover over the original storage
            _characterPanelReturnStorage.alpha = show ? 0 : 1;
            _characterPanelReturnStorage.interactable = !show;
            _characterPanelReturnStorage.blocksRaycasts = !show;
        }

        #endregion Public Methods

        #region Private Methods

        private static void CreateSettingsPanel(Transform midPanel)
        {
            if (_settingsUiWindowPanel != null)
                return;

            var fontResource = Resources
                .FindObjectsOfTypeAll<Font>()
                .FirstOrDefault(resourceFont => resourceFont.name.Equals("Roboto-Bold", StringComparison.OrdinalIgnoreCase));
            var robotoBold = TMP_FontAsset.CreateFontAsset(fontResource);

            var trackerSettings = new GameObject("Panel_StorageAsBagSettings");
            trackerSettings.transform.SetParent(midPanel);
            trackerSettings.layer = Layers.UI;
            var trackerSettingsRect = trackerSettings.AddComponent<RectTransform>();
            trackerSettingsRect.sizeDelta = new Vector2(240, 120);
            trackerSettingsRect.localScale = Vector3.one;
            trackerSettingsRect.anchorMin = new Vector2(0.5f, 0.5f);
            trackerSettingsRect.anchorMax = new Vector2(0.5f, 0.5f);
            trackerSettingsRect.anchoredPosition = Vector2.zero;
            var trackerSettingsPreventMouseImage = trackerSettings.AddComponent<Image>();
            trackerSettingsPreventMouseImage.color = new Color(0, 0, 0, 0);
            trackerSettingsPreventMouseImage.raycastTarget = true;
            trackerSettings.AddComponent<GraphicRaycaster>();
            trackerSettings.AddComponent<UIDraggable>();
            _settingsUiWindowPanel = trackerSettings.AddComponent<UIWindowPanel>();
            var canvasGroup = trackerSettings.AddComponent<CanvasGroup>();
            _settingsUiWindowPanel.CanvasGroup = canvasGroup;
            _settingsUiWindowPanel._displayName = "";

            // Panel background
            var trackerSettingsBackground = new GameObject("Background");
            trackerSettingsBackground.transform.SetParent(trackerSettings.transform);
            var trackerSettingsBackgroundRect = trackerSettingsBackground.AddComponent<RectTransform>();
            trackerSettingsBackgroundRect.anchoredPosition = new Vector2(1.5f, -1.5f);
            trackerSettingsBackgroundRect.anchorMax = new Vector2(1, 1);
            trackerSettingsBackgroundRect.anchorMin = new Vector2(0, 0);
            trackerSettingsBackgroundRect.sizeDelta = new Vector2(-1, -1);
            trackerSettingsBackgroundRect.localScale = Vector3.one;
            var trackerSettingsBackgroundCanvasRenderer = trackerSettingsBackground.AddComponent<CanvasRenderer>();
            trackerSettingsBackgroundCanvasRenderer.cullTransparentMesh = false;
            var trackerSettingsBackgroundImage = trackerSettingsBackground.AddComponent<Image>();
            var trackerSettingsBackgroundImageTexture = Global.LoadImageToTexture2d("InventoryBag_Bkg.png");
            trackerSettingsBackgroundImage.sprite = Sprite.Create(trackerSettingsBackgroundImageTexture,
                new Rect(0, 0, trackerSettingsBackgroundImageTexture.width, trackerSettingsBackgroundImageTexture.height),
                new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight,
                new Vector4(6, 7, 16, 14));
            trackerSettingsBackgroundImage.type = Image.Type.Sliced;
            var backgroundLayoutElement = trackerSettingsBackground.AddComponent<LayoutElement>();
            backgroundLayoutElement.ignoreLayout = true;

            // Close button of the background
            var backgroundCancelButton = new GameObject("CancelButton");
            backgroundCancelButton.transform.SetParent(trackerSettingsBackground.transform, false);
            var backgroundCancelRect = backgroundCancelButton.AddComponent<RectTransform>();
            backgroundCancelRect.anchoredPosition = new Vector2(-14f, -13f);
            backgroundCancelRect.anchorMax = new Vector2(1f, 1f);
            backgroundCancelRect.anchorMin = new Vector2(1f, 1f);
            backgroundCancelRect.sizeDelta = new Vector2(30f, 30f);
            backgroundCancelRect.localScale = Vector3.one;
            var backgroundCancelRenderer = backgroundCancelButton.AddComponent<CanvasRenderer>();
            backgroundCancelRenderer.cullTransparentMesh = false;
            var backgroundCancelButtonImage = backgroundCancelButton.AddComponent<Image>();
            var backgroundCancelButtonTexture = Global.LoadImageToTexture2d("CodexCloseBtn.png");
            backgroundCancelButtonImage.sprite = Sprite.Create(backgroundCancelButtonTexture,
                new Rect(0, 0, backgroundCancelButtonTexture.width, backgroundCancelButtonTexture.height),
                new Vector2(0.5f, 0.5f));
            var cancelButton = backgroundCancelButton.AddComponent<Button>();
            cancelButton.onClick.AddListener(new Action(() =>
            {
                Global.StorageAsBagCategory.SaveToFile();
                _settingsUiWindowPanel.Hide();
            }));
            var backgroundCancelLayoutElement = backgroundCancelButton.AddComponent<LayoutElement>();
            backgroundCancelLayoutElement.ignoreLayout = true;

            // Panel title
            var settingsTitle = new GameObject("Title");
            settingsTitle.transform.SetParent(trackerSettingsBackground.transform);
            var settingsTitleRect = settingsTitle.AddComponent<RectTransform>();
            settingsTitleRect.anchoredPosition = new Vector2(0, -12);
            settingsTitleRect.anchorMax = new Vector2(0.5f, 1);
            settingsTitleRect.anchorMin = new Vector2(0.5f, 1);
            settingsTitleRect.pivot = new Vector2(0.5f, 1);
            settingsTitleRect.sizeDelta = Vector2.zero;
            settingsTitleRect.localScale = Vector3.one;
            var settingsTitleFirstRow = new GameObject("First");
            settingsTitleFirstRow.transform.SetParent(settingsTitle.transform);
            var settingsTitleFirstRowRect = settingsTitleFirstRow.AddComponent<RectTransform>();
            settingsTitleFirstRowRect.anchoredPosition = Vector2.zero;
            settingsTitleFirstRowRect.anchorMax = Vector2.one;
            settingsTitleFirstRowRect.anchorMin = Vector2.zero;
            settingsTitleFirstRowRect.pivot = new Vector2(0.5f, 1);
            settingsTitleFirstRowRect.localScale = Vector3.one;
            var storageAsBagText = settingsTitleFirstRow.AddComponent<TextMeshProUGUI>();
            storageAsBagText.text = "Storage As Bag";
            storageAsBagText.fontSize = 14;
            storageAsBagText.fontStyle = FontStyles.Bold;
            storageAsBagText.color = new Color(250f / 255, 212f / 255, 13f / 255, 1);
            storageAsBagText.font = robotoBold!;
            settingsTitleFirstRowRect.sizeDelta = new Vector2(storageAsBagText.preferredWidth, 0);
            var settingsTitleSecondRow = new GameObject("Second");
            settingsTitleSecondRow.transform.SetParent(settingsTitle.transform);
            var settingsTitleSecondRowRect = settingsTitleSecondRow.AddComponent<RectTransform>();
            settingsTitleSecondRowRect.anchoredPosition = new Vector2(0, -20);
            settingsTitleSecondRowRect.anchorMax = Vector2.one;
            settingsTitleSecondRowRect.anchorMin = Vector2.zero;
            settingsTitleSecondRowRect.pivot = new Vector2(0.5f, 1);
            settingsTitleSecondRowRect.localScale = Vector3.one;
            var settingsTitleText = settingsTitleSecondRow.AddComponent<TextMeshProUGUI>();
            settingsTitleText.text = "Settings";
            settingsTitleText.fontSize = 14;
            settingsTitleText.fontStyle = FontStyles.Bold;
            settingsTitleText.color = new Color(250f / 255, 212f / 255, 13f / 255, 1);
            settingsTitleText.font = robotoBold!;
            settingsTitleSecondRowRect.sizeDelta = new Vector2(settingsTitleText.preferredWidth, 0);

            var settingsContainer = new GameObject("Settings Container");
            settingsContainer.transform.SetParent(trackerSettings.transform);
            var settingsContainerRect = settingsContainer.AddComponent<RectTransform>();
            settingsContainerRect.anchoredPosition = new Vector2(0, -65);
            settingsContainerRect.anchorMax = Vector2.one;
            settingsContainerRect.anchorMin = new Vector2(0, 0.5f);
            settingsContainerRect.pivot = new Vector2(0.5f, 0.5f);
            settingsContainerRect.sizeDelta = Vector2.zero;
            settingsContainerRect.localScale = Vector3.one;

            var scale = Global.StorageAsBagCategory.GetEntry<float>(Global.StorageAsBagCategoryScale).Value;

            var scaleSettingGroup = new GameObject("Scale Setting");
            scaleSettingGroup.transform.SetParent(settingsContainer.transform);
            var scaleSettingGroupRect = scaleSettingGroup.AddComponent<RectTransform>();
            scaleSettingGroupRect.anchoredPosition = new Vector2(0, -20);
            scaleSettingGroupRect.anchorMax = Vector2.up;
            scaleSettingGroupRect.anchorMin = Vector2.up;
            scaleSettingGroupRect.pivot = Vector2.up;
            scaleSettingGroupRect.localScale = Vector3.one;

            var scaleSettingTitle = new GameObject("Title");
            scaleSettingTitle.transform.SetParent(scaleSettingGroup.transform);
            var scaleSettingTitleRect = scaleSettingTitle.AddComponent<RectTransform>();
            scaleSettingTitleRect.anchoredPosition = new Vector2(70, 45);
            scaleSettingTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
            scaleSettingTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
            scaleSettingTitleRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSettingTitleRect.localScale = Vector3.one;
            var scaleSettingTitleText = scaleSettingTitle.AddComponent<TextMeshProUGUI>();
            scaleSettingTitleText.text = $"Scale: {scale}";
            scaleSettingTitleText.fontSize = 12;
            scaleSettingTitleText.fontStyle = FontStyles.Bold;
            scaleSettingTitleText.color = new Color(1, 1, 1, 1);
            scaleSettingTitleText.font = robotoBold!;

            var scaleSetting = new GameObject("Scale Slider");
            scaleSetting.transform.SetParent(scaleSettingGroup.transform);
            var scaleSettingSliderRect = scaleSetting.AddComponent<RectTransform>();
            scaleSettingSliderRect.anchoredPosition = new Vector2(70, 45);
            scaleSettingSliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            scaleSettingSliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            scaleSettingSliderRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSettingSliderRect.localScale = Vector3.one;
            scaleSettingSliderRect.sizeDelta = new Vector2(200, 20);
            var scaleSlider = scaleSetting.AddComponent<Slider>();
            scaleSlider.maxValue = 150;
            scaleSlider.minValue = 50;
            scaleSlider.normalizedValue = 0.7f;
            scaleSlider.value = scale;
            scaleSlider.wholeNumbers = true;
            scaleSlider.onValueChanged.AddListener(new Action<float>(_ => ScaleSettingChanged(scaleSlider, scaleSettingTitleText)));

            var scaleSliderBackground = new GameObject("Background");
            scaleSliderBackground.transform.SetParent(scaleSetting.transform);
            var scaleSliderBackgroundRect = scaleSliderBackground.AddComponent<RectTransform>();
            scaleSliderBackgroundRect.anchoredPosition = Vector2.zero;
            scaleSliderBackgroundRect.anchorMax = new Vector2(1, 0.75f);
            scaleSliderBackgroundRect.anchorMin = new Vector2(0, 0.25f);
            scaleSliderBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSliderBackgroundRect.localScale = Vector3.one;
            scaleSliderBackgroundRect.sizeDelta = Vector2.zero;
            var scaleSliderBackgroundImage = scaleSliderBackground.AddComponent<Image>();
            scaleSliderBackgroundImage.color = new Color(1, 1, 1, 1);

            var scaleSliderFillArea = new GameObject("Fill Area");
            scaleSliderFillArea.transform.SetParent(scaleSetting.transform);
            var scaleSliderFillAreaRect = scaleSliderFillArea.AddComponent<RectTransform>();
            scaleSliderFillAreaRect.anchoredPosition = new Vector2(-5, 0);
            scaleSliderFillAreaRect.anchorMax = new Vector2(1, 0.75f);
            scaleSliderFillAreaRect.anchorMin = new Vector2(0, 0.25f);
            scaleSliderFillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSliderFillAreaRect.localScale = Vector3.one;
            scaleSliderFillAreaRect.sizeDelta = new Vector2(-20, 0);
            var scaleSliderFillAreaFill = new GameObject("Fill");
            scaleSliderFillAreaFill.transform.SetParent(scaleSliderFillArea.transform);
            var scaleSliderFillAreaFillRect = scaleSliderFillAreaFill.AddComponent<RectTransform>();
            scaleSliderFillAreaFillRect.anchoredPosition = Vector2.zero;
            scaleSliderFillAreaFillRect.anchorMax = new Vector2(0.51f, 1);
            scaleSliderFillAreaFillRect.anchorMin = Vector2.zero;
            scaleSliderFillAreaFillRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSliderFillAreaFillRect.localScale = Vector3.one;
            scaleSliderFillAreaFillRect.sizeDelta = new Vector2(10, 0);
            var scaleSliderFillAreaFillImage = scaleSliderFillAreaFill.AddComponent<Image>();
            scaleSliderFillAreaFillImage.color = new Color(1, 1, 1, 1);
            scaleSlider.fillRect = scaleSliderFillAreaFillRect;

            var scaleSliderHandle = new GameObject("Handle");
            scaleSliderHandle.transform.SetParent(scaleSetting.transform);
            var scaleSliderHandleRect = scaleSliderHandle.AddComponent<RectTransform>();
            scaleSliderHandleRect.anchoredPosition = Vector2.zero;
            scaleSliderHandleRect.anchorMax = Vector2.one;
            scaleSliderHandleRect.anchorMin = Vector2.zero;
            scaleSliderHandleRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSliderHandleRect.localScale = Vector3.one;
            scaleSliderHandleRect.sizeDelta = new Vector2(-20, 0);
            var scaleSliderHandleHandle = new GameObject("Handle");
            scaleSliderHandleHandle.transform.SetParent(scaleSliderHandle.transform);
            var scaleSliderHandleHandleRect = scaleSliderHandleHandle.AddComponent<RectTransform>();
            scaleSliderHandleHandleRect.anchoredPosition = Vector2.zero;
            scaleSliderHandleHandleRect.anchorMax = new Vector2(0.51f, 1);
            scaleSliderHandleHandleRect.anchorMin = new Vector2(0.51f, 0);
            scaleSliderHandleHandleRect.pivot = new Vector2(0.5f, 0.5f);
            scaleSliderHandleHandleRect.sizeDelta = new Vector2(10, 0);
            scaleSliderHandleHandleRect.localScale = Vector3.one;
            var scaleSliderHandleHandleImage = scaleSliderHandleHandle.AddComponent<Image>();
            var scaleSliderHandleHandleTexture = Global.LoadImageToTexture2d("ScrollBarVertical.png");
            scaleSliderHandleHandleImage.sprite = Sprite.Create(scaleSliderHandleHandleTexture,
                new Rect(0, 0, scaleSliderHandleHandleTexture.width, scaleSliderHandleHandleTexture.height),
                new Vector2(0.5f, 0.5f));
            scaleSlider.handleRect = scaleSliderHandleHandleRect;
            scaleSlider.image = scaleSliderHandleHandleImage;
            scaleSlider.targetGraphic = scaleSliderHandleHandleImage;
        }

        /// <summary>
        /// Creates the main panel for the storage bag
        /// </summary>
        private static void CreateStorageBag()
        {
            if (_uiCharacterPanel == null)
                return;

            // Store anchorPosition of the current storage
            Global.StorageAsBagCategory.SetAndSavePreference(Global.StorageAsBagCategoryStorageAnchorPosition, _storage.GetComponent<RectTransform>().anchoredPosition);

            var midPanel = _uiCharacterPanel.transform.parent;

            // Main Storage Bag panel
            _storageBag = new GameObject("Panel_StorageBag");
            _storageBag.transform.SetParent(midPanel, false);
            _storageBag.layer = Layers.UI;
            var storageBagRect = _storageBag.AddComponent<RectTransform>();
            storageBagRect.anchoredPosition = Global.StorageAsBagCategory.GetEntry<Vector2>(Global.StorageAsBagCategoryBagAnchoredPosition).Value;
            storageBagRect.sizeDelta = new Vector2(212, 147.2f);
            var scale = Global.StorageAsBagCategory.GetEntry<float>(Global.StorageAsBagCategoryScale).Value;
            storageBagRect.localScale = new Vector3(scale / 100, scale / 100, scale / 100);

            // prevent mouse from clicking through the bag
            var storagePreventMouseImage = _storageBag.AddComponent<Image>();
            storagePreventMouseImage.color = new Color(0, 0, 0, 0);
            storagePreventMouseImage.raycastTarget = true;
            _storageBag.AddComponent<GraphicRaycaster>();

            // Add window panel components
            _uiDraggable = _storageBag.AddComponent<UIDraggable>();
            _uiWindowPanel = _storageBag.AddComponent<UIWindowPanel>();
            _uiDraggable._windowPanel = _uiWindowPanel;
            
            var canvasGroup = _storageBag.AddComponent<CanvasGroup>();
            _storageBag.AddComponent<BagPositionSaver>();
            _uiWindowPanel.CanvasGroup = canvasGroup;
            _uiWindowPanel._displayName = "";

            // Background object of the storage bag
            var storageBagBackground = new GameObject("Background");
            storageBagBackground.transform.SetParent(_storageBag.transform, false);
            var backgroundRect = storageBagBackground.AddComponent<RectTransform>();
            backgroundRect.anchoredPosition = new Vector2(1.5f, -1.5f);
            backgroundRect.anchorMax = new Vector2(1f, 1f);
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.sizeDelta = new Vector2(-1f, -1f);
            var backgroundCanvasRenderer = storageBagBackground.AddComponent<CanvasRenderer>();
            backgroundCanvasRenderer.cullTransparentMesh = false;
            var backgroundImage = storageBagBackground.AddComponent<Image>();
            var backgroundTexture = Global.LoadImageToTexture2d("InventoryBag_Bkg.png");
            backgroundImage.sprite = Sprite.Create(backgroundTexture, new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, new Vector4(6, 7, 16, 14));
            backgroundImage.type = Image.Type.Sliced;
            var backgroundLayoutElement = storageBagBackground.AddComponent<LayoutElement>();
            backgroundLayoutElement.ignoreLayout = true;

            // Lock toggle object of the background
            var toggleObject = new GameObject("LockToggle");
            toggleObject.transform.SetParent(storageBagBackground.transform, false);
            var toggleRect = toggleObject.AddComponent<RectTransform>();
            toggleRect.anchoredPosition = new Vector2(-198, -14f);
            toggleRect.anchorMax = new Vector2(1f, 1f);
            toggleRect.anchorMin = new Vector2(1f, 1f);
            toggleRect.sizeDelta = new Vector2(12, 10);
            var backgroundToggleRenderer = toggleObject.AddComponent<CanvasRenderer>();
            backgroundToggleRenderer.cullTransparentMesh = false;
            _positionLockImage = toggleObject.AddComponent<Image>();
            _positionLockImage.preserveAspect = true;
            var toggleButton = toggleObject.AddComponent<Button>();
            toggleButton.onClick.AddListener(new Action(LockTogglePosition));

            // Settings button
            var settingsButton = new GameObject("Settings Button");
            settingsButton.transform.SetParent(storageBagBackground.transform);
            var settingsButtonRect = settingsButton.AddComponent<RectTransform>();
            settingsButtonRect.anchoredPosition = new Vector2(-35, -13.5f);
            settingsButtonRect.anchorMax = new Vector2(1, 1);
            settingsButtonRect.anchorMin = new Vector2(1, 1);
            settingsButtonRect.sizeDelta = new Vector2(13, 13);
            settingsButtonRect.localScale = new Vector3(1, 1, 1);
            var settingsButtonRenderer = settingsButton.AddComponent<CanvasRenderer>();
            settingsButtonRenderer.cullTransparentMesh = false;
            var settingsButtonImage = settingsButton.AddComponent<Image>();
            var settingsButtonTexture = Global.LoadImageToTexture2d("settings.png");
            settingsButtonTexture.filterMode = FilterMode.Point;
            settingsButtonImage.sprite = Sprite.Create(settingsButtonTexture, new Rect(0, 0, settingsButtonTexture.width, settingsButtonTexture.height), new Vector2(0.5f, 0.5f));
            var settingsButtonButton = settingsButton.AddComponent<Button>();
            settingsButtonButton.onClick.AddListener(new Action(OpenSettingsPanelToggle));
            var settingsButtonLayoutElement = settingsButton.AddComponent<LayoutElement>();
            settingsButtonLayoutElement.ignoreLayout = true;

            // Close button object of the background
            var backgroundCancelButton = new GameObject("CancelButton");
            backgroundCancelButton.transform.SetParent(storageBagBackground.transform, false);
            var backgroundCancelRect = backgroundCancelButton.AddComponent<RectTransform>();
            backgroundCancelRect.anchoredPosition = new Vector2(-14f, -13f);
            backgroundCancelRect.anchorMax = new Vector2(1f, 1f);
            backgroundCancelRect.anchorMin = new Vector2(1f, 1f);
            backgroundCancelRect.sizeDelta = new Vector2(30f, 30f);
            var backgroundCancelRenderer = backgroundCancelButton.AddComponent<CanvasRenderer>();
            backgroundCancelRenderer.cullTransparentMesh = false;
            backgroundCancelButton.AddComponent<Image>();
            var cancelButton = backgroundCancelButton.AddComponent<Button>();
            cancelButton.onClick.AddListener(new Action(CloseBag));
            var cancelButtonSwap = backgroundCancelButton.AddComponent<ButtonHoverSpriteSwap>();
            cancelButtonSwap.Initialize("Close");
            var backgroundCancelLayoutElement = backgroundCancelButton.AddComponent<LayoutElement>();
            backgroundCancelLayoutElement.ignoreLayout = true;

            // Money panel. Show money in a panel that sits on the bottom of our new bag
            var moneyPanel = new GameObject("MoneyPanel");
            moneyPanel.transform.SetParent(_storageBag.transform, false);
            moneyPanel.transform.SetSiblingIndex(0);
            var moneyPanelRect = moneyPanel.AddComponent<RectTransform>();
            moneyPanelRect.anchoredPosition = new Vector2(1, -82);
            moneyPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            moneyPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            moneyPanelRect.sizeDelta = new Vector2(207.5f, 26);

            // Money panel background
            var moneyPanelBackground = new GameObject("Background");
            moneyPanelBackground.transform.SetParent(moneyPanel.transform, false);
            var moneyBackgroundRect = moneyPanelBackground.AddComponent<RectTransform>();
            moneyBackgroundRect.anchorMax = new Vector2(1f, 1f);
            moneyBackgroundRect.anchorMin = new Vector2(0f, 0f);
            moneyBackgroundRect.offsetMax = new Vector2(0f, 0f);
            moneyBackgroundRect.offsetMin = new Vector2(0f, 0f);
            moneyBackgroundRect.sizeDelta = new Vector2(0f, 0f);
            moneyPanelBackground.AddComponent<CanvasRenderer>();
            var moneyBackgroundImage = moneyPanelBackground.AddComponent<Image>();
            var moneyBackgroundTexture = Global.LoadImageToTexture2d("CharSheet_AccType_Btn.png");
            moneyBackgroundImage.sprite = Sprite.Create(moneyBackgroundTexture, new Rect(0, 0, moneyBackgroundTexture.width, moneyBackgroundTexture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, new Vector4(7, 7, 7, 7));
            moneyBackgroundImage.type = Image.Type.Sliced;

            // Money currency display
            var moneyCurrencyWrapper = new GameObject("CurrencyWrapper");
            moneyCurrencyWrapper.transform.SetParent(moneyPanel.transform, false);
            var moneyCurrencyWrapperRect = moneyCurrencyWrapper.AddComponent<RectTransform>();
            moneyCurrencyWrapperRect.anchorMax = new Vector2(1f, 1f);
            moneyCurrencyWrapperRect.anchorMin = new Vector2(0f, 0f);
            moneyCurrencyWrapperRect.sizeDelta = new Vector2(0, 0);
            var moneyCurrencyWrapperLayout = moneyCurrencyWrapper.AddComponent<HorizontalLayoutGroup>();
            moneyCurrencyWrapperLayout.childControlHeight = true;
            moneyCurrencyWrapperLayout.childControlWidth = true;
            moneyCurrencyWrapperLayout.childForceExpandHeight = false;
            moneyCurrencyWrapperLayout.childForceExpandWidth = false;
            moneyCurrencyWrapperLayout.spacing = 0;
            moneyCurrencyWrapperLayout.childAlignment = TextAnchor.MiddleRight;

            // Currency objects
            var mithril = new GameObject("Mithril");
            mithril.transform.SetParent(moneyCurrencyWrapper.transform, false);
            _currencyMithril = mithril.AddComponent<CurrencyDisplay>();
            _currencyMithril.Initialize("mithril.png");

            var platinum = new GameObject("Platinum");
            platinum.transform.SetParent(moneyCurrencyWrapper.transform, false);
            _currencyPlatinum = platinum.AddComponent<CurrencyDisplay>();
            _currencyPlatinum.Initialize("platinum.png");

            var gold = new GameObject("Gold");
            gold.transform.SetParent(moneyCurrencyWrapper.transform, false);
            _currencyGold = gold.AddComponent<CurrencyDisplay>();
            _currencyGold.Initialize("gold.png");

            var silver = new GameObject("Silver");
            silver.transform.SetParent(moneyCurrencyWrapper.transform, false);
            _currencySilver = silver.AddComponent<CurrencyDisplay>();
            _currencySilver.Initialize("silver.png");

            var copper = new GameObject("Copper");
            copper.transform.SetParent(moneyCurrencyWrapper.transform, false);
            _currencyCopper = copper.AddComponent<CurrencyDisplay>();
            _currencyCopper.Initialize("copper.png");

            // Check if we need to disable uiDraggable
            var lockPosition = Global.StorageAsBagCategory.GetEntry<bool>(Global.StorageAsBagCategoryLockPosition).Value;
            EnableUiDraggable(lockPosition);

            // Check if we need to open the bag to start with
            if (Global.StorageAsBagCategory.GetEntry<bool>(Global.StorageAsBagCategoryBagEnabled).Value)
                OpenBag();
            else
                ShowHideOpenAsBagButton(true);
        }

        /// <summary>
        /// This creates a cover over the character panel where the normal storage is which allows you to return the normal storage
        /// if our bag is open. When bag is shown this cover is shown. If the bag isn't shown then this cover is hidden.
        /// </summary>
        private static void CreateStorageCover()
        {
            if (_uiCharacterPanel == null || _storage == null)
                return;

            // We need to add this cover onto the character panel Overview Panel
            // In the same place as the storage when it's shown in the character panel
            var overViewPanel = _uiCharacterPanel.transform.Find("LeftTabs").Find("Tabs Content").Find("Adventuring").Find("[Overview Panel]");

            // Main object to be added to Overview Panel
            var storageCover = new GameObject("StorageCover");
            storageCover.transform.SetParent(overViewPanel, false);

            // We match the size and position of the storage
            var storageCoverRect = storageCover.AddComponent<RectTransform>();
            storageCoverRect.anchoredPosition = Global.StorageAsBagCategory.GetEntry<Vector2>(Global.StorageAsBagCategoryStorageAnchorPosition).Value;
            storageCoverRect.sizeDelta = _storage.GetComponent<RectTransform>().sizeDelta;

            // Add a black image to cover where the storage was
            var storageCoverImage = storageCover.AddComponent<Image>();
            storageCoverImage.color = new Color(0, 0, 0, 1);

            // Canvas group so we can handle visibility
            _characterPanelReturnStorage = storageCover.AddComponent<CanvasGroup>();

            // Button on top of the image to allow the user to return the storage and close the bag
            var button = new GameObject("Return Storage");
            button.transform.SetParent(storageCover.transform, false);
            var buttonRect = button.AddComponent<RectTransform>();
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(175f, 25f);
            button.AddComponent<CanvasRenderer>();
            var buttonImage = button.AddComponent<Image>();
            buttonImage.type = Image.Type.Sliced;
            var theButton = button.AddComponent<Button>();
            theButton.onClick.AddListener(new Action(CloseBag));
            var buttonHoverSwap = button.AddComponent<ButtonHoverSpriteSwap>();
            buttonHoverSwap.Initialize("Blue");

            // Text of button
            var text = new GameObject("Text (TMP)");
            text.transform.SetParent(button.transform, false);
            text.AddComponent<RectTransform>();
            text.AddComponent<CanvasRenderer>();
            var buttonText = text.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Return Storage";
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.enableWordWrapping = true;
            buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            buttonText.fontSize = 12;
            buttonText.vertexBufferAutoSizeReduction = true;
        }

        /// <summary>
        /// UIDraggable allows the user to move the storage bag around the screen. If we disable it then
        /// the user can't move the bag around.
        /// </summary>
        /// <param name="enable"></param>
        private static void EnableUiDraggable(bool enable)
        {
            if (_uiDraggable == null)
                return;

            _uiDraggable.enabled = !enable;

            if (_positionLockImage != null)
            {
                if (enable)
                {
                    var spriteLockedTexture = Global.LoadImageToTexture2d("padlock-locked.png");
                    spriteLockedTexture.filterMode = FilterMode.Point;
                    _positionLockImage.sprite = Sprite.Create(spriteLockedTexture,
                        new Rect(0, 0, spriteLockedTexture.width, spriteLockedTexture.height),
                        new Vector2(0.5f, 0.5f));
                }
                else
                {
                    var spriteUnlockedTexture = Global.LoadImageToTexture2d("padlock-unlocked.png");
                    spriteUnlockedTexture.filterMode = FilterMode.Point;
                    _positionLockImage.sprite = Sprite.Create(spriteUnlockedTexture,
                        new Rect(0, 0, spriteUnlockedTexture.width, spriteUnlockedTexture.height),
                        new Vector2(0.5f, 0.5f));
                }
            }

            Global.StorageAsBagCategory.SetAndSavePreference(Global.StorageAsBagCategoryLockPosition, enable);
        }

        /// <summary>
        /// Lock is a button on the storage bag used to keep the panel in place or allow for movement
        /// </summary>
        private static void LockTogglePosition()
        {
            if (_uiDraggable == null)
                return;

            var currentSetting = Global.StorageAsBagCategory.GetEntry<bool>(Global.StorageAsBagCategoryLockPosition).Value;
            EnableUiDraggable(!currentSetting);
        }

        private static void OpenSettingsPanelToggle()
        {
            if (_settingsUiWindowPanel == null)
                return;

            if (_settingsUiWindowPanel.IsVisible)
            {
                _settingsUiWindowPanel.Hide();
                Global.StorageAsBagCategory.SaveToFile();
            }
            else
                _settingsUiWindowPanel.Show();
        }

        /// <summary>
        /// HarmonyPatch Postfix for UICharacterPanel.Awake. This is the first thing called in this class.
        /// It will then set up all the ui elements needed for the storage bag.
        /// </summary>
        /// <param name="__instance"></param>
        private static void Postfix(UICharacterPanel __instance)
        {
            _uiCharacterPanel = __instance;
            _storage = _uiCharacterPanel.transform
                .Find("LeftTabs")
                .Find("Tabs Content")
                .Find("Adventuring")
                .Find("[Overview Panel]")
                .Find("Storage");

            SetupButtonToOpenStorageAsBag();
            CreateStorageCover();
            CreateStorageBag();
            CreateSettingsPanel(_uiCharacterPanel.transform.parent);
        }

        private static void ScaleSettingChanged(Slider slider, TextMeshProUGUI text)
        {
            var sliderValue = slider.value;
            text.text = $"Scale: {sliderValue}";
            Global.StorageAsBagCategory.GetEntry<float>(Global.StorageAsBagCategoryScale).Value = sliderValue;

            if (_storageBag == null)
                return;

            _storageBag.GetComponent<RectTransform>().localScale = new Vector3(sliderValue / 100, sliderValue / 100, sliderValue / 100);
        }

        /// <summary>
        /// Button panel is the panel that's added to the very bottom of the character panel which allows you to open
        /// our new as bag. When the bag is shown this panel is hidden.
        /// </summary>
        private static void SetupButtonToOpenStorageAsBag()
        {
            if (_uiCharacterPanel == null)
                return;

            // Adventuring tab will be where we add our open storage as bag as well as our cover over the original storage
            var adventuringTab = _uiCharacterPanel.transform.Find("LeftTabs").Find("Tabs Content").Find("Adventuring");

            // Small panel we put below the character panel to allow the user to open the storage as a bag
            var storageBagOptionPanel = new GameObject("StorageBagOptionPanel");
            storageBagOptionPanel.transform.SetParent(adventuringTab, false);
            storageBagOptionPanel.transform.SetSiblingIndex(0);
            var storageBagOptionRect = storageBagOptionPanel.AddComponent<RectTransform>();
            storageBagOptionRect.anchoredPosition = new Vector2(79f, -339f);
            storageBagOptionRect.anchorMax = new Vector2(1f, 1f);
            storageBagOptionRect.anchorMin = new Vector2(0f, 1f);
            storageBagOptionRect.sizeDelta = new Vector2(105f, 45f);

            // Canvas group to allow us to hide/show the panel
            _characterPanelOpenAsBagOption = storageBagOptionPanel.AddComponent<CanvasGroup>();

            // Background object of the panel
            var background = new GameObject("Background");
            background.transform.SetParent(storageBagOptionPanel.transform, false);
            var backgroundRect = background.AddComponent<RectTransform>();
            backgroundRect.anchorMax = new Vector2(1f, 1f);
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.offsetMax = new Vector2(0f, 0f);
            backgroundRect.offsetMin = new Vector2(0f, 0f);
            backgroundRect.sizeDelta = new Vector2(0f, 0f);
            background.AddComponent<CanvasRenderer>();
            var backgroundImage = background.AddComponent<Image>();
            var backgroundTexture = Global.LoadImageToTexture2d("CharSheet_AccType_Btn.png");
            backgroundImage.sprite = Sprite.Create(backgroundTexture, new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, new Vector4(7, 7, 7, 7));
            backgroundImage.type = Image.Type.Sliced;

            // Button to allow the user to open the storage as a bag
            var button = new GameObject("Open Storage Button");
            button.transform.SetParent(storageBagOptionPanel.transform, false);
            var buttonRect = button.AddComponent<RectTransform>();
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(175f, 25f);
            button.AddComponent<CanvasRenderer>();
            var buttonImage = button.AddComponent<Image>();
            buttonImage.type = Image.Type.Sliced;
            var theButton = button.AddComponent<Button>();
            theButton.onClick.AddListener(new Action(OpenBag));
            var buttonHoverSwap = button.AddComponent<ButtonHoverSpriteSwap>();
            buttonHoverSwap.Initialize("Blue");

            // Text of button
            var text = new GameObject("Text (TMP)");
            text.transform.SetParent(button.transform, false);
            text.AddComponent<RectTransform>();
            text.AddComponent<CanvasRenderer>();
            var buttonText = text.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Open Storage As Bag";
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.enableWordWrapping = true;
            buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            buttonText.fontSize = 12;
            buttonText.vertexBufferAutoSizeReduction = true;
        }

        #endregion Private Methods
    }
}