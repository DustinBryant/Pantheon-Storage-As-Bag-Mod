using MelonLoader;
using UnityEngine;

namespace PantheonStorageAsBagMod.Components
{
    /// <summary>
    /// This allows us to save our inventory as bag's position when scene changes.
    /// </summary>
    [RegisterTypeInIl2Cpp]
    internal class BagPositionSaver : MonoBehaviour
    {
        #region Private Methods

        private void OnDestroy()
        {
            var rectTransform = GetComponent<RectTransform>();

            if (rectTransform == null)
                return;

            Global.StorageAsBagCategory.SetAndSavePreference(Global.StorageAsBagCategoryBagAnchoredPosition, rectTransform.anchoredPosition);
        }

        #endregion Private Methods
    }
}