using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BossFight.Models;
using BossFight.CustemExceptions;

namespace BossFight.Controllers
{
    public static class ShopController
    {
        public static Dictionary<string, object> GetShopForPlayer(Player pPlayer)
        {
            var shop = new Dictionary<string, object>();
            var playerClasses = PlayerUnlocks.UnlockedClasses(pPlayer, false);
            shop["playerClasses"] = playerClasses;

            //TODO add weapons as well

            return shop;
        }

        public static Tuple<bool, string> BuyPlayerClass(PlayerClass playerClassToPurchase, Player pPurchasingPlayer)
        {
            var resultText = "";
            var purchaseComplete = false;

            if (pPurchasingPlayer.Gold >= playerClassToPurchase.PurchasePrice)
            {
                using var connection = GlobalConnection.GetNewOpenConnection();
                var transaction = connection.BeginTransaction();
                try
                {
                    pPurchasingPlayer.Gold -= playerClassToPurchase.PurchasePrice;
                    var purchasedPlayerClass = new PlayerPlayerClass {
                        PlayerId = pPurchasingPlayer.PlayerId,
                        PlayerClassId = playerClassToPurchase.PlayerClassId,
                        Level = 1,
                        XP = 0,
                        Active = false};

                    pPurchasingPlayer.Persist();
                    purchasedPlayerClass.Persist();
                    transaction.Commit();

                    purchaseComplete = true;
                    resultText = $"You bough the {playerClassToPurchase.Name} for {playerClassToPurchase.PurchasePrice:n0} gold";
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to persis PlayerPlayerClass Acquisition");
                    Console.WriteLine(e);
                    transaction.Rollback();
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                resultText = $"You can't afford '{playerClassToPurchase.Name}'. Current gold: {pPurchasingPlayer.Gold:n0}, item cost: {playerClassToPurchase.PurchasePrice:n0}";
            }

            return new Tuple<bool, string>(purchaseComplete, resultText);
        }


        // currently unused
        public static string AutoSellItem(string pLootName, string pClientId)
        {
            string mes;
            try
            {
                var autoSellingPlayer = new Player();
                var weaponToUpdateAutoSellStatus = new Weapon(-1, "", "");

                if (autoSellingPlayer.LootIsInAutoSellList(weaponToUpdateAutoSellStatus.LootId))
                {
                    autoSellingPlayer.AutoSellList.Remove(weaponToUpdateAutoSellStatus.LootId);
                    mes = $"Removed '{weaponToUpdateAutoSellStatus.LootName}' from auto-sell list";
                }
                else
                {
                    autoSellingPlayer.AutoSellList.Add(weaponToUpdateAutoSellStatus.LootId);
                    mes = $"Added '{weaponToUpdateAutoSellStatus.LootName}' to auto-sell list";
                }

                // else
                // {
                //     mes = "Loot could not be added/removed from auto-sell list";
                // }
            }
            catch (Exception err)
            {
                mes = err.ToString();
            }
            return mes;
        }
    }
}
