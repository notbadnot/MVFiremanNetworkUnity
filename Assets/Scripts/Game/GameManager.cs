using Game.Configs;
using UnityEngine;
using View;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private GameView _view;
        [SerializeField] private GameplayView _gameplayView;

        [Header("Managers")]
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private BonusManager _bonusManager; //new
        
        [Header("Configs")]
        [SerializeField] private GameConfig _config;
        [SerializeField] private NetworkEvents _networkEvents;

        private BulletManager _bulletManager;
        private PlayersManager _playersManager;

       

        public void OnEnable()
        {
            _view.CreateGameClickEvent += OnCreateGameClick;
            _view.FindRandomGameEvent += OnFindRandomGame;
            _view.StartGameEvent += OnStartGame;
            _networkManager.RoomJoinEvent += OnRoomJoin;
            _networkManager.ConnectedEvent += OnConnected;
            _networkManager.ModelChangedEvent += OnModelChanged;

            _bulletManager = new BulletManager(_config);
            _playersManager = new PlayersManager(_networkManager, _bulletManager, _networkEvents, _config, _bonusManager /*new*/, _gameplayView);

            _view.SetLoadingState(true);
            _view.SetSettingsState(false);
            _view.SetError(string.Empty);
            _networkManager.Connect();
        }

        private void OnStartGame()
        {
            _playersManager.SetRandomFireman();
            _networkManager.StartGame();
        }

        private void Update()
        {
            _bulletManager.Tick(Time.deltaTime);
            _playersManager.Tick(Time.deltaTime);
        }

        private void OnModelChanged()
        {
            switch (_networkManager.GameState)
            {
                case GameState.Play:
                    _view.SetStartState(false, false);
                    break;
                
                case GameState.End:
                    _view.SetWinState(true, !_playersManager.IsFireman);
                    break;
            }
        }
        
        private void OnConnected()
        {
            _view.SetLoadingState(false);
            _view.SetSettingsState(true);
        }

        private void OnRoomJoin(bool ok, string error)
        {
            _view.SetLoadingState(false);
            _view.SetSettingsState(false);
            if (ok)
            {
                _playersManager.CreateLocalPlayer();
                _view.SetStartState(true, _networkManager.IsMaster);
            }
            else
            {
                _view.SetSettingsState(true);
                _view.SetError(error);
            }
        }

        public void OnDisable()
        {
            _view.CreateGameClickEvent -= OnCreateGameClick;
            _view.FindRandomGameEvent -= OnFindRandomGame;
            _networkManager.RoomJoinEvent -= OnRoomJoin;
            _networkManager.ConnectedEvent -= OnConnected;
            _playersManager.Release();
        }

        private void OnCreateGameClick()
        {
            SaveSettings();
            _view.SetLoadingState(true);
            _view.SetError(string.Empty);
            _networkManager.CreateGame(_view.RoomName);
        }

        private void OnFindRandomGame()
        {
            SaveSettings();
            _view.SetLoadingState(true);
            _view.SetError(string.Empty);
            _networkManager.FindRandomRoom();
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetString("PlayerName", _view.PlayerName);
        }
    }
}