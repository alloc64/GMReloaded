using UnityEngine;
using System.Collections;

public class SunLensFlare : MonoBehaviour 
{
	[SerializeField]
	private LensFlare lensFlare;

	private float coord1 = 1.2f; 
	private float coord2 = -0.2f;

	//The strength of the flare relative to it's distance from the camera ("brightness = strength/distance")
	private int strength = 5;

	//Simple counter to ensure that the flare is visible for a few frames before the layer is changed
	private int count = 0;

	private void Start()
	{
		//Ensures that the flare's layer isn't part of its ingore list to begin with
		//Change to whatever you want, as long as it's not on the ignore list
		//gameObject.layer = Layer.Default;
		lensFlare = GetComponent<LensFlare>();
	}

	private void Update ()
	{
		Vector3 heading = gameObject.transform.position - Camera.main.transform.position;
		Vector3 heading2 = Camera.main.transform.position -gameObject.transform.position;
		float dist = Vector3.Dot(heading, Camera.main.transform.forward);
		Vector3 viewPos = Camera.main.WorldToViewportPoint (gameObject.transform.position);

		//Turns off the flare when it goes outside of the camera's frustrum
		if(viewPos.x > coord1 || viewPos.x < coord2 || viewPos.y < coord2 || viewPos.y > coord1)
		{
			lensFlare.brightness = 0;

		}
		else if(Physics.Raycast(transform.position, heading2.normalized, Mathf.Clamp(dist, 0.01f, 20f)))
		{
			lensFlare.brightness = 0;
		}
		else
		{
			//Sets the flares brightness to be an inverse function of distance from the camera
			lensFlare.brightness = strength/dist;

			if(count<50)
				count = count+1;
		}

		//Changes the layer of the flare to be "Transparent FX"
		//Change it to whatever you want as long as you include that layer in your ignore list of the flare
		if(count > 20)
		{
			gameObject.layer= 1;
		}
	}
}