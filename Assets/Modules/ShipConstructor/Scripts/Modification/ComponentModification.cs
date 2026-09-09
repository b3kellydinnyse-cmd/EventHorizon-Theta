using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;

namespace Constructor.Modification
{
    class ComponentModification : IModification
    {
        private readonly ComponentMod _modification;
        private readonly ModificationQuality _quality;

        public static IModification Create(ComponentMod modification, ModificationQuality quality)
        {
            return modification == ComponentMod.Empty ? EmptyModification.Instance : new ComponentModification(modification, quality);
        }

        public ModificationQuality Quality => _quality;

        public string GetDescription(ILocalization localization)
        {
            if (_modification.Modifications.Count == 0)
                return string.Empty;

            var arguments = new string[_modification.Modifications.Count];
            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    // Format all flat/additive values as signed numbers (+150 / -50)
                    case StatModificationType.ExtraHitPoints:
                    case StatModificationType.FlatShieldPoints:
                    case StatModificationType.FlatShieldRechargeRate:
                    case StatModificationType.FlatEnergyCapacity:
                    case StatModificationType.FlatEnergyRecharge:
                    case StatModificationType.FlatEnginePower:
                    case StatModificationType.FlatTurnRate:
                    case StatModificationType.FlatMass:
                    case StatModificationType.FlatArmorRepairRate:
                    case StatModificationType.FlatWeightReduction:
                    case StatModificationType.FlatEnergyCost:
                    case StatModificationType.FlatEnergyAbsorption:
                    case StatModificationType.FlatRammingDamage:
                    case StatModificationType.FlatKineticResistance:
                    case StatModificationType.FlatThermalResistance:
                    case StatModificationType.FlatEnergyResistance:
                    case StatModificationType.FlatDroneReconstructionSpeed:
                    case StatModificationType.FlatWeaponDamage:
                    case StatModificationType.FlatWeaponRange:
                    case StatModificationType.FlatWeaponFireRate:
                    case StatModificationType.FlatWeaponBulletSpeed:
                    case StatModificationType.FlatWeaponBulletMass:
                    case StatModificationType.FlatWeaponAoe:
                    case StatModificationType.FlatWeaponEnergyCost:
                    case StatModificationType.FlatWeaponRecoil:
                    case StatModificationType.FlatDeviceCooldown:
                    case StatModificationType.FlatDeviceRange:
                    case StatModificationType.FlatDevicePower:
                    case StatModificationType.FlatDroneRange:
                        arguments[i] = Maths.Format.SignedFloat(value);
                        break;
                    default:
                        arguments[i] = Maths.Format.SignedPercent(value - 1.0f);
                        break;
                }
            }

            return localization.GetString(_modification.Description, arguments);
        }

        public void Apply(ref ShipEquipmentStats stats)
        {
            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    // Drone booster modifications
                    case StatModificationType.DroneAttack:
                        if (stats.DroneDamageMultiplier.HasValue)
                            stats.DroneDamageMultiplier = stats.DroneDamageMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.DroneDefense:
                        if (stats.DroneDefenseMultiplier.HasValue)
                            stats.DroneDefenseMultiplier = stats.DroneDefenseMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.DroneSpeed:
                        if (stats.DroneSpeedMultiplier.HasValue)
                            stats.DroneSpeedMultiplier = stats.DroneSpeedMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.DroneRange:
                        if (stats.DroneRangeMultiplier.HasValue)
                            stats.DroneRangeMultiplier = stats.DroneRangeMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.DroneReconstructionTime:
                        // Scale drone build time delay and reconstruction speed
                        if (stats.DroneReconstructionTimeMultiplier.HasValue && value > 0)
                            stats.DroneReconstructionTimeMultiplier = stats.DroneReconstructionTimeMultiplier.AmplifyDelta(value);
                        if (stats.DroneReconstructionSpeed != 0 && value > 0)
                            stats.DroneReconstructionSpeed /= value;
                        break;
                    case StatModificationType.FlatDroneReconstructionSpeed:
                        stats.DroneReconstructionSpeed += value;
                        break;

                    // Weapon booster modifications on equipment
                    case StatModificationType.WeaponDamage:
                        if (stats.WeaponDamageMultiplier.HasValue)
                            stats.WeaponDamageMultiplier = stats.WeaponDamageMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.WeaponFireRate:
                        if (stats.WeaponFireRateMultiplier.HasValue)
                            stats.WeaponFireRateMultiplier = stats.WeaponFireRateMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.WeaponRange:
                        if (stats.WeaponRangeMultiplier.HasValue)
                            stats.WeaponRangeMultiplier = stats.WeaponRangeMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.WeaponEnergyCost:
                        if (stats.WeaponEnergyCostMultiplier.HasValue && value > 0)
                            stats.WeaponEnergyCostMultiplier = stats.WeaponEnergyCostMultiplier.AmplifyDelta(value);
                        break;

                    // Core energy, shield and armor stats (multiplicative)
                    case StatModificationType.EnergyCapacity:
                        if (stats.EnergyPoints != 0)
                            stats.EnergyPoints *= value;
                        break;
                    case StatModificationType.EnergyRechargeRate:
                        stats.EnergyRecharge *= value;
                        break;
                    case StatModificationType.ShieldPoints:
                        if (stats.ShieldPoints != 0)
                            stats.ShieldPoints *= value;
                        break;
                    case StatModificationType.ShieldRechargeRate:
                        if (stats.ShieldRechargeRate != 0)
                            stats.ShieldRechargeRate *= value;
                        break;
                    case StatModificationType.ShieldRechargeCooldown:
                        if (stats.ShieldRechargeCooldownMultiplier.HasValue && value > 0)
                            stats.ShieldRechargeCooldownMultiplier = stats.ShieldRechargeCooldownMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.ArmorPoints:
                        if (stats.ArmorPoints != 0)
                            stats.ArmorPoints *= value;
                        break;
                    case StatModificationType.ArmorRepairRate:
                        stats.ArmorRepairRate *= value;
                        break;

                    // Flat additive equipment stats
                    case StatModificationType.ExtraHitPoints:
                        stats.ArmorPoints += value;
                        break;
                    case StatModificationType.FlatArmorRepairRate:
                        stats.ArmorRepairRate += value;
                        break;
                    case StatModificationType.FlatShieldPoints:
                        stats.ShieldPoints += value;
                        break;
                    case StatModificationType.FlatShieldRechargeRate:
                        stats.ShieldRechargeRate += value;
                        break;
                    case StatModificationType.FlatEnergyCapacity:
                        stats.EnergyPoints += value;
                        break;
                    case StatModificationType.FlatEnergyRecharge:
                        stats.EnergyRecharge += value;
                        break;
                    case StatModificationType.FlatEnginePower:
                        stats.EnginePower += value;
                        if (stats.EnginePowerWithoutEnergy > 0)
                            stats.EnginePowerWithoutEnergy += value;
                        break;
                    case StatModificationType.FlatTurnRate:
                        stats.TurnRate += value;
                        if (stats.TurnRateWithoutEnergy > 0)
                            stats.TurnRateWithoutEnergy += value;
                        break;
                    case StatModificationType.FlatMass:
                        stats.Weight += value;
                        break;
                    case StatModificationType.FlatWeightReduction:
                        stats.WeightReduction += value;
                        break;
                    case StatModificationType.FlatEnergyCost:
                        stats.EnergyConsumption += value;
                        break;
                    case StatModificationType.FlatEnergyAbsorption:
                        stats.EnergyAbsorption += value;
                        break;
                    case StatModificationType.FlatRammingDamage:
                        stats.RammingDamage += value;
                        break;
                    case StatModificationType.FlatKineticResistance:
                        stats.KineticResistance += value;
                        break;
                    case StatModificationType.FlatThermalResistance:
                        stats.ThermalResistance += value;
                        break;
                    case StatModificationType.FlatEnergyResistance:
                        stats.EnergyResistance += value;
                        break;

                    // Resistances, absorption and ramming stats (multiplicative)
                    case StatModificationType.Resistance:
                        if (stats.EnergyResistance != 0)
                            stats.EnergyResistance *= value;
                        if (stats.ThermalResistance != 0)
                            stats.ThermalResistance *= value;
                        if (stats.KineticResistance != 0)
                            stats.KineticResistance *= value;
                        break;
                    case StatModificationType.KineticResistance:
                        if (stats.KineticResistance != 0)
                            stats.KineticResistance *= value;
                        break;
                    case StatModificationType.ThermalResistance:
                        if (stats.ThermalResistance != 0)
                            stats.ThermalResistance *= value;
                        break;
                    case StatModificationType.EnergyResistance:
                        if (stats.EnergyResistance != 0)
                            stats.EnergyResistance *= value;
                        break;
                    case StatModificationType.EnergyAbsorption:
                        if (stats.EnergyAbsorption != 0)
                            stats.EnergyAbsorption *= value;
                        break;
                    case StatModificationType.RammingDamage:
                        if (stats.RammingDamage != 0)
                            stats.RammingDamage *= value;
                        if (stats.RammingDamageMultiplier.HasValue)
                            stats.RammingDamageMultiplier = stats.RammingDamageMultiplier.AmplifyDelta(value);
                        break;

                    // Engine mechanics (multiplicative)
                    case StatModificationType.EnginePower:
                        if (stats.EnginePower != 0)
                            stats.EnginePower *= value;
                        if (stats.EnginePowerWithoutEnergy != 0)
                            stats.EnginePowerWithoutEnergy *= value;
                        break;
                    case StatModificationType.EngineTurnRate:
                        if (stats.TurnRate != 0)
                            stats.TurnRate *= value;
                        if (stats.TurnRateWithoutEnergy != 0)
                            stats.TurnRateWithoutEnergy *= value;
                        break;

                    // Mass and inertia compensation (multiplicative)
                    case StatModificationType.Mass:
                        if (stats.Weight != 0)
                            stats.Weight *= value;
                        break;

                    case StatModificationType.WeightReduction:
                        if (stats.WeightReduction != 0)
                            stats.WeightReduction *= value;
                        break;

                    case StatModificationType.EnergyCost:
                        stats.EnergyConsumption *= value;
                        break;
                }
            }
        }

        public void Apply(ref DeviceStats device)
        {
            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    case StatModificationType.DeviceCooldown:
                        device.Cooldown *= value;
                        break;
                    case StatModificationType.FlatDeviceCooldown:
                        device.Cooldown += value;
                        break;
                    case StatModificationType.DeviceRange:
                        device.Range *= value;
                        break;
                    case StatModificationType.FlatDeviceRange:
                        device.Range += value;
                        break;
                    case StatModificationType.DevicePower:
                        device.Power *= value;
                        break;
                    case StatModificationType.FlatDevicePower:
                        device.Power += value;
                        break;
                    case StatModificationType.EnergyCost:
                        if (device.EnergyConsumption != 0)
                            device.EnergyConsumption *= value;
                        break;
                    case StatModificationType.FlatEnergyCost:
                        device.EnergyConsumption += value;
                        break;
                }
            }
        }

        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition)
        {
            // Check if explicit recoil modification is present
            var hasExplicitRecoil = HasModification(StatModificationType.WeaponRecoil) || HasModification(StatModificationType.FlatWeaponRecoil);

            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    case StatModificationType.WeaponAoe:
                        ammunition.AreaOfEffect *= value;
                        break;
                    case StatModificationType.FlatWeaponAoe:
                        ammunition.AreaOfEffect += value;
                        break;
                    case StatModificationType.WeaponBulletMass:
                        ammunition.Impulse *= value;
                        if (!hasExplicitRecoil)
                            ammunition.Recoil *= value;
                        break;
                    case StatModificationType.FlatWeaponBulletMass:
                        ammunition.Impulse += value;
                        if (!hasExplicitRecoil)
                            ammunition.Recoil += value;
                        break;
                    case StatModificationType.WeaponRecoil:
                        ammunition.Recoil *= value;
                        break;
                    case StatModificationType.FlatWeaponRecoil:
                        ammunition.Recoil += value;
                        break;
                    case StatModificationType.WeaponBulletSpeed:
                        ammunition.Velocity *= value;
                        break;
                    case StatModificationType.FlatWeaponBulletSpeed:
                        ammunition.Velocity += value;
                        break;
                    case StatModificationType.WeaponDamage:
                        ammunition.Damage *= value;
                        break;
                    case StatModificationType.FlatWeaponDamage:
                        ammunition.Damage += value;
                        break;
                    case StatModificationType.WeaponFireRate:
                        weapon.FireRate *= value;
                        break;
                    case StatModificationType.FlatWeaponFireRate:
                        weapon.FireRate += value;
                        break;
                    case StatModificationType.WeaponRange:
                        ammunition.Range *= value;
                        break;
                    case StatModificationType.FlatWeaponRange:
                        ammunition.Range += value;
                        break;
                    case StatModificationType.WeaponEnergyCost:
                    case StatModificationType.EnergyCost:
                        if (ammunition.EnergyCost != 0)
                            ammunition.EnergyCost *= value;
                        break;
                    case StatModificationType.FlatWeaponEnergyCost:
                    case StatModificationType.FlatEnergyCost:
                        ammunition.EnergyCost += value;
                        break;
                }
            }
        }

        public void Apply(ref WeaponStatModifier statModifier)
        {
            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    case StatModificationType.WeaponAoe:
                        statModifier.AoeRadiusMultiplier *= value;
                        break;
                    case StatModificationType.WeaponBulletMass:
                        statModifier.WeightMultiplier *= value;
                        break;
                    case StatModificationType.WeaponBulletSpeed:
                        statModifier.VelocityMultiplier *= value;
                        break;
                    case StatModificationType.WeaponDamage:
                        statModifier.DamageMultiplier *= value;
                        break;
                    case StatModificationType.WeaponFireRate:
                        statModifier.FireRateMultiplier *= value;
                        break;
                    case StatModificationType.WeaponRange:
                        statModifier.RangeMultiplier *= value;
                        break;
                    case StatModificationType.WeaponEnergyCost:
                    case StatModificationType.EnergyCost:
                        statModifier.EnergyCostMultiplier *= value;
                        break;
                }
            }
        }

        public void Apply(ref DroneBayStats droneBay)
        {
            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    case StatModificationType.DroneAttack:
                        droneBay.DamageMultiplier += value - 1f;
                        break;
                    case StatModificationType.DroneDefense:
                        droneBay.DefenseMultiplier += value - 1f;
                        break;
                    case StatModificationType.DroneSpeed:
                        droneBay.SpeedMultiplier += value - 1.0f;
                        break;
                    case StatModificationType.DroneRange:
                        droneBay.Range *= value;
                        break;
                    case StatModificationType.FlatDroneRange:
                        droneBay.Range += value;
                        break;
                    case StatModificationType.EnergyCost:
                        if (droneBay.EnergyConsumption != 0)
                            droneBay.EnergyConsumption *= value;
                        break;
                    case StatModificationType.FlatEnergyCost:
                        droneBay.EnergyConsumption += value;
                        break;
                }
            }
        }

        // Helper to check if a specific modification type exists in the component mod
        private bool HasModification(StatModificationType type)
        {
            if (_modification.Modifications.Count == 0)
                return false;

            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                if (_modification.Modifications[i].Type == type)
                    return true;
            }
            return false;
        }

        private float GetModifier(StatModification mod, ModificationQuality quality)
        {
            switch (quality)
            {
                case ModificationQuality.N3: return mod.Gray3;
                case ModificationQuality.N2: return mod.Gray2;
                case ModificationQuality.N1: return mod.Gray1;
                case ModificationQuality.P1: return mod.Green;
                case ModificationQuality.P2: return mod.Purple;
                case ModificationQuality.P3: return mod.Gold;
                default:
                    return 0f;
            }
        }

        private ComponentModification(ComponentMod modification, ModificationQuality quality)
        {
            _modification = modification;
            _quality = quality;
        }
    }
}