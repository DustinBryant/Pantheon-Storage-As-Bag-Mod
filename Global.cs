using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.Bindings;

namespace PantheonStorageAsBagMod
{
    internal static class Global
    {
        #region Public Fields

        public static Action<Currency> CurrencyChangedHandler = StorageAsBag.CurrencyChanged;
        public static EntityPlayerGameObject? PlayerGameObject;

        public static MelonPreferences_Category StorageAsBagCategory = null!;
        public static string StorageAsBagCategoryBagAnchoredPosition = "BagAnchoredPosition";
        public static string StorageAsBagCategoryBagEnabled = "InventoryAsBagEnabled";
        public static string StorageAsBagCategoryLockPosition = "LockStorageBagPosition";
        public static string StorageAsBagCategoryStorageAnchorPosition = "StorageInventoryAnchoredPosition";

        #endregion Public Fields

        #region Public Methods

        /// <summary>
        /// Uses the path of the mod directory + provided imageName for our mod to load images from into Texture2D objects.
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public static Texture2D LoadImageToTexture2d(string imageName)
        {
            var imageLocation = Path.Combine(Path.GetDirectoryName(MelonEnvironment.ModsDirectory) ?? "", "Mods", "pantheon-storage-as-bag-mod", imageName);
            var imageAsBytes = File.ReadAllBytes(imageLocation);
            var image = new Texture2D(2, 2);

            unsafe
            {
                var intPtr = UnityEngine.Object.MarshalledUnityObject.MarshalNotNull(image);

                fixed (byte* ptr = imageAsBytes)
                {
                    var managedSpanWrapper = new ManagedSpanWrapper(ptr, imageAsBytes.Length);

                    ImageConversion.LoadImage_Injected(intPtr, ref managedSpanWrapper, false);
                }
            }

            return image;
        }

        /// <summary>
        /// Will set the preference value and also save that change to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <param name="entryName"></param>
        /// <param name="value"></param>
        public static void SetAndSavePreference<T>(this MelonPreferences_Category category, string entryName, T value)
        {
            category.GetEntry<T>(entryName).Value = value;
            category.SaveToFile();
        }

        #endregion Public Methods
    }
}