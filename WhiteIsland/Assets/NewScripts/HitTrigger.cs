using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTrigger : MonoBehaviour { // This class lets a stone start flying when shooting it.

    public bool hit = false;
    public float smoothSpeed = 0.0008f; // can be between 0.0f and 1.0f.
    public int counter;

    // Use this for initialization
    void Start () {
        counter = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (hit)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + Vector3.up * 6, smoothSpeed);
            counter --;
        }
        if (counter < 1)
        {
            hit = false;
            counter = 0;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        hit = true;
        counter = 250;
    }
}
