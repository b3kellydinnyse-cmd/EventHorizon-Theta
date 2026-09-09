using Combat.Manager;
using Services.Gui;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.Combat
{
    public class CombatMenu : MonoBehaviour
    {
        [SerializeField] private Button _nextEnemyButton;
        [SerializeField] private Button _call10EnemiesButton;
        [SerializeField] private Button _callAllEnemiesButton;
        [SerializeField] private Button _changeShipButton;
        [SerializeField] private Button _killAllButton;

        [Inject]
        private void Initialize(IMessenger messenger, CombatManager manager)
        {
            _manager = manager;
            messenger.AddListener<int>(EventType.EnemyShipCountChanged, OnEnemyShipCountChanged);
        }

        public void Open()
        {
            GetComponent<IWindow>().Open();
        }

        public void InitializeWindow()
        {
            bool canCall = _manager.CanCallNextEnemy();

            _nextEnemyButton.gameObject.SetActive(canCall);
            _nextEnemyButton.interactable = canCall;

            if (_call10EnemiesButton != null)
            {
                _call10EnemiesButton.gameObject.SetActive(canCall);
                _call10EnemiesButton.interactable = canCall;
            }

            if (_callAllEnemiesButton != null)
            {
                _callAllEnemiesButton.gameObject.SetActive(canCall);
                _callAllEnemiesButton.interactable = canCall;
            }

            _changeShipButton.gameObject.SetActive(_manager.CanChangeShip());
#if !UNITY_EDITOR
            _killAllButton.gameObject.SetActive(_manager.CanKillAllEnemies);
#endif
        }

        public void ExitButtonClicked()
        {
            _manager.Surrender();
        }

        // Maintained original method name with typo for backward compatibility with existing Inspector bindings
        public void NextEnemyButtonClicled()
        {
            _manager.CallNextEnemy();
        }

        public void NextEnemyButtonClicked()
        {
            _manager.CallNextEnemy();
        }

        public void Call10EnemiesButtonClicked()
        {
            _manager.CallEnemies(10);
        }

        public void CallAllEnemiesButtonClicked()
        {
            _manager.CallAllEnemies();
        }

        public void ChangeShipButtonClicked()
        {
            _manager.ChangeShip();
        }

        public void KillThemAll()
        {
            _manager.KillAllEnemies();
        }

        private void OnEnemyShipCountChanged(int count)
        {
            if (!gameObject.activeSelf)
                return;

            bool canCall = _manager.CanCallNextEnemy();
            _nextEnemyButton.interactable = canCall;

            if (_call10EnemiesButton != null)
                _call10EnemiesButton.interactable = canCall;

            if (_callAllEnemiesButton != null)
                _callAllEnemiesButton.interactable = canCall;
        }

        private CombatManager _manager;
    }
}