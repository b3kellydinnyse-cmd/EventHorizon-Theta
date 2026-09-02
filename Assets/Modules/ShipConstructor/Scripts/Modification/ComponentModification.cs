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
                    case StatModificationType.ExtraHitPoints:
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

                    // Core energy, shield and armor stats
                    case StatModificationType.EnergyCapacity:
                        // Allow scaling non-zero energy capacity
                        if (stats.EnergyPoints != 0)
                            stats.EnergyPoints *= value;
                        break;
                    case StatModificationType.EnergyRechargeRate:
                        stats.EnergyRecharge *= value;
                        break;
                    case StatModificationType.ShieldPoints:
                        // Allow scaling non-zero shield points
                        if (stats.ShieldPoints != 0)
                            stats.ShieldPoints *= value;
                        break;
                    case StatModificationType.ShieldRechargeRate:
                        // Allow scaling non-zero shield recharge rate
                        if (stats.ShieldRechargeRate != 0)
                            stats.ShieldRechargeRate *= value;
                        break;
                    case StatModificationType.ShieldRechargeCooldown:
                        // Shield recharge delay modifier
                        if (stats.ShieldRechargeCooldownMultiplier.HasValue && value > 0)
                            stats.ShieldRechargeCooldownMultiplier = stats.ShieldRechargeCooldownMultiplier.AmplifyDelta(value);
                        break;
                    case StatModificationType.ArmorPoints:
                        // Allow scaling non-zero armor points
                        if (stats.ArmorPoints != 0)
                            stats.ArmorPoints *= value;
                        break;
                    case StatModificationType.ArmorRepairRate:
                        stats.ArmorRepairRate *= value;
                        break;

                    // Resistances, absorption and ramming stats
                    case StatModificationType.Resistance:
                        // Allow scaling non-zero resistances
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

                    // Engine mechanics
                    case StatModificationType.EnginePower:
                        // Allow scaling non-zero engine power
                        if (stats.EnginePower != 0)
                            stats.EnginePower *= value;
                        if (stats.EnginePowerWithoutEnergy != 0)
                            stats.EnginePowerWithoutEnergy *= value;
                        break;
                    case StatModificationType.EngineTurnRate:
                        // Allow scaling non-zero turn rate
                        if (stats.TurnRate != 0)
                            stats.TurnRate *= value;
                        if (stats.TurnRateWithoutEnergy != 0)
                            stats.TurnRateWithoutEnergy *= value;
                        break;

                    // Mass power
                    case StatModificationType.Mass:
                        if (stats.Weight != 0)
                            stats.Weight *= value;
                        break;

                    // Inertia compensator power
                    case StatModificationType.WeightReduction:
                        if (stats.WeightReduction != 0)
                            stats.WeightReduction *= value;
                        break;

                    case StatModificationType.EnergyCost:
                        stats.EnergyConsumption *= value;
                        break;
                    case StatModificationType.ExtraHitPoints:
                        stats.ArmorPoints += value;
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
                    case StatModificationType.DeviceRange:
                        device.Range *= value;
                        break;
                    case StatModificationType.DevicePower:
                        device.Power *= value;
                        break;
                    case StatModificationType.EnergyCost:
                        // Allow scaling non-zero device energy consumption
                        if (device.EnergyConsumption != 0)
                            device.EnergyConsumption *= value;
                        break;
                }
            }
        }

        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition)
        {
            for (int i = 0; i < _modification.Modifications.Count; ++i)
            {
                var mod = _modification.Modifications[i];
                var value = GetModifier(mod, _quality);

                switch (mod.Type)
                {
                    case StatModificationType.WeaponAoe:
                        ammunition.AreaOfEffect *= value;
                        break;
                    case StatModificationType.WeaponBulletMass:
                        ammunition.Impulse *= value;
                        ammunition.Recoil *= value;
                        break;
                    case StatModificationType.WeaponRecoil:
                        // Specific recoil reduction modifier
                        ammunition.Recoil *= value;
                        break;
                    case StatModificationType.WeaponBulletSpeed:
                        ammunition.Velocity *= value;
                        break;
                    case StatModificationType.WeaponDamage:
                        ammunition.Damage *= value;
                        break;
                    case StatModificationType.WeaponFireRate:
                        weapon.FireRate *= value;
                        break;
                    case StatModificationType.WeaponRange:
                        ammunition.Range *= value;
                        break;
                    case StatModificationType.WeaponEnergyCost:
                    case StatModificationType.EnergyCost:
                        // Allow scaling non-zero ammunition energy cost
                        if (ammunition.EnergyCost != 0)
                            ammunition.EnergyCost *= value;
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
                    case StatModificationType.EnergyCost:
                        // Allow scaling non-zero drone bay energy consumption
                        if (droneBay.EnergyConsumption != 0)
                            droneBay.EnergyConsumption *= value;
                        break;
                }
            }
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