//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class EconomySettings 
	{
		partial void OnDataDeserialized(EconomySettingsSerializable serializable, Database.Loader loader);

		public static EconomySettings Create(EconomySettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new EconomySettings(serializable, loader);
		}

		private EconomySettings(EconomySettingsSerializable serializable, Database.Loader loader)
		{
			EnablePremiumCurrency = serializable.EnablePremiumCurrency;
			EnableCraftingWithStars = serializable.EnableCraftingWithStars;
			EnablePremiumPrices = serializable.EnablePremiumPrices;
			EnablePremiumComponentPrices = serializable.EnablePremiumComponentPrices;
			EnableBlackMarketWithStars = serializable.EnableBlackMarketWithStars;
			EnableShipUpgradesWithStars = serializable.EnableShipUpgradesWithStars;

			OnDataDeserialized(serializable, loader);
		}

		public bool EnablePremiumCurrency { get; private set; }
		public bool EnableCraftingWithStars { get; private set; }
		public bool EnablePremiumPrices { get; private set; }
		public bool EnablePremiumComponentPrices { get; private set; }
		public bool EnableBlackMarketWithStars { get; private set; }
		public bool EnableShipUpgradesWithStars { get; private set; }

		public static EconomySettings DefaultValue { get; private set; }
	}
}
