using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Bonus : MonoBehaviour
{
    protected Collider collider;
    public Rigidbody rigidbody;
    public BonusModel.Effect effect;
    public BonusModel.Targets targets;

    public event Action<int> BonusGathered;

    public int ?spawnPoint = null;
    private void Awake()
    {
        collider = gameObject.GetComponent<Collider>();
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0.5f, 0, 1));
    }



    private void OnCollisionEnter(Collision collision)
    {
        var picker = collision.gameObject.GetComponent<BonusAcceptor>();
        if (picker != null)
        {
            picker.pickedUpBonus(effect, targets);
            if (spawnPoint != null)
            {
                BonusGathered?.Invoke(spawnPoint.Value);
            }
            Photon.Pun.PhotonNetwork.Destroy(gameObject);
        }
    }







}
