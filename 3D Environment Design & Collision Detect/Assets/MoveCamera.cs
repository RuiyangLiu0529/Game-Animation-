using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {
	public float swing_angle = 0;
	public float elevate_angle = 0;

	// Use this for initialization
	void Start () {
		transform.localRotation = Quaternion.identity;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetMouseButton(0))
		{
			float swing= Input.GetAxis("Mouse X");
			float elevate = Input.GetAxis("Mouse Y");
			elevate_angle = elevate_angle + elevate; 
			swing_angle = swing_angle + swing;
			transform.Rotate(elevate, swing, 0);

		}
	
		if (!Input.GetMouseButton(0))
		{
			if (elevate_angle <= 0)
			{
				transform.Rotate( -0.1f, 0, 0);
				elevate_angle += 0.1f;
			}
			if (elevate_angle >= 0)
			{
				transform.Rotate(0.1f,0, 0);
				elevate_angle -= 0.1f;
			}
			if (swing_angle <= 0)
			{
				transform.Rotate(Vector3.up, 0.1f);
				swing_angle += 0.1f;
			}
			if (swing_angle >= 0)
			{
				swing_angle -= 0.1f;
				transform.Rotate(Vector3.up, -0.1f);
			}


		}
		
	}
}
