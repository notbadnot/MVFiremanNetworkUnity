using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePS : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ParticleSystem>().Play();
        Debug.Log("I'm Playing");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
