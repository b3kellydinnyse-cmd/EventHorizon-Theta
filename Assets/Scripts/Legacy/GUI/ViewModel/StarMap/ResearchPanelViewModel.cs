using System.Linq;
using Domain.Player;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Extensions;
using GameServices.Research;
using UnityEngine;
using UnityEngine.UI;
using Services.Messenger;
using Services.ObjectPool;
using Zenject;

namespace ViewModel
{
    public class ResearchPanelViewModel : MonoBehaviour
    {
        // Injected dependencies
        [Inject] private readonly IMessenger _messenger;
        [Inject] private readonly IGameObjectFactory _factory;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly Research _research;
        [Inject] private readonly StarMapManager _starMapManager;

        public TechTreePanelViewModel TechTree;
        public LayoutGroup FactionsLayout;
        public ToggleGroup FactionsGroup;

        public void OnFactionSelected(Faction faction)
        {
            TechTree.Initialize(faction);
        }

        private void Start()
        {
            _messenger.AddListener(EventType.TechResearched, UpdateFactions);
        }

        private void OnEnable()
        {
            UpdateFactions();

            // Select first available faction toggle
            var firstItem = FactionsLayout.transform
                .Cast<Transform>()
                .Where(child => child.gameObject.activeSelf)
                .Select(child => child.GetComponent<FactionViewModel>())
                .FirstOrDefault(item => item != null);

            if (firstItem != null)
                firstItem.Toggle.isOn = true;
        }

        // Rebuild faction tabs list based on settings filter
        private void UpdateFactions()
        {
            if (!gameObject.activeSelf) return;

            var visibleFactions = _database.FactionsWithEmpty
                .WithTechTree()
                .Where(IsFactionVisible);

            FactionsLayout.transform.InitializeElements<FactionViewModel, Faction>(
                visibleFactions,
                UpdateFaction,
                _factory
            );
        }

        // Check if faction should be displayed in the tech tree
        private bool IsFactionVisible(Faction faction)
        {
            // Always show Common Stars / Neutral tab
            if (faction == Faction.Empty || faction.Id.IsNull)
                return true;

            // If hiding is disabled in database settings, show all factions (with ???)
            bool shouldHide = _database.FactionsSettings == null || _database.FactionsSettings.HideUndiscoveredFactionsInTechTree;
            if (!shouldHide)
                return true;

            // Show only if discovered or has research points
            return _starMapManager.IsFactionDiscovered(faction) || _research.AnyResearchPointsObtained(faction);
        }

        private void UpdateFaction(FactionViewModel item, Faction faction)
        {
            item.SetFaction(faction);
            item.Toggle.isOn = false;
        }
    }
}