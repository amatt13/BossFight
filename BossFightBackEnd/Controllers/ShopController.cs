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

        public static string BuyWeapon(string pLootName, string pClientId)
        {
            var resultText = "";
            pLootName = pLootName.ToLower();
            try
            {
                throw new MyException();
            }
            catch (MyException)
            {
                resultText = $"Could not find item '{ pLootName }'";
            }
            // var purchasingPlayer = next(from p in gameManager.globalGameManager.players
            //                              where p.playerId == pClientId.author.id.ToString()
            //                              select p);
            // if (purchasingPlayer.loot.Contains(itemToPurchase.lootId))
            // {
            //     var replyMessage = "You already own '{itemToPurchase.lootName}'";
            // }
            // else if (itemToPurchase.cost > purchasingPlayer.gold)
            // {
            //     replyMessage = "You can't afford that item. Current gold: {purchasingPlayer.gold:,}, item cost: {itemToPurchase.cost:,}";
            // }
            // else
            // {
            //     purchasingPlayer.gold -= itemToPurchase.cost;
            //     purchasingPlayer.addLoot(itemToPurchase.lootId);
            //     gameManager.globalGameManager.persistChanges();
            //     replyMessage = "You bough the {itemToPurchase.lootName} for {itemToPurchase.cost:,} gold";
            // }
            return resultText;
        }

        public static string BuyPlayerClass(int pPlayerClassId, int pPlayerId)
        {
            var resultText = "";

            
            // var purchasingPlayer = next(from p in gameManager.globalGameManager.players
            //                              where p.playerId == pClientId.author.id.ToString()
            //                              select p);
            // // check player haven't already unlocked class
            // Debug.Assert(!(from pc in purchasingPlayer.playerClassList
            //                select pc.playerClassId).ToList().Contains(classToPurchase.playerClassId));
            // Debug.Assert("You have already acquired that class");
            // // check player can afford class
            // Debug.Assert(purchasingPlayer.gold >= classToPurchase.purchasePrice);
            // Debug.Assert($"You can't afford '{classToPurchase.name}'. Current gold: {purchasingPlayer.gold:,}, item cost: {classToPurchase.purchasePrice:,}");
            // // check player have fulfilled class requirements
            // classToPurchase.haveMetRequirements(purchasingPlayer.playerClassList);
            // // make the actual purchase
            // purchasingPlayer.gold -= classToPurchase.purchasePrice;
            // purchasingPlayer.playerClassList.append(classToPurchase.@Class(purchasingPlayer));
            // gameManager.globalGameManager.persistChanges();
            // var buyMessage = $"You bough the {classToPurchase.name} for {classToPurchase.purchasePrice:,} gold";
            return resultText;
        }

        // public string BuyItem(string pThingToBuyId, string pClientId)
        // {
        //     string replyMessage = String.Empty;
        //     // replyMessage = buyWeapon(int(itemId), playerMessage)
        //     try
        //     {
        //         replyMessage = BuyClass(pThingToBuyId, pClientId);
        //     }
        //     catch (Exception ae)
        //     {
        //         replyMessage = $"Failed to acquire class: { ae }";
        //     }
        //     catch
        //     {
        //         //TODO will now reach this place
        //         // Should be split up anyways
        //         try
        //         {
        //             replyMessage = BuyWeapon(pThingToBuyId, pClientId);
        //         }
        //         catch
        //         {
        //             replyMessage = $"no items found";
        //         }
        //     }
        //     return replyMessage;
        // }

        public static string SellItem(string pLootName, string pClientId)
        {
            string mes;
            try
            {
                // var sellingPlayer = next(from p in gameManager.globalGameManager.players
                //                           where p.playerId == pClientId.author.id.ToString()
                //                           select p);
                // var weaponToSell = statics.findLootByName(pLootName);
                // Debug.Assert(sellingPlayer.loot.Contains(weaponToSell.lootId));
                mes = $"You don't own '{ pLootName }'";
                //mes = sellingPlayer.sellLoot(weaponToSell);
            }
            catch (Exception err)
            {
                mes = err.ToString();
            }
            return mes;
        }

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
