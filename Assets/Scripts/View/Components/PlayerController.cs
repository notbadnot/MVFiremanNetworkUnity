using System;
using Photon.Pun;
using UnityEngine;
using View.Input;

namespace View.Components
{
    public class PlayerController : MonoBehaviour, IBulletTarget
    {
        public event Action<Vector3, Quaternion> ShootEvent;

        [SerializeField] private PhotonView _photonView;
        
        public Rigidbody Rigidbody;
        public Renderer BodyRenderer;
        public Transform BulletSpawnPoint;

        [Header("Views")]
        public HitpointsView HitpointsView;

        [Header("Gameplay")] 
        public Material NormalBodyMaterial;
        public Material FiremanBodyMaterial;
        public float Speed = 5f;

        [Header("Network")] 
        [SerializeField] private NetworkEvents _events;

        private IPlayerInput _playerInput;

        public int Id => _photonView.ViewID;
        
        public bool IsFireman
        {
            set => BodyRenderer.material = value ? FiremanBodyMaterial : NormalBodyMaterial;
        }

        public void SetInput(IPlayerInput playerInput)
        {
            _playerInput = playerInput;
        }

        public void SetLayer(int layerMask)
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach(var c in colliders)
            {
                c.gameObject.layer = layerMask;
            }
        }

        public void Start()
        {
            _events.RaisePlayerControllerCreated(this);
        }

        private void OnEnable()
        {
            IsFireman = true;
            HitpointsView.SetPlayerName(_photonView.Controller.CustomProperties["PlayerName"].ToString());
        }

        private void Update()
        {
            if (_playerInput == null)
                return;

            var (moveDirection, viewDirection, shoot) = _playerInput.CurrentInput();
            ProcessShoot(shoot);
            Rigidbody.velocity = moveDirection.normalized * Speed;
            transform.rotation = viewDirection;
        }

        private void ProcessShoot(bool isShoot)
        {
            if (isShoot)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            ShootEvent?.Invoke(BulletSpawnPoint.position, transform.rotation);
        }
    }
}