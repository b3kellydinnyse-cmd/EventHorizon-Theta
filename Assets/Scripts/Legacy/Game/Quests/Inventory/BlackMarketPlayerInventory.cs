using System.Collections.Generic;
using System.Linq;
using Economy;
using Economy.ItemType;
using Economy.Products;
using GameDatabase;
using GameServices.Player;

namespace GameModel
{
    namespace Quests
    {
        public class BlackMarketPlayerInventory : IInventory
        {
            // Injected dependencies
            public BlackMarketPlayerInventory(
                ProductFactory productFactory,
                PlayerResources playerResources,
                ItemTypeFactory itemTypeFactory,
                IDatabase database)
            {
                _productFactory = productFactory;
                _playerResources = playerResources;
                _itemTypeFactory = itemTypeFactory;
                _database = database;
            }

            public void Refresh() { }

            // Returns available currency products for black market transactions
            public IEnumerable<IProduct> Items
            {
                get
                {
                    // Check master switch and black market specific switch
                    bool allowStars = _database.EconomySettings == null ||
                        (_database.EconomySettings.EnablePremiumCurrency && _database.EconomySettings.EnableBlackMarketWithStars);

                    if (!allowStars)
                        yield break;

                    // Provide stars as spendable currency item if player has any
                    var itemType = _itemTypeFactory.CreateCurrencyItem(Currency.Stars);
                    if (_playerResources.Stars > 0)
                        yield return _productFactory.CreatePlayerProduct(itemType, itemType.MaxItemsToWithdraw);
                }
            }

            private readonly ProductFactory _productFactory;
            private readonly PlayerResources _playerResources;
            private readonly ItemTypeFactory _itemTypeFactory;
            private readonly IDatabase _database;
        }
    }
}