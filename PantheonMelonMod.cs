using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

namespace PantheonStorageAsBagMod
{
    public class PantheonMelonMod : MelonMod
    {
        #region Public Methods

        public override void OnLateInitializeMelon()
        {
            // Set up preferences
            Global.StorageAsBagCategory = MelonPreferences.CreateCategory("StorageAsBagModCategory", "Storage Bag");
            Global.StorageAsBagCategory.SetFilePath(Path.Combine(Path.GetDirectoryName(MelonEnvironment.UserDataDirectory) ?? "", "UserData", "StorageAsBagMod.cfg"));
            Global.StorageAsBagCategory.CreateEntry(Global.StorageAsBagCategoryStorageAnchorPosition, new Vector2(-47.12f, -156.91f));
            Global.StorageAsBagCategory.CreateEntry(Global.StorageAsBagCategoryBagEnabled, false);
            Global.StorageAsBagCategory.CreateEntry(Global.StorageAsBagCategoryBagAnchoredPosition, new Vector2(0, 0));
            Global.StorageAsBagCategory.CreateEntry(Global.StorageAsBagCategoryLockPosition, false);
            Global.StorageAsBagCategory.CreateEntry(Global.StorageAsBagCategoryScale, 100f);
            Global.StorageAsBagCategory.SaveToFile();
        }

        #endregion Public Methods
    }
}