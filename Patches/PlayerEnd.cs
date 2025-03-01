using HarmonyLib;
using Il2Cpp;

namespace PantheonStorageAsBagMod.Patches
{
    /// <summary>
    /// We need to make sure to remove our currencyChanged event handler when the player gets destroyed
    /// </summary>
    [HarmonyPatch(typeof(EntityPlayerGameObject), nameof(EntityPlayerGameObject.NetworkStop))]
    internal class PlayerEnd
    {
        #region Private Methods

        private static void Prefix(EntityPlayerGameObject __instance)
        {
            if (__instance.NetworkId.Value != EntityPlayerGameObject.LocalPlayerId.Value)
                return;

            var entityCurrency = __instance.GetComponent<EntityCurrency>();

            if (entityCurrency == null)
                return;

            // Remove our event so GC can do its job
            entityCurrency.logic.ChangedEvent -= Global.CurrencyChangedHandler;

            Global.PlayerGameObject = null;
        }

        #endregion Private Methods
    }
}