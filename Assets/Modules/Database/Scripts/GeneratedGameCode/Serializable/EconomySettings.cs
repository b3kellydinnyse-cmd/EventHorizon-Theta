//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace GameDatabase.Serializable
{
	[Serializable]
	public class EconomySettingsSerializable : SerializableItem
	{
		public bool EnablePremiumCurrency = true;
		public bool EnableCraftingWithStars = true;
		public bool EnablePremiumPrices = true;
		public bool EnablePremiumComponentPrices = true;
		public bool EnableBlackMarketWithStars = true;
		public bool EnableShipUpgradesWithStars = true;
	}
}
