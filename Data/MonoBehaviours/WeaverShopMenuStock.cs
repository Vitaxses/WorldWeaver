namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Weaver Shop Menu Stock")]
    public class WeaverShopMenuStock : ShopMenuStock
    {
        public new void DisplayCurrencyCounters()
        {
            base.DisplayCurrencyCounters();

            bool showRosary = false;
            bool showShard = false;

            foreach (ShopItem shopItem in stock)
            {
                if (shopItem == null || !shopItem.IsAvailable || shopItem is not WeaverShopItem weaverShopItem)
                    continue;
                    
                if (weaverShopItem.shellShardCost > 0)
                    showShard = true;
                
                if (weaverShopItem.rosaryCost > 0)
                    showRosary = true;
            }

            if (showRosary)
                CurrencyCounter.Show(CurrencyType.Money, setStackVisible: true);
            
            if (showShard)
                CurrencyCounter.Show(CurrencyType.Shard, setStackVisible: true);
        }

        private new void Start() // Unsure if new actually does what I think it does but it works so whatever
        {
            SpawnStock();
        }

        public new void SpawnStock()
        {
            if (MasterList != null)
            {
                spawnedStock = MasterList.spawnedStock;
                spawnedSubItems = MasterList.spawnedSubItems;
                subItemsLayout = MasterList.subItemsLayout;
                return;
            }

            lastSpawnTime = Time.time;
            int availableItems = 0;
            int subItems = 0;

            foreach (ShopItem item in stock)
            {
                if (item.IsAvailable)
                    availableItems++;

                if (!item.HasSubItems)
                    continue;

                int subItemsCount = item.SubItemsCount;
                if (subItemsCount > subItems)
                    subItems = subItemsCount;
            }

            templateItem.gameObject.SetActive(value: false);
            templateSubItem?.gameObject.SetActive(value: false);

            int itemsToSpawn = availableItems - spawnedStock.Count;
            if (itemsToSpawn > 0)
            {
                Transform parent = templateItem.transform.parent;
                bool wasParentActive = false;

                if (parent != null)
                {
                    wasParentActive = parent.gameObject.activeSelf;
                    parent.gameObject.SetActive(value: true);
                }
                while (itemsToSpawn > 0)
                {
                    ShopItemStats shopItemStats = Instantiate(templateItem, parent);
                    shopItemStats.gameObject.SetActive(value: true);
                    shopItemStats.gameObject.SetActive(value: false);
                    spawnedStock.Add(shopItemStats);
                    itemsToSpawn--;
                }

                if (!wasParentActive)
                {
                    parent!.gameObject.SetActive(value: false);
                }
            }
            int subItemsToSpawn = subItems - spawnedSubItems.Count;
            if (subItemsToSpawn > 0)
            {
                Transform parent = templateSubItem!.transform.parent;
                bool wasParentActive = false;

                if (parent != null)
                {
                    wasParentActive = parent.gameObject.activeSelf;
                    parent.gameObject.SetActive(value: true);
                }

                while (subItemsToSpawn > 0)
                {
                    ShopSubItemStats shopSubItemStats = Instantiate(templateSubItem, parent);
                    shopSubItemStats.gameObject.SetActive(value: true);
                    shopSubItemStats.gameObject.SetActive(value: false);
                    spawnedSubItems.Add(shopSubItemStats);
                    subItemsToSpawn--;
                }
                
                if (!wasParentActive)
                {
                    parent!.gameObject.SetActive(value: false);
                }
            }
            
            int num = 0;
            bool pooled = false;

            foreach (ShopItem item in stock)
            {
                if (num >= spawnedStock.Count)
                    break;

                if (!item.IsAvailable)
                    continue;

                spawnedStock[num++].Item = item;
                
                if (item.EnsurePool(gameObject))
                    pooled = true;
            }

            if (pooled)
                PersonalObjectPool.EnsurePooledInSceneFinished(gameObject);

            for (int j = num; j < spawnedStock.Count; j++)
                spawnedStock[j].Item = null;
        }

        public new void SetStock(ShopItem[] newStock)
        {
            if (stock != newStock || !(Time.timeAsDouble - lastSpawnTime < 0.5))
            {
                stock = newStock;
                bool activeSelf = gameObject.activeSelf;
                if (!activeSelf)
                    gameObject.SetActive(value: true);

                SpawnStock();

                if (!activeSelf)
                    gameObject.SetActive(value: false);
            }
        }

        public new void BuildItemList()
        {
            SpawnStock();
            availableStock.Clear();
            
            float y = 0f;
            foreach (ShopItemStats item in spawnedStock)
            {
                if (!item.IsAvailable())
                    continue;

                item.transform.localPosition = new Vector3(0f, y, 0f);
                item.ItemNumber = availableStock.Count;
                availableStock.Add(item);
                y += yDistance;
                item.gameObject.SetActive(value: true);

                if (item is WeaverShopItemStats weaverStats)
                    weaverStats.UpdateAppearance();
                else
                    item.UpdateAppearance();
            }

            foreach (ShopSubItemStats spawnedSubItem in spawnedSubItems)
                spawnedSubItem.gameObject.SetActive(value: false);
        }

        public int GetCost(int itemNum, bool isRosaryCost)
        {
            var item = availableStock[itemNum].Item;
            if (item is not WeaverShopItem weaverItem)
            {
                if (item.CurrencyType == CurrencyType.Money && isRosaryCost)
                    return GetCost(itemNum);
                else if (item.CurrencyType == CurrencyType.Shard && !isRosaryCost)
                    return GetCost(itemNum);

                return 0;
            }

            return isRosaryCost ? weaverItem.rosaryCost : weaverItem.shellShardCost;
        }

        public new bool CanBuy(int itemNum)
        {
            var stats = availableStock[itemNum];
            if (stats is WeaverShopItemStats weaverStats)
                return weaverStats.CanBuy();

            return stats.CanBuy();
        }

        public new void BuyFail(int itemNum)
        {
            var stats = availableStock[itemNum];
            if (stats is WeaverShopItemStats weaverStats)
                weaverStats.BuyFail();

            stats.BuyFail();
        }
    }
}
