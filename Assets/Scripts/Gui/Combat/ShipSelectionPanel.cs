using Combat.Component.Ship;
using Combat.Domain;
using Combat.Manager;
using Combat.Scene;
using Combat.Unit;
using Services.Gui;
using UnityEngine;
using Zenject;

namespace Gui.Combat
{
    public class ShipSelectionPanel : MonoBehaviour
    {
        [Inject] private readonly CombatManager _manager;
        [Inject] private readonly IScene _scene;

        // TODO: Inject CombatRules here if needed to fetch the actual scroll limit.
        // [Inject] private readonly CombatRules _combatRules;

        [SerializeField] private ShipList _enemyShips;
        [SerializeField] private ShipList _playerShips;

        public void Open(ICombatModel combatModel)
        {
            if (Window.IsVisible)
                return;

            int enemyActiveIndex = combatModel.EnemyFleet.Ships.FindIndex(item => item.Status == ShipStatus.Active);
            int playerActiveIndex = combatModel.PlayerFleet.Ships.FindIndex(item => item.Status == ShipStatus.Active);

            // Fetch limit setting from rules (using 0.1f as a placeholder).
            float scrollLimitSetting = 0.1f;
            // scrollLimitSetting = combatModel.Rules.EnemyShipScrollLimit; 

            // Calculate the maximum allowed index based on the limit.
            int enemyMaxScrollIndex = Mathf.RoundToInt(24 * scrollLimitSetting);

            // Apply limit for enemies; allow full access (up to 24) for the player.
            _enemyShips.Initialize(combatModel.EnemyFleet, enemyActiveIndex, enemyMaxScrollIndex);
            _playerShips.Initialize(combatModel.PlayerFleet, playerActiveIndex, 24);

            GetComponent<IWindow>().Open();
        }

        public void StartButtonClicked()
        {
            var ship = _playerShips.SelectedShip;
            if (ship.Status != ShipStatus.Ready)
                return;

            _manager.CreateShip(ship);
            GetComponent<IWindow>().Close();
        }

        private void Update()
        {
            if (_scene.PlayerShip.IsActive() && Window.IsVisible)
                Window.Close();
        }

        private IWindow Window { get { return GetComponent<IWindow>(); } }
    }
}