using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public int counter = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)  ) // if trigger is pressed 
		{
			counter++;
			if (counter % 3 == 0) { // and maximum every 3 frame 
				Fire ();			// a bullet is released	
			}
		}

	}

	void Fire()
	{
		// Create the Bullet from the Bullet Prefab
		var bullet = (GameObject)Instantiate (
			bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation);

		// Add velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * -6; // -6 because forward should be backward

		// Destroy the bullet after 1.5 seconds
		Destroy(bullet, 1.5f);
	}

}
