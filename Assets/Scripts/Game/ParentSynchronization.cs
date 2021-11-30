using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ParentSynchronization : MonoBehaviour, IPunObservable
{

    [SerializeField] private PhotonView _photonView;
    private bool _firstData = true;
    public GameObject _parent; /*= null; //new*/
    public Vector3 _parentposition;
    public void Update()
    {
        /*if (_photonView.IsMine)
            return;*/
        Debug.Log(_parent);
        if (_parent != null)
        {
            transform.SetParent(_parent.transform)  ; // new
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.parent.position); // new
        }
        else
        {
            _parentposition = (Vector3)stream.ReceiveNext(); // new
        }
    }

}
