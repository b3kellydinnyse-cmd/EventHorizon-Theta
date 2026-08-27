using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameServices.Settings;
using Services.Audio;
using GameDatabase;
using Zenject;

namespace ViewModel
{
    public class DevConsoleViewModel : MonoBehaviour
    {
        // Injected services
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly Cheats _cheats;
        [Inject] private readonly IDatabase _database;

        // UI references
        public RectTransform[] Buttons;
        public AudioClip ClickSound;
        public AudioClip ConfirmSound;
        public Text DeviceId;

        [Inject]
        private void Initialize(GameSettings gameSettings)
        {
            _hash = Security.DebugCommands.GetHashCode(SystemInfo.deviceUniqueIdentifier);
        }

        public void OnButtonClicked(string key)
        {
            _idletime = 0;

            if (string.IsNullOrEmpty(key))
            {
                if (TryExecuteCommand(_value))
                    DeviceId.text = "OK";
                else
                    DeviceId.text = "ERROR";

                _value = string.Empty;
                return;
            }

            if (key == "*")
            {
                _value = string.Empty;
            }
            else
            {
                _value += key;
            }

            DeviceId.text = _value.Length <= 8 ? _value : _value.Substring(_value.Length - 8);
            _soundPlayer.Play(ClickSound);
        }

        // Open dev keypad on triple click
        public void OnPointerClick(BaseEventData data)
        {
            if (Time.unscaledTime - _lastClickTime > 0.4f)
            {
                _clickCount = 1;
            }
            else if (++_clickCount == 3)
            {
                _idletime = 0;
                foreach (var item in Buttons)
                    item.gameObject.SetActive(true);
                DeviceId.gameObject.SetActive(true);
                DeviceId.text = _hash.ToString();
            }

            _lastClickTime = Time.unscaledTime;
        }

        // Check if dev console is enabled from database settings
        private void OnEnable()
        {
            bool isEnabled = _database.DebugSettings == null || _database.DebugSettings.EnableDevCodesConsole;

            if (!isEnabled)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            _value = string.Empty;
            foreach (var item in Buttons)
                item.gameObject.SetActive(false);
            DeviceId.gameObject.SetActive(false);
        }

        private bool TryExecuteCommand(string command)
        {
            if (_cheats.TryExecuteCommand(command, _hash))
            {
                _soundPlayer.Play(ConfirmSound);
                return true;
            }

            return false;
        }

        // Auto-hide keypad after 5 seconds of inactivity
        private void Update()
        {
            _idletime += Time.deltaTime;
            if (_idletime > 5f)
            {
                foreach (var item in Buttons)
                    item.gameObject.SetActive(false);
                DeviceId.gameObject.SetActive(false);
            }
        }

        private float _idletime;
        private float _lastClickTime;
        private int _clickCount;
        private int _hash;
        private string _value = string.Empty;
    }
}