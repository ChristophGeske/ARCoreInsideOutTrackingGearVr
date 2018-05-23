using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColideColourChange : MonoBehaviour {

	public Material newColour;
	bool colourChangeCollision = false;

	void Start (){

	}

	// Update is called once per frame
	void Update () {
		if(colourChangeCollision){
			this.GetComponent<Renderer>().material = newColour;
		}

	}

	void OnCollisionEnter(Collision other) {
		colourChangeCollision = true;
	}
		
}
