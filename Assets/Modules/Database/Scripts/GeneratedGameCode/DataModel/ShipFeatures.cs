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
	public partial class ShipFeatures 
	{
		partial void OnDataDeserialized(ShipFeaturesSerializable serializable, Database.Loader loader);

		public static ShipFeatures Create(ShipFeaturesSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipFeatures(serializable, loader);
		}

		private ShipFeatures(ShipFeaturesSerializable serializable, Database.Loader loader)
		{
			EnergyResistance = UnityEngine.Mathf.Clamp(serializable.EnergyResistance, -100f, 100f);
			KineticResistance = UnityEngine.Mathf.Clamp(serializable.KineticResistance, -100f, 100f);
			HeatResistance = UnityEngine.Mathf.Clamp(serializable.HeatResistance, -100f, 100f);
			ShipWeightBonus = UnityEngine.Mathf.Clamp(serializable.ShipWeightBonus, -1f, 100f);
			EquipmentWeightBonus = UnityEngine.Mathf.Clamp(serializable.EquipmentWeightBonus, -1f, 100f);
			VelocityBonus = UnityEngine.Mathf.Clamp(serializable.VelocityBonus, -1f, 100f);
			TurnRateBonus = UnityEngine.Mathf.Clamp(serializable.TurnRateBonus, -1f, 100f);
			ArmorBonus = UnityEngine.Mathf.Clamp(serializable.ArmorBonus, -1f, 100f);
			ShieldBonus = UnityEngine.Mathf.Clamp(serializable.ShieldBonus, -1f, 100f);
			EnergyBonus = UnityEngine.Mathf.Clamp(serializable.EnergyBonus, -1f, 100f);
			DroneBuildSpeedBonus = UnityEngine.Mathf.Clamp(serializable.DroneBuildSpeedBonus, -1f, 100f);
			DroneAttackBonus = UnityEngine.Mathf.Clamp(serializable.DroneAttackBonus, -1f, 100f);
			DroneDefenseBonus = UnityEngine.Mathf.Clamp(serializable.DroneDefenseBonus, -1f, 100f);
			DroneRangeBonus = UnityEngine.Mathf.Clamp(serializable.DroneRangeBonus, -1f, 100f);
			DroneSpeedBonus = UnityEngine.Mathf.Clamp(serializable.DroneSpeedBonus, -1f, 100f);
			DronesBuiltPerSecondBonus = UnityEngine.Mathf.Clamp(serializable.DronesBuiltPerSecondBonus, 0f, 100f);
			DroneCapacityBonus = UnityEngine.Mathf.Clamp(serializable.DroneCapacityBonus, 0, 100);
			Regeneration = serializable.Regeneration;
			RegenerationAmount = UnityEngine.Mathf.Clamp(serializable.RegenerationAmount, -0.1f, 1f);
			BuiltinDevices = new ImmutableCollection<Device>(serializable.BuiltinDevices?.Select(item => loader.GetDevice(new ItemId<Device>(item), true)));

			OnDataDeserialized(serializable, loader);
		}

		public float EnergyResistance { get; private set; }
		public float KineticResistance { get; private set; }
		public float HeatResistance { get; private set; }
		public float ShipWeightBonus { get; private set; }
		public float EquipmentWeightBonus { get; private set; }
		public float VelocityBonus { get; private set; }
		public float TurnRateBonus { get; private set; }
		public float ArmorBonus { get; private set; }
		public float ShieldBonus { get; private set; }
		public float EnergyBonus { get; private set; }
		public float DroneBuildSpeedBonus { get; private set; }
		public float DroneAttackBonus { get; private set; }
		public float DroneDefenseBonus { get; private set; }
		public float DroneRangeBonus { get; private set; }
		public float DroneSpeedBonus { get; private set; }
		public float DronesBuiltPerSecondBonus { get; private set; }
		public int DroneCapacityBonus { get; private set; }
		public bool Regeneration { get; private set; }
		public float RegenerationAmount { get; private set; }
		public ImmutableCollection<Device> BuiltinDevices { get; private set; }

		public static ShipFeatures DefaultValue { get; private set; }= new(new(), null);
	}
}
