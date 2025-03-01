using HarmonyLib;
using Il2Cpp;

namespace PantheonStorageAsBagMod.Patches
{
    /// <summary>
    /// This gets us the player object and hooks into the currency changed event
    /// </summary>
    [HarmonyPatch(typeof(EntityPlayerGameObject), nameof(EntityPlayerGameObject.NetworkStart))]
    internal class PlayerStart
    {
        #region Private Methods

        private static void Postfix(EntityPlayerGameObject __instance)
        {
            if (__instance.NetworkId.Value != EntityPlayerGameObject.LocalPlayerId.Value)
                return;

            var entityCurrency = __instance.GetComponent<EntityCurrency>();

            if (entityCurrency == null)
                return;

            Global.PlayerGameObject = __instance;

            // When currency has changed we hook into the event
            entityCurrency.logic.ChangedEvent += Global.CurrencyChangedHandler;

            StorageAsBag.CurrencyChanged(entityCurrency.logic.Current);
        }

        #endregion Private Methods
    }
}