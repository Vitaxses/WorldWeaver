using System;
using UnityEngine;

namespace WorldWeaver.Data
{
    [CreateAssetMenu(menuName = "WorldWeaver/Shop Item")]
    public class WeaverShopItem : ShopItem
    {   
        public int rosaryCost = 0;
        public int shellShardCost = 0;

        public new void SetPurchased(Action onComplete, int subItemIndex)
        {
            cost = 0;
            costReference = null;

            CurrencyManager.TakeGeo(rosaryCost);
            CurrencyManager.TakeShards(shellShardCost);

            base.SetPurchased(onComplete, subItemIndex);
        }
    }
}