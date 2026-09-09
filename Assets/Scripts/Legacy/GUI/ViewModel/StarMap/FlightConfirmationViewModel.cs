using Galaxy;
using GameServices.Player;
using GameStateMachine.States;
using Gui.Theme;
using Gui.Windows;
using Services.Gui;
using Services.Localization;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ViewModel
{
    public class FlightConfirmationViewModel : MonoBehaviour
    {
        [Inject] private readonly PlayerResources _playerResources;
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly StartTravelSignal.Trigger _travelTrigger;
        [Inject] private readonly IMessenger _messenger;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly StarMap _starMap;

        [Header("Fuel & Actions")]
        public Text FuelText;
        public Image FuelIcon;
        public Button ConfirmButton;

        [Header("Target Star (Individual fields)")]
        public Text TargetStarNameText;
        public Text TargetFactionText;
        public Text TargetDefenseText;

        [Header("Target Star (Combined field)")]
        public Text TargetInfoText;

        [SerializeField] private ThemeColor _normalColor = ThemeColor.Text;
        [SerializeField] private ThemeColor _notEnoughColor = ThemeColor.ErrorText;

        private Color NormalColor => UiTheme.Current.GetColor(_normalColor);
        private Color NotEnoughColor => UiTheme.Current.GetColor(_notEnoughColor);

        public void Initialize(WindowArgs args)
        {
            var starId = args.Get<int>(0);

            var required = _motherShip.CalculateRequiredFuel(_motherShip.CurrentStar.Id, starId);
            var haveFuel = _playerResources.Fuel >= required;

            FuelText.text = required.ToString();
            FuelText.color = haveFuel ? NormalColor : NotEnoughColor;
            FuelIcon.color = haveFuel ? NormalColor : NotEnoughColor;

            UpdateTargetStarInfo(starId);

            _messenger.Broadcast<int>(EventType.FocusedPositionChanged, starId);
            ConfirmButton.interactable = haveFuel;
        }

        public void OnWindowClosed()
        {
            _messenger.Broadcast<int>(EventType.FocusedPositionChanged, -1);
        }

        public void ConfirmButtonClicked()
        {
            GetComponent<AnimatedWindow>().Close(WindowExitCode.Ok);
        }

        private void UpdateTargetStarInfo(int starId)
        {
            // Retrieve destination star data.
            var targetStar = _starMap.GetStarById(starId);

            var starName = string.IsNullOrEmpty(targetStar.Bookmark) ? targetStar.Name : targetStar.Bookmark;
            var factionName = _localization.GetString(targetStar.Region.Faction.Name);
            var header = $"{starName}\n{factionName}";

            // Multi-line layout matching the star map format.
            if (TargetInfoText != null)
            {
                if (targetStar.Region.IsCaptured)
                {
                    TargetInfoText.color = new Color(0.5f, 1f, 1f);
                    TargetInfoText.text = _localization.GetString("$CapturedStarInfo", header, Mathf.Max(targetStar.Level, 5));
                }
                else
                {
                    TargetInfoText.color = new Color(1f, 0.75f, 0.5f);
                    TargetInfoText.text = _localization.GetString("$StarInfo", header, targetStar.Region.BaseDefensePower + "%");
                }
            }

            // Populate individual labels if assigned in the inspector.
            if (TargetStarNameText != null)
            {
                TargetStarNameText.text = starName;
            }

            if (TargetFactionText != null)
            {
                TargetFactionText.text = factionName;
                TargetFactionText.color = targetStar.Region.Faction.Color;
            }

            if (TargetDefenseText != null)
            {
                if (targetStar.Region.IsCaptured)
                {
                    TargetDefenseText.color = new Color(0.5f, 1f, 1f);
                    TargetDefenseText.text = _localization.GetString("$CapturedStarInfo", "", Mathf.Max(targetStar.Level, 5)).Trim();
                }
                else
                {
                    TargetDefenseText.color = new Color(1f, 0.75f, 0.5f);
                    TargetDefenseText.text = _localization.GetString("$StarInfo", "", targetStar.Region.BaseDefensePower + "%").Trim();
                }
            }
        }
    }
}