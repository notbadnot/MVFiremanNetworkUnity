using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Game;
using System;
using Random = UnityEngine.Random;

public class BonusManager : MonoBehaviour, IPunObservable
{
    [SerializeField] List<BonusSpawnPoint> spawnPoints;
    [SerializeField] PhotonView photonView;
    
    [SerializeField] List<string> bonusPrefabsPath;

    [SerializeField] List<bool> correctBools;
    struct BonusSpawnPoint
    {
        public Vector3 position;
        public bool occupated;
        public BonusSpawnPoint(Vector3 position, bool occupated)
        {
            this.position = position;
            this.occupated = occupated;
        }
    }

    public PlayersManager playersManager;

    private void Start()
    {
        var p1 = new BonusSpawnPoint(new Vector3(-2, 1, 2), false);
        var p2 = new BonusSpawnPoint(new Vector3(-6, 1, -7), false);
        var p3 = new BonusSpawnPoint(new Vector3(9, 1, -7), false);
        spawnPoints = new List<BonusSpawnPoint>();
        spawnPoints.Add(p1);
        spawnPoints.Add(p2);
        spawnPoints.Add(p3);
        correctBools = new List<bool>();
        foreach (var point in spawnPoints)
        {
            correctBools.Add(point.occupated);
            Debug.Log(point);
        }
    }





    public void ApplyEffectOfBonus(BonusModel.Effect effect, BonusModel.Targets targets, int EffectUserId)
    {

        photonView.RPC(nameof(ApplyEffectOfBonusRPC), RpcTarget.All, effect, targets,EffectUserId); ;
 
    }

    [PunRPC]
    private void  ApplyEffectOfBonusRPC(BonusModel.Effect effect, BonusModel.Targets targets, int EffectUserId) 
    {
        playersManager.UseEffectOnTargets(effect, targets, EffectUserId);
    }

    private void Update()
    {
        for (int i = 0; i< spawnPoints.Count; i++)
        {
            var spawnPoint = spawnPoints[i];
            spawnPoint.occupated = correctBools[i];
            spawnPoints[i] = spawnPoint;
        }


        if (Random.Range(0f, 1f) < 0.0005f && PhotonNetwork.CurrentRoom != null && PhotonNetwork.IsMasterClient)
        {
            var spawnPointNumber = Random.Range(0, spawnPoints.Count);
            if (!spawnPoints[spawnPointNumber].occupated)
            {
                var prefabNumber = Random.Range(0, bonusPrefabsPath.Count);
                var spawnedBonus = PhotonNetwork.Instantiate(bonusPrefabsPath[prefabNumber], spawnPoints[spawnPointNumber].position, Quaternion.identity).GetComponent<Bonus>();
                var spawnPoint = spawnPoints[spawnPointNumber] ;
                spawnPoint.occupated = true;
                correctBools[spawnPointNumber] = true;
                spawnPoints[spawnPointNumber] = spawnPoint;
                spawnedBonus.spawnPoint = spawnPointNumber;
                spawnedBonus.BonusGathered += BonusManager_BonusGathered;
            }
        }
    }

    private void BonusManager_BonusGathered(int obj)
    {
        var spawnPoint = spawnPoints[obj];
        spawnPoint.occupated = false;
        correctBools[obj] = false;
        spawnPoints[obj] = spawnPoint;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                stream.SendNext(spawnPoint.occupated);
            }
            
        }
        else
        {
            
            for (int i = 0; i < correctBools.Count; i++)
            {
                correctBools[i] = (bool)stream.ReceiveNext();
            }

        }
    }





}
