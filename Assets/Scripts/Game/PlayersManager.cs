using System;
using System.Collections.Generic;
using Game.Configs;
using UnityEngine;
using View;
using View.Components;
using Random = UnityEngine.Random;

namespace Game
{
    public class PlayersManager
    {
        private readonly NetworkManager _networkManager;
        private readonly BulletManager _bulletManager;
        private readonly NetworkEvents _networkEvents;
        private readonly GameConfig _config;
        private readonly GameplayView _gameplayView;

        private readonly BonusManager _bonusManager; //new

        private float _health;
        private float _fireTimer;
        private PlayerController _playerController;
        private readonly List<PlayerController> _players = new List<PlayerController>();

        public bool IsFireman => _playerController != null && _playerController.Id == _networkManager.FiremanId;

        public PlayersManager(
            NetworkManager networkManager, 
            BulletManager bulletManager,
            NetworkEvents networkEvents, 
            GameConfig config, 
            BonusManager bonusManager, // new
            GameplayView gameplayView)
        {
            _networkManager = networkManager;
            _networkEvents = networkEvents;
            _config = config;
            _gameplayView = gameplayView;
            _bulletManager = bulletManager;

            _bonusManager = bonusManager; // new
            _bonusManager.playersManager = this; // new

            _health = _config.PlayerHelth;

            _networkManager.ModelChangedEvent += OnModelChanged;
            _networkEvents.PlayerControllerCreatedEvent += AddPlayer;
            _bulletManager.OnTargetReachedEvent += OnTargetReached;
        }

        public void Release()
        {
            _networkManager.ModelChangedEvent -= OnModelChanged;
            _networkEvents.PlayerControllerCreatedEvent -= AddPlayer;
            _bulletManager.OnTargetReachedEvent -= OnTargetReached;
        }
        
        public void CreateLocalPlayer()
        {
            var points = _gameplayView.SpawnPoints;
            var spawnPoint = points[Random.Range(0, points.Length)];
            
            _playerController = _networkManager.CreatePlayer(_config.PlayerPrefab.Path, spawnPoint.position, spawnPoint.rotation);
            _playerController.ShootEvent += OnShoot;


            //my code 

            _playerController.gameObject.GetComponent<BonusAcceptor>().ActivateBonusEvent += PlayersManager_ActivateBonusEvent; 

            //my code 
            
            _gameplayView.SetLocalPlayer(_playerController);
            _gameplayView.AddPlayer(_playerController);
            _playerController.HitpointsView.SetValue(1);
        }



        public void SetRandomFireman()
        {
            var idx = Random.Range(0, _players.Count);
            _networkManager.SetFireman(_players[idx].Id);
        }

        public void Tick(float deltaTime)
        {
            _fireTimer -= Time.deltaTime;
            
            if (!IsFireman || _health <= 0)
                return;

            _health -= deltaTime;
            _playerController.HitpointsView.SetValue(Math.Max(_health / _config.PlayerHelth, 0));

            if (_health <= 0)
            {
                _networkManager.EndGame();
            }
        }
        
        private void OnShoot(Vector3 point, Quaternion rotation)
        {
            if (_fireTimer > 0 || !IsFireman || _health <= 0 || _networkManager.GameState != GameState.Play)
                return;
            
            _bulletManager.CreateBullet(point, rotation);
            _fireTimer = _config.FirePeriod;
        }
        
        private void OnTargetReached(IBulletTarget target)
        {
            if (target is ZombieComponent zombie)
            {
                zombie.SetState(false);
            }
            else if (target is PlayerController player)
            {
                _networkManager.SetFireman(player.Id);
            }
        }
        
        private void OnModelChanged()
        {
            var id = _networkManager.FiremanId;
            foreach (var player in _players)
            {
                player.IsFireman = player.Id == id;
            }
        }
        
        private void AddPlayer(PlayerController player)
        {
            if (_players.Contains(player))
                return;
            
            _players.Add(player);
            _gameplayView.AddPlayer(player);
            player.IsFireman = player.Id == _networkManager.FiremanId;
            player.SetLayer(player.Id == _playerController.Id ? _config.CurrentPlayerLayer : _config.RemotePlayerLayer);
        }

        private void PlayersManager_ActivateBonusEvent(BonusModel.Effect arg1, BonusModel.Targets arg2, int idOfBonusUser)
        {
            /*var targets = FindTargetsOfEffect(arg2, idOfBonusUser);
            
            _bonusManager.ApplyEffectOnTargets(arg1, targets);*/
            _bonusManager.ApplyEffectOfBonus(arg1, arg2, idOfBonusUser);
        }

        public void UseEffectOnTargets(BonusModel.Effect effect, BonusModel.Targets targets, int UserId)
        {
            var targetsList = FindTargetsOfEffect(targets, UserId);
            ApplyEffectForTargets(effect, targetsList);
        }


        private List<BonusAcceptor> FindTargetsOfEffect (BonusModel.Targets targets, int userID)
        {
            List<BonusAcceptor> listOfAcceptors  = new List<BonusAcceptor>();
            if (targets == BonusModel.Targets.Picker)
            {
                foreach (var player in _players)
                {
                    if (player.Id == userID)
                    {
                        listOfAcceptors.Add(player.gameObject.GetComponent<BonusAcceptor>());
                    }
                }

            }
            else if (targets == BonusModel.Targets.EveryoneExpectPicker)
            {
                foreach (var player in _players)
                {

                    if (player.Id != userID)
                    {
                        listOfAcceptors.Add(player.gameObject.GetComponent<BonusAcceptor>());
                    }

                }
            }
            else if (targets == BonusModel.Targets.Everyone)
            {
                foreach (var player in _players)
                {
                    listOfAcceptors.Add(player.gameObject.GetComponent<BonusAcceptor>());
                }
            }
            else if (targets == BonusModel.Targets.Random)
            {
                var idx = Random.Range(0, _players.Count);
                listOfAcceptors.Add(_players[idx].gameObject.GetComponent<BonusAcceptor>());
            }
            else { listOfAcceptors = null; }

            return listOfAcceptors;
        }
        private void ApplyEffectForTargets(BonusModel.Effect effect, List<BonusAcceptor> targets)
        {
            if (effect == BonusModel.Effect.Acceleration)
            {
                foreach (var target in targets)
                {
                    target.accelerationApplied = true;
                }
            }else if (effect == BonusModel.Effect.Freezer)
            {
                foreach (var target in targets)
                {
                    target.freezerApplied = true;
                }
            }
        }
        
    }
}