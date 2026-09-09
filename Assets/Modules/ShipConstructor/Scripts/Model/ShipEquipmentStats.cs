using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Model
{
    public struct ShipEquipmentStats
    {
        // Armor systems
        public float ArmorPoints;
        public float ArmorRepairRate;
        public StatMultiplier ArmorRepairCooldownMultiplier;

        // Energy systems
        public float EnergyPoints;
        public float EnergyRecharge;
        public float EnergyConsumption;
        public StatMultiplier EnergyRechargeCooldownMultiplier;

        // Shield systems
        public float ShieldPoints;
        public float ShieldRechargeRate;
        public StatMultiplier ShieldRechargeCooldownMultiplier;

        // Base cooldown constants
        public float ArmorRepairBaseCooldown;
        public float HullRepairBaseCooldown;
        public float EnergyRechargeBaseCooldown;
        public float ShieldRechargeBaseCooldown;

        // Mass and inertia compensation
        public float Weight;
        public float WeightReduction;

        // Damage interaction, ramming and collision
        public float EnergyAbsorption;
        public float RammingDamage;
        public StatMultiplier RammingDamageMultiplier;

        // Damage resistances
        public float KineticResistance;
        public float EnergyResistance;
        public float ThermalResistance;

        // Engine mechanics and boosters
        public float EnginePower;
        public float TurnRate;
        public float EnginePowerWithoutEnergy;
        public float TurnRateWithoutEnergy;
        public float EngineEnergyConsumption;
        public StatMultiplier EnginePowerMultiplier;
        public StatMultiplier TurnRateMultiplier;

        // Autopilot
        public bool Autopilot;

        // Drone systems and modifiers
        public StatMultiplier DroneRangeMultiplier;
        public StatMultiplier DroneDamageMultiplier;
        public StatMultiplier DroneDefenseMultiplier;
        public StatMultiplier DroneSpeedMultiplier;
        public float DroneReconstructionSpeed;
        public StatMultiplier DroneReconstructionTimeMultiplier;

        // Global ship weapon boosters
        public StatMultiplier WeaponFireRateMultiplier;
        public StatMultiplier WeaponDamageMultiplier;
        public StatMultiplier WeaponRangeMultiplier;
        public StatMultiplier WeaponEnergyCostMultiplier;
        public StatMultiplier WeaponVelocityMultiplier;
        public StatMultiplier WeaponAoeMultiplier;
        public StatMultiplier WeaponImpulseMultiplier;
        public StatMultiplier WeaponRecoilMultiplier;

        // Active devices boosters
        public StatMultiplier DeviceCooldownMultiplier;
        public StatMultiplier DevicePowerMultiplier;
        public StatMultiplier DeviceRangeMultiplier;

        public static ShipEquipmentStats FromComponent(ComponentStats component, int cellCount)
        {
            var stats = new ShipEquipmentStats();

            var multiplier = component.Type == ComponentStatsType.PerOneCell ? cellCount : 1.0f;

            // Armor
            stats.ArmorPoints = component.ArmorPoints * multiplier;
            stats.ArmorRepairRate = component.ArmorRepairRate * multiplier;
            stats.ArmorRepairCooldownMultiplier = new StatMultiplier(component.ArmorRepairCooldownModifier * multiplier);

            // Energy
            stats.EnergyPoints = component.EnergyPoints * multiplier;
            stats.EnergyRechargeCooldownMultiplier = new StatMultiplier(component.EnergyRechargeCooldownModifier * multiplier);

            if (component.EnergyRechargeRate > 0)
                stats.EnergyRecharge = component.EnergyRechargeRate * multiplier;
            else
                stats.EnergyConsumption = -component.EnergyRechargeRate * multiplier;

            // Shield
            stats.ShieldPoints = component.ShieldPoints * multiplier;
            stats.ShieldRechargeRate = component.ShieldRechargeRate * multiplier;
            stats.ShieldRechargeCooldownMultiplier = new StatMultiplier(component.ShieldRechargeCooldownModifier * multiplier);

            // Mass
            if (component.Weight > 0)
                stats.Weight = multiplier * component.Weight;
            else if (component.Weight < 0)
                stats.WeightReduction = multiplier * component.Weight;

            // Ramming and energy absorption
            stats.RammingDamage = component.RammingDamage * multiplier;
            stats.RammingDamageMultiplier = new StatMultiplier(component.RammingDamageModifier * multiplier);
            stats.EnergyAbsorption = component.EnergyAbsorption * multiplier;

            // Resistances
            stats.KineticResistance = component.KineticResistance * multiplier;
            stats.EnergyResistance = component.EnergyResistance * multiplier;
            stats.ThermalResistance = component.ThermalResistance * multiplier;

            // Engines and boosters
            stats.EnginePower = component.EnginePower * multiplier;
            stats.TurnRate = component.TurnRate * multiplier;
            stats.EnginePowerMultiplier = new StatMultiplier(component.EnginePowerModifier * multiplier);
            stats.TurnRateMultiplier = new StatMultiplier(component.TurnRateModifier * multiplier);

            if (component.EnergyRechargeRate >= 0 && component.EnginePower > 0)
                stats.EnginePowerWithoutEnergy += component.EnginePower * multiplier;
            if (component.EnergyRechargeRate >= 0 && component.TurnRate > 0)
                stats.TurnRateWithoutEnergy += component.TurnRate * multiplier;
            if (component.EnergyRechargeRate < 0 && component.EnginePower > 0)
                stats.EngineEnergyConsumption -= component.EnergyRechargeRate;

            // Autopilot
            stats.Autopilot = component.Autopilot;

            // Drones
            stats.DroneRangeMultiplier = new StatMultiplier(component.DroneRangeModifier * multiplier);
            stats.DroneDamageMultiplier = new StatMultiplier(component.DroneDamageModifier * multiplier);
            stats.DroneDefenseMultiplier = new StatMultiplier(component.DroneDefenseModifier * multiplier);
            stats.DroneSpeedMultiplier = new StatMultiplier(component.DroneSpeedModifier * multiplier);
            stats.DroneReconstructionTimeMultiplier = new StatMultiplier(component.DroneBuildTimeModifier * multiplier);
            stats.DroneReconstructionSpeed = component.DronesBuiltPerSecond * multiplier;

            // Weapon boosters
            stats.WeaponFireRateMultiplier = new StatMultiplier(component.WeaponFireRateModifier * multiplier);
            stats.WeaponDamageMultiplier = new StatMultiplier(component.WeaponDamageModifier * multiplier);
            stats.WeaponRangeMultiplier = new StatMultiplier(component.WeaponRangeModifier * multiplier);
            stats.WeaponEnergyCostMultiplier = new StatMultiplier(component.WeaponEnergyCostModifier * multiplier);
            stats.WeaponVelocityMultiplier = new StatMultiplier(component.WeaponVelocityModifier * multiplier);
            stats.WeaponAoeMultiplier = new StatMultiplier(component.WeaponAoeModifier * multiplier);
            stats.WeaponImpulseMultiplier = new StatMultiplier(component.WeaponImpulseModifier * multiplier);
            stats.WeaponRecoilMultiplier = new StatMultiplier(component.WeaponRecoilModifier * multiplier);

            // Device boosters
            stats.DeviceCooldownMultiplier = new StatMultiplier(component.DeviceCooldownModifier * multiplier);
            stats.DevicePowerMultiplier = new StatMultiplier(component.DevicePowerModifier * multiplier);
            stats.DeviceRangeMultiplier = new StatMultiplier(component.DeviceRangeModifier * multiplier);

            return stats;
        }

        public void AddNegativeStatsOnly(in ShipEquipmentStats other)
        {
            EnergyConsumption += other.EnergyConsumption;
            Weight += other.Weight;
        }

        public void AddStats(in ShipEquipmentStats other)
        {
            // Armor
            ArmorPoints += other.ArmorPoints;
            ArmorRepairRate += other.ArmorRepairRate;
            ArmorRepairCooldownMultiplier += other.ArmorRepairCooldownMultiplier;

            // Energy
            EnergyPoints += other.EnergyPoints;
            EnergyRecharge += other.EnergyRecharge;
            EnergyConsumption += other.EnergyConsumption;
            EnergyRechargeCooldownMultiplier += other.EnergyRechargeCooldownMultiplier;

            // Shield
            ShieldPoints += other.ShieldPoints;
            ShieldRechargeRate += other.ShieldRechargeRate;
            ShieldRechargeCooldownMultiplier += other.ShieldRechargeCooldownMultiplier;

            // Mass
            Weight += other.Weight;
            WeightReduction += other.WeightReduction;

            // Ramming and absorption
            EnergyAbsorption += other.EnergyAbsorption;
            RammingDamage += other.RammingDamage;
            RammingDamageMultiplier += other.RammingDamageMultiplier;

            // Resistances
            KineticResistance += other.KineticResistance;
            EnergyResistance += other.EnergyResistance;
            ThermalResistance += other.ThermalResistance;

            // Engines and boosters
            EnginePower += other.EnginePower;
            TurnRate += other.TurnRate;
            EnginePowerWithoutEnergy += other.EnginePowerWithoutEnergy;
            TurnRateWithoutEnergy += other.TurnRateWithoutEnergy;
            EngineEnergyConsumption += other.EngineEnergyConsumption;
            EnginePowerMultiplier += other.EnginePowerMultiplier;
            TurnRateMultiplier += other.TurnRateMultiplier;

            // Autopilot
            Autopilot |= other.Autopilot;

            // Drones
            DroneRangeMultiplier += other.DroneRangeMultiplier;
            DroneDamageMultiplier += other.DroneDamageMultiplier;
            DroneDefenseMultiplier += other.DroneDefenseMultiplier;
            DroneSpeedMultiplier += other.DroneSpeedMultiplier;
            DroneReconstructionSpeed += other.DroneReconstructionSpeed;
            DroneReconstructionTimeMultiplier += other.DroneReconstructionTimeMultiplier;

            // Weapon boosters
            WeaponFireRateMultiplier += other.WeaponFireRateMultiplier;
            WeaponDamageMultiplier += other.WeaponDamageMultiplier;
            WeaponRangeMultiplier += other.WeaponRangeMultiplier;
            WeaponEnergyCostMultiplier += other.WeaponEnergyCostMultiplier;
            WeaponVelocityMultiplier += other.WeaponVelocityMultiplier;
            WeaponAoeMultiplier += other.WeaponAoeMultiplier;
            WeaponImpulseMultiplier += other.WeaponImpulseMultiplier;
            WeaponRecoilMultiplier += other.WeaponRecoilMultiplier;

            // Device boosters
            DeviceCooldownMultiplier += other.DeviceCooldownMultiplier;
            DevicePowerMultiplier += other.DevicePowerMultiplier;
            DeviceRangeMultiplier += other.DeviceRangeMultiplier;
        }
    }
}