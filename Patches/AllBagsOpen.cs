using HarmonyLib;
using Il2Cpp;

namespace PantheonStorageAsBagMod.Patches;

[HarmonyPatch(typeof(UIBagManager), nameof(UIBagManager.ToggleAllBags))]
public class AllBagsOpen
{
    private static bool _opening = false;
    
    private static void Prefix(UIBagManager __instance)
    {
        _opening = false;

        if (Global.PlayerGameObject == null)
        {
            return;
        }
        
        foreach (var item in Global.PlayerGameObject.Inventory.items)
        {
            if (!item.value.IsEquippedBag() || item.value.CorpseID != 0)
            {
                continue;
            }

            UIBagManager.Instance.bagWindows.TryGetValue(item.Value.ItemInstanceGuid, out var uiBag);

            if (uiBag == null)
            {
                uiBag = UIBagManager.Instance.CreateBagWindowIfItDoesNotExist(item.Value);
            }

            if (!uiBag.Window.IsVisible)
            {
                _opening = true;
                break;
            }
        }
    }

    private static void Postfix(UIBagManager __instance)
    {
        if (_opening)
        {
            StorageAsBag.OpenBag();
        }
        else
        {
            StorageAsBag.CloseBag();
        }
    }
}