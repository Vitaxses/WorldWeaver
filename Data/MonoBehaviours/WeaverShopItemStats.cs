namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Weaver Shop Item Stats")]
    public class WeaverShopItemStats : ShopItemStats
    {
        [SerializeField]
        GameObject rosariesCostParent = null!;

        [SerializeField]
        GameObject shardsCostParent = null!;

        [SerializeField]
        SpriteRenderer shardsCostSprite = null!;
        
        [SerializeField]
        TMProOld.TMP_Text shardsCostText = null!;
         
        public new bool DisplayMoneyCost
        {
            get
            {
                if (shopItem is WeaverShopItem weaverItem)
                    return weaverItem.rosaryCost > 0;

                return shopItem.Cost > 0 || !shopItem.RequiredItem;
            }
        }

        public new void SetPurchased(Action onComplete, int subItemIndex)
        {
            if (Item is WeaverShopItem weaverShopItem)
                weaverShopItem.SetPurchased(onComplete, subItemIndex);
            else
                Item.SetPurchased(onComplete, subItemIndex);
        }

        public new bool CanBuy()
        {
            if (Item is WeaverShopItem weaverShopItem)
                return HasRequiredItems() && PlayerData.instance.ShellShards >= weaverShopItem.shellShardCost && PlayerData.instance.geo >= weaverShopItem.rosaryCost;

            return base.CanBuy();
        }

        public new void BuyFail()
        {
            PlayerData pd = PlayerData.instance;
            if (shopItem is not WeaverShopItem weaverItem)
            {
                base.BuyFail();
                return;
            }

            if (pd.geo < weaverItem.rosaryCost)
                CurrencyCounter.ReportFail(CurrencyType.Money);
                
            if (pd.ShellShards < weaverItem.shellShardCost)
                CurrencyCounter.ReportFail(CurrencyType.Shard);
        }

        public new void UpdateAppearance()
        {
            base.UpdateAppearance();

            rosariesCostParent.SetActive(true);
            shardsCostParent.SetActive(false);

            if (Item is not WeaverShopItem item)
                return;

            if (DisplayMoneyCost)
            {
                itemCostText.text = item.rosaryCost.ToString();

                costSprite.sprite = rosarySprite;
                costSprite.transform.localScale = initialCostSpriteScale;

                itemCostText.color = costSprite.color = CanBuy() ? activeColour : inactiveColour;
            }

            rosariesCostParent.SetActive(item.rosaryCost > 0);
            shardsCostParent.SetActive(item.shellShardCost > 0);
            shardsCostText.text = item.shellShardCost.ToString();
            
            shardsCostText.color = shardsCostSprite.color = CanBuy() ? activeColour : inactiveColour;
        }
    }
}
