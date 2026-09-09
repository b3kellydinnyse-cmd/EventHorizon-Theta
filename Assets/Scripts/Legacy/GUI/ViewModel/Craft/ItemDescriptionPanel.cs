using Constructor;
using Constructor.Extensions;
using Constructor.Model;
using Constructor.Ships;
using Economy.ItemType;
using Economy.Products;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Gui.Theme;
using Services.Localization;
using Services.ObjectPool;
using Services.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ViewModel.Craft
{
    public class ItemDescriptionPanel : MonoBehaviour
    {
        // Injected dependencies
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IGameObjectFactory _factory;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly IDatabase _database;

        // UI references
        [SerializeField] private Image _icon;
        [SerializeField] private Text _name;
        [SerializeField] private Text _description;
        [SerializeField] private Text _modification;
        [SerializeField] private LayoutGroup _stats;
        [SerializeField] private GameObject _weaponSlots; // Whole weapon slots container (with Label and Layout)
        [SerializeField] private Transform _slotsContainer; // Container holding slot badges (SlotsLayout)

        [SerializeField] private Sprite _emptyIcon;

        // Initialize panel based on product item type
        public void Initialize(IProduct item)
        {
            if (item == null)
            {
                CreateEmpty();
            }
            else if (item.Type is ComponentItem)
            {
                CreateComponent(((ComponentItem)item.Type).Component);
            }
            else if (item.Type is ShipItemBase)
            {
                CreateShip(((ShipItemBase)item.Type).Ship);
            }
            else if (item.Type is SatelliteItem)
            {
                CreateSatellite(((SatelliteItem)item.Type).Satellite);
            }
            else
            {
                CreateDefault(item.Type);
            }
        }

        // Setup ship view
        private void CreateShip(IShip ship)
        {
            _icon.sprite = _resourceLocator.GetSprite(ship.Model.ModelImage);
            _icon.color = Color.white;
            _name.text = _localization.GetString(ship.Name);
            _name.color = UiTheme.Current.GetQualityColor(ship.Model.Quality());

            if (!string.IsNullOrEmpty(ship.Model.OriginalShip.Description))
            {
                _description.gameObject.SetActive(true);
                _description.text = _localization.Localize(ship.Model.OriginalShip.Description);
            }
            else
                _description.gameObject.SetActive(false);

            _modification.gameObject.SetActive(ship.Model.Modifications.Any());
            _modification.text = string.Join("\n", ship.Model.Modifications.Select(item => item.GetDescription(_localization)).ToArray());

            _stats.gameObject.SetActive(true);
            _stats.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string, string>>(GetShipDescription(ship, _localization, _database), UpdateTextField);

            // Update weapon slots only for ships
            UpdateWeaponSlots(ship.Model.Barrels);
        }

        // Setup satellite view
        private void CreateSatellite(Satellite satellite)
        {
            _icon.sprite = _resourceLocator.GetSprite(satellite.ModelImage);
            _icon.color = Color.white;
            _name.text = _localization.GetString(satellite.Name);
            _name.color = UiTheme.Current.GetQualityColor(ItemQuality.Common);
            _description.gameObject.SetActive(false);
            _modification.gameObject.SetActive(false);

            _stats.gameObject.SetActive(true);
            _stats.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string, string>>(GetSatelliteDescription(satellite), UpdateTextField);

            // Hide weapon slots
            HideWeaponSlots();
        }

        // Setup component view
        private void CreateComponent(ComponentInfo info)
        {
            _icon.sprite = _resourceLocator.GetSprite(info.Data.Icon);
            _icon.color = info.Data.Color;
            _name.text = _localization.GetString(info.Data.Name);
            _name.color = UiTheme.Current.GetQualityColor(info.ItemQuality);

            if (!string.IsNullOrEmpty(info.Data.Description))
            {
                _description.gameObject.SetActive(true);
                _description.text = _localization.Localize(info.Data.Description);
            }
            else
                _description.gameObject.SetActive(false);

            var component = info.CreateComponent(100);

            var modification = component.Modification ?? Constructor.Modification.EmptyModification.Instance;
            _modification.gameObject.SetActive(!string.IsNullOrEmpty(_modification.text = modification.GetDescription(_localization)));
            _modification.color = UiTheme.Current.GetQualityColor(info.ItemQuality);

            _stats.gameObject.SetActive(true);
            _stats.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string, string>>(
                ShipEditor.UI.ComponentItem.GetDescription(component, _localization, _database.LocalizationSettings), UpdateTextField, _factory);

            // Render weapon slot badge and size if component is a weapon
            UpdateWeaponSlotsForComponent(info);
        }

        // Reset to empty state
        private void CreateEmpty()
        {
            _icon.sprite = _emptyIcon;
            _icon.color = Color.white;
            _name.text = string.Empty;
            _description.gameObject.SetActive(false);
            _stats.gameObject.SetActive(false);
            HideWeaponSlots();
            _modification.gameObject.SetActive(false);
        }

        // Setup generic default item view
        private void CreateDefault(IItemType item)
        {
            _icon.sprite = _resourceLocator.GetSprite(item.Icon);
            _icon.color = item.Color;
            _name.text = item.Name;
            _name.color = UiTheme.Current.GetQualityColor(item.Quality);

            var description = item.Description;
            _description.gameObject.SetActive(!string.IsNullOrEmpty(description));
            _description.text = item.Description;

            _stats.gameObject.SetActive(false);
            HideWeaponSlots();
            _modification.gameObject.SetActive(false);
        }

        // Completely hide the weapon slots container
        private void HideWeaponSlots()
        {
            if (_weaponSlots != null)
                _weaponSlots.SetActive(false);
        }

        // Render weapon slot type badge and size for weapon components
        private void UpdateWeaponSlotsForComponent(ComponentInfo info)
        {
            if (_weaponSlots == null) return;

            // Check if component requires a weapon slot
            bool isWeapon = info.Data.CellType == CellType.Weapon || info.Data.WeaponSlotType != default;

            if (!isWeapon)
            {
                HideWeaponSlots();
                return;
            }

            _weaponSlots.SetActive(true);

            Transform container = _slotsContainer != null ? _slotsContainer : _weaponSlots.transform;
            if (container.childCount < 2) return;

            GameObject badgeObj = container.GetChild(0).gameObject;
            GameObject countObj = container.GetChild(1).gameObject;

            badgeObj.SetActive(true);
            countObj.SetActive(true);

            // Display slot type letter (e.g. C, L, M, S)
            string slotLetter = info.Data.WeaponSlotType != default ? info.Data.WeaponSlotType.ToString() : "·";
            Text badgeText = badgeObj.GetComponentInChildren<Text>(true);
            if (badgeText != null)
                badgeText.text = slotLetter;

            // Display weapon cell count / size (e.g. ×1, ×9)
            int size = info.Data.Layout.CellCount;
            Text countText = countObj.GetComponentInChildren<Text>(true);
            if (countText != null)
                countText.text = $"×{size}";

            // Hide unused badges if previously displayed for a multi-slot ship
            for (int i = 2; i < container.childCount; i++)
            {
                container.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Update and render weapon slot badges for ships
        private void UpdateWeaponSlots(IReadOnlyCollection<Barrel> barrels)
        {
            if (_weaponSlots == null) return;

            if (barrels == null || barrels.Count == 0)
            {
                HideWeaponSlots();
                return;
            }

            _weaponSlots.SetActive(true);

            Transform container = _slotsContainer != null ? _slotsContainer : _weaponSlots.transform;

            // Count barrels by weapon class
            Dictionary<string, int> weaponCounts = new Dictionary<string, int>();
            foreach (var barrel in barrels)
            {
                string weaponClass = string.IsNullOrEmpty(barrel.WeaponClass) ? "·" : barrel.WeaponClass;

                if (weaponCounts.ContainsKey(weaponClass))
                    weaponCounts[weaponClass]++;
                else
                    weaponCounts[weaponClass] = 1;
            }

            if (container.childCount < 2) return;

            GameObject badgeTemplate = container.GetChild(0).gameObject;
            GameObject countTemplate = container.GetChild(1).gameObject;

            int slotIndex = 0;

            foreach (var kvp in weaponCounts)
            {
                int badgeIndex = slotIndex * 2;
                int countIndex = slotIndex * 2 + 1;

                GameObject badgeObj;
                GameObject countObj;

                if (countIndex < container.childCount)
                {
                    badgeObj = container.GetChild(badgeIndex).gameObject;
                    countObj = container.GetChild(countIndex).gameObject;
                }
                else
                {
                    badgeObj = Instantiate(badgeTemplate, container);
                    countObj = Instantiate(countTemplate, container);
                }

                badgeObj.SetActive(true);
                countObj.SetActive(true);

                Text badgeText = badgeObj.GetComponentInChildren<Text>(true);
                if (badgeText != null)
                    badgeText.text = kvp.Key;

                Text countText = countObj.GetComponentInChildren<Text>(true);
                bool isLast = slotIndex == weaponCounts.Count - 1;
                string space = isLast ? "" : " ";

                if (countText != null)
                {
                    countText.text = kvp.Value > 1 ? $"×{kvp.Value}{space}" : space;
                }
                slotIndex++;
            }

            // Hide unused elements
            for (int i = slotIndex * 2; i < container.childCount; i++)
            {
                container.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Update single key-value text row
        private void UpdateTextField(TextFieldViewModel viewModel, KeyValuePair<string, string> data)
        {
            viewModel.Label.text = _localization.GetString(data.Key);
            viewModel.Value.text = data.Value;
        }

        // Build ship stats description
        private static IEnumerable<KeyValuePair<string, string>> GetShipDescription(IShip ship, ILocalization localization, IDatabase database)
        {
            var size = ship.Model.Layout.CellCount;
            yield return new KeyValuePair<string, string>("$CellCount", size.ToString());

            var engineSize = CalculateCellSize(ship.Model.Layout, CellType.Engine);
            if (engineSize > 0)
                yield return new KeyValuePair<string, string>("$EngineSize", engineSize.ToString());

            var weaponSize = CalculateCellSize(ship.Model.Layout, CellType.Weapon);
            if (weaponSize > 0)
                yield return new KeyValuePair<string, string>("$WeaponSize", weaponSize.ToString());

            var innerSize = CalculateCellSize(ship.Model.Layout, CellType.Inner);
            if (innerSize > 0)
                yield return new KeyValuePair<string, string>("$InnerSize", innerSize.ToString());

            var data = ship.Model.OriginalShip;
            var kineticResistance = CalculateResistance(data.Features.KineticResistance);
            var heatResistance = CalculateResistance(data.Features.HeatResistance);
            var energyResistance = CalculateResistance(data.Features.EnergyResistance);
            var baseWeightBonus = data.Features.ShipWeightBonus;
            var equipmentWeightBonus = data.Features.EquipmentWeightBonus;
            var armorBonus = data.Features.ArmorBonus;
            var shieldBonus = data.Features.ShieldBonus;
            var energyBonus = data.Features.EnergyBonus;
            var velocityBonus = data.Features.VelocityBonus;
            var turnRateBonus = data.Features.TurnRateBonus;

            var droneBuildSpeedBonus = data.Features.DroneBuildSpeedBonus;
            var droneAttackBonus = data.Features.DroneAttackBonus;
            var droneDefenseBonus = data.Features.DroneDefenseBonus;
            var droneRangeBonus = data.Features.DroneRangeBonus;
            var droneSpeedBonus = data.Features.DroneSpeedBonus;

            if (kineticResistance != 0)
                yield return new KeyValuePair<string, string>("$KineticDamageResistance", kineticResistance + "%");
            if (heatResistance != 0)
                yield return new KeyValuePair<string, string>("$ThermalDamageResistance", heatResistance + "%");
            if (energyResistance != 0)
                yield return new KeyValuePair<string, string>("$EnergyDamageResistance", energyResistance + "%");
            if (data.Features.Regeneration)
            {
                float regenAmount = data.Features.RegenerationAmount != 0f ? data.Features.RegenerationAmount : 0.01f;
                yield return new KeyValuePair<string, string>("$RepairRate", localization.GetString("$ValuePerSecond", (regenAmount * 100f).ToString("0.##") + "%"));
            }
            if (baseWeightBonus != 0)
                yield return new KeyValuePair<string, string>("$Weight", SignedPercent(baseWeightBonus));
            if (equipmentWeightBonus != 0)
                yield return new KeyValuePair<string, string>("$EquipmentWeight", SignedPercent(equipmentWeightBonus));
            if (velocityBonus != 0)
                yield return new KeyValuePair<string, string>("$Velocity", SignedPercent(velocityBonus));
            if (turnRateBonus != 0)
                yield return new KeyValuePair<string, string>("$TurnRate", SignedPercent(turnRateBonus));
            if (armorBonus != 0)
                yield return new KeyValuePair<string, string>("$Armor", SignedPercent(armorBonus));
            if (shieldBonus != 0)
                yield return new KeyValuePair<string, string>("$Shield", SignedPercent(shieldBonus));
            if (energyBonus != 0)
                yield return new KeyValuePair<string, string>("$Energy", SignedPercent(energyBonus));

            if (droneBuildSpeedBonus != 0)
                yield return new KeyValuePair<string, string>("$DroneBuildSpeedBonus", SignedPercent(droneBuildSpeedBonus));
            if (droneAttackBonus != 0)
                yield return new KeyValuePair<string, string>("$DroneAttackBonus", SignedPercent(droneAttackBonus));
            if (droneDefenseBonus != 0)
                yield return new KeyValuePair<string, string>("$DroneDefenseBonus", SignedPercent(droneDefenseBonus));
            if (droneRangeBonus != 0)
                yield return new KeyValuePair<string, string>("$DroneRangeBonus", SignedPercent(droneRangeBonus));
            if (droneSpeedBonus != 0)
                yield return new KeyValuePair<string, string>("$DroneSpeedBonus", SignedPercent(droneSpeedBonus));
            if (data.Features.DronesBuiltPerSecondBonus != 0)
            {
                string sign = data.Features.DronesBuiltPerSecondBonus > 0 ? "+" : "";
                string formattedValue = sign + data.Features.DronesBuiltPerSecondBonus.ToString("0.##");
                yield return new KeyValuePair<string, string>("$DronesBuiltPerSecondBonus", localization.GetString("$ValuePerSecond", formattedValue));
            }
            if (data.Features.DroneCapacityBonus > 0)
                yield return new KeyValuePair<string, string>("$DroneCapacityBonus", "+" + data.Features.DroneCapacityBonus);

            foreach (var item in data.Features.BuiltinDevices)
            {
                yield return new KeyValuePair<string, string>("$Device", GetDeviceName(item, database, localization));
            }
        }

        // Build satellite stats description
        private static IEnumerable<KeyValuePair<string, string>> GetSatelliteDescription(Satellite satellite)
        {
            var size = satellite.Layout.CellCount;
            yield return new KeyValuePair<string, string>("$CellCount", size.ToString());

            var engineSize = satellite.Layout.Data.Count(value => value == (char)CellType.Engine);
            if (engineSize > 0)
                yield return new KeyValuePair<string, string>("$EngineSize", engineSize.ToString());

            var weaponSize = satellite.Layout.Data.Count(value => value == (char)CellType.Weapon);
            if (weaponSize > 0)
                yield return new KeyValuePair<string, string>("$WeaponSize", weaponSize.ToString());

            var innerSize = satellite.Layout.Data.Count(value => value == (char)CellType.Inner);
            if (innerSize > 0)
                yield return new KeyValuePair<string, string>("$InnerSize", innerSize.ToString());
        }

        // Calculate percentage damage resistance
        private static int CalculateResistance(float value)
        {
            return Mathf.FloorToInt(100 * value / (value + 1));
        }

        // Format signed percentage string
        private static string SignedPercent(float value)
        {
            var sb = new StringBuilder();
            var percent = value * 100f;

            if (percent > 0)
            {
                sb.Append('+');
                sb.Append(UnityEngine.Mathf.RoundToInt(percent));
            }
            else if (percent < 0)
            {
                sb.Append(percent.ToString("0.##"));
            }
            else
            {
                sb.Append("0");
            }

            sb.Append('%');
            return sb.ToString();
        }

        // Get localized device name from database component, device ID, or device class
        private static string GetDeviceName(Device device, IDatabase database, ILocalization localization)
        {
            if (device == null)
                return string.Empty;

            // 1. Try to find the component in database that owns this device and use its localized name
            if (database != null)
            {
                try
                {
                    var component = database.ComponentList?.FirstOrDefault(c => c.Device == device);
                    if (component != null && !string.IsNullOrEmpty(component.Name))
                    {
                        string componentName = localization.GetString(component.Name);
                        if (!string.IsNullOrEmpty(componentName) && !componentName.StartsWith("$"))
                            return componentName;
                    }
                }
                catch { }
            }

            // 2. Try direct device ID localization (e.g. "$Device_1141" or "$1141")
            string directIdKey = "$" + device.Id;
            string directIdName = localization.GetString(directIdKey);
            if (!string.IsNullOrEmpty(directIdName) && !directIdName.StartsWith("$"))
                return directIdName;

            // 3. Fallback to classic built-in device mapping
            switch (device.Stats.DeviceClass)
            {
                case DeviceClass.TimeMachine:
                    return localization.GetString("$InfinityStone_S");
                case DeviceClass.ToxicWaste:
                    return localization.GetString("$ToxicWaste");
                case DeviceClass.WormTail:
                    return localization.GetString("$Wormtail");
                case DeviceClass.DroneCamouflage:
                    return localization.GetString("$DroneCamouflage");
                case DeviceClass.MissileCamouflage:
                    return localization.GetString("$MissileCamouflage");
                case DeviceClass.RepairBot:
                    return localization.GetString("$RepairBot_M");
                default:
                    // 4. Try generic class localization key (e.g. "$Accelerator", "$Teleporter")
                    string classKey = "$" + device.Stats.DeviceClass;
                    string localizedClass = localization.GetString(classKey);
                    return !string.IsNullOrEmpty(localizedClass) && !localizedClass.StartsWith("$")
                        ? localizedClass
                        : device.Stats.DeviceClass.ToString();
            }
        }

        // Count cell occurrences in layout
        private static int CalculateCellSize(IShipLayout layout, CellType targetCellType)
        {
            int count = 0;

            for (int i = layout.Rect.yMin; i <= layout.Rect.yMax; ++i)
                for (int j = layout.Rect.xMin; j <= layout.Rect.xMax; ++j)
                    if (layout[j, i] == targetCellType)
                        count++;

            return count;
        }
    }
}