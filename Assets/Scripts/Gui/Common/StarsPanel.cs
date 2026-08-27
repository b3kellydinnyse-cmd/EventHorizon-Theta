using GameServices.Player;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CommonComponents;
using GameDatabase; // Added to access database settings

namespace Gui.Common
{
    public class StarsPanel : MonoBehaviour
    {
        [SerializeField] private Text _starsText;

        // Injected dependencies now include IDatabase to read EconomySettings
        [Inject]
        private void Initialize(IMessenger messenger, PlayerResources playerResources, IDatabase database)
        {
            // Read the master switch for premium currency
            bool enablePremiumCurrency = database.EconomySettings == null || database.EconomySettings.EnablePremiumCurrency;

            if (!enablePremiumCurrency)
            {
                // Completely hide the stars panel if premium currency is disabled globally
                gameObject.SetActive(false);
            }
            else
            {
                // Subscribe to balance changes and initialize current value
                gameObject.SetActive(true);
                messenger.AddListener<Money>(EventType.StarsValueChanged, SetValue);
                SetValue(playerResources.Stars);
            }
        }

        // Updates the UI text when the star balance changes
        private void SetValue(Money value)
        {
            if (_stars == value) return;
            _stars = value;
            _starsText.text = _stars.ToString();
        }

        private Money _stars = -1;
    }
}