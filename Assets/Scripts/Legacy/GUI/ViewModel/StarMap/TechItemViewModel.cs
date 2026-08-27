using System;
using DataModel.Technology;
using GameDatabase.DataModel;
using GameServices.Research;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Services.Localization;
using Services.Resources;
using Zenject;
using Gui.Theme;

namespace ViewModel
{
    public class TechItemViewModel : MonoBehaviour
    {
        // Injected services
        [Inject] private readonly Research _research;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IResourceLocator _resourceLocator;

        // UI references
        [SerializeField] private Toggle Toggle;
        [SerializeField] private Image Icon;
        [SerializeField] private Text Name;
        [SerializeField] private Text Description;
        [SerializeField] private Text PriceText;
        [SerializeField] private Graphic[] UiElements;
        [SerializeField] private Sprite HiddenIcon;

        // Events
        public TechEvent OnTechSelectedEvent = new TechEvent();
        public TechEvent OnTechDeselectedEvent = new TechEvent();

        // Theme colors
        private Color ResearchedColor => UiTheme.Current.GetTechColor(TechColor.Obtained);
        private Color AvailableColor => UiTheme.Current.GetTechColor(TechColor.Available);
        private Color NotAvailableColor => UiTheme.Current.GetTechColor(TechColor.NotAvailable);
        private Color HiddenColor => UiTheme.Current.GetTechColor(TechColor.Hidden);

        [Serializable]
        public class TechEvent : UnityEvent<ITechnology>
        {
        }

        // Initialize UI item with technology data
        public void Initialize(ITechnology technology)
        {
            _technology = technology;
            var researched = _research.IsTechResearched(technology);
            var available = _research.IsTechAvailable(technology) && !technology.Hidden;
            var hidden = technology.Hidden && !researched;

            // Apply state-based colors
            var color = researched ? ResearchedColor : hidden ? HiddenColor : available ? AvailableColor : NotAvailableColor;
            foreach (var element in UiElements)
                element.color = color;

            Icon.sprite = hidden ? HiddenIcon : technology.GetImage(_resourceLocator);

            if (!available && !researched)
            {
                // Locked / undiscovered state
                Name.text = "?????";
                Description.text = string.Empty;
                Icon.color = Color.black;
                Toggle.interactable = false;
            }
            else
            {
                // Available or unlocked state
                Name.text = _localization.GetString(technology.GetName(_localization));

                // Append ship class/size to description if applicable
                string descriptionText = _localization.GetString(technology.GetDescription(_localization));
                string shipSizeText = GetShipSizeString(technology);

                if (!string.IsNullOrEmpty(shipSizeText))
                {
                    if (!string.IsNullOrEmpty(descriptionText))
                        descriptionText += "\n";
                    descriptionText += shipSizeText;
                }

                Description.text = descriptionText;
                Icon.color = technology.Color;
                Toggle.interactable = true;
            }

            PriceText.text = researched || hidden ? string.Empty : technology.Price.ToString();
        }

        // Retrieve localized ship size class using '$' key prefix (e.g. $Battleship)
        private string GetShipSizeString(ITechnology technology)
        {
            if (technology?.Data is Technology_Ship shipTech && shipTech.Ship != null)
            {
                return _localization.GetString($"$Class{shipTech.Ship.SizeClass}");
            }

            return string.Empty;
        }

        // Handle toggle selection changes
        public void OnToggleValueChanged(bool value)
        {
            if (_technology == null) return;

            if (value)
                OnTechSelectedEvent.Invoke(_technology);
            else
                OnTechDeselectedEvent.Invoke(_technology);
        }

        private ITechnology _technology;
    }
}