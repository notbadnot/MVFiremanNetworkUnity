using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using View.Components;
using System;

public class BonusAcceptor : MonoBehaviour
{
    [SerializeField] GameObject sparksPrefab;
    [SerializeField] GameObject icePrefab;
    [SerializeField] PhotonView _photonView;
    private PlayerController playerController;
    public bool accelerationApplied = false;
    public bool freezerApplied = false;

    private float initialSpeed;

    private GameObject sparks;
    private GameObject iceCube;

    private bool localAccelerationApplied = false;
    private bool localFreezerApplied = false;

    public event Action<BonusModel.Effect, BonusModel.Targets, int>  ActivateBonusEvent;
    void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        initialSpeed = playerController.Speed;
    }
    public void SetAccelerationOn()
    {
        accelerationApplied = true;
    }
    public void SetFreezerOn()
    {
        freezerApplied = true;
    }
    private IEnumerator AccelerationWearingOff()
    {
        yield return new WaitForSeconds(15f);
        accelerationApplied = false;
        Debug.Log("Effect weared off");
    }
    private void ApplyAcceleration()
    {
        sparks = Instantiate(sparksPrefab,gameObject.transform.position, Quaternion.identity);
        sparks.transform.SetParent(gameObject.transform);
        playerController.Speed = playerController.Speed * 5;
        localAccelerationApplied = true;
        StartCoroutine(AccelerationWearingOff());
        Debug.Log("Acceleration applied to player id " + playerController.Id);
    }

    private void UnApplyAcceleration()
    {
        Destroy(sparks);
        playerController.Speed = playerController.Speed / 5;
        localAccelerationApplied = false;
    }

    private IEnumerator FreezerWearingOff()
    {
        yield return new WaitForSeconds(3f);
        freezerApplied = false;
        Debug.Log("Effect weared off");
    }

    private void ApplyFreezer()
    {
        iceCube = Instantiate(icePrefab, gameObject.transform.position, Quaternion.identity);
        iceCube.transform.SetParent(gameObject.transform);
        playerController.Speed = playerController.Speed * 0;
        localFreezerApplied = true;
        StartCoroutine(FreezerWearingOff());
        Debug.Log("Freezer applied to player id " + playerController.Id);
    }

    private void UnApplyFreezer()
    {
        Destroy(iceCube);
        if (localAccelerationApplied != true)
        {
            playerController.Speed = initialSpeed;
        }
        else
        { playerController.Speed = initialSpeed * 5; }
        localFreezerApplied = false;
    }

    public void pickedUpBonus (BonusModel.Effect effect, BonusModel.Targets targets)
    {
        ActivateBonusEvent?.Invoke(effect, targets ,playerController.Id);
    }

    public void Update()
    {
        if (accelerationApplied && localAccelerationApplied == false)
        {
            ApplyAcceleration();
        }
        if (!accelerationApplied && localAccelerationApplied == true)
        {
            UnApplyAcceleration();
        }
        if (freezerApplied && localFreezerApplied == false)
        {
            ApplyFreezer();
        }
        if (!freezerApplied && localFreezerApplied == true)
        {
            UnApplyFreezer();
        }
    }

}
