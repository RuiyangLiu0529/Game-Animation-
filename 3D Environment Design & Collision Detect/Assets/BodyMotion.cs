using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMotion : MonoBehaviour {
	public GameObject cylinder;
	public Vector3 player_position;
	public Vector3 cylinder_position;
	public float player_cylinder_distance;
	public float falling_speed;
	public Vector3 player_position_update;
	//public bool isJump = true;

	// Use this for initialization
	void Start () {
		cylinder = GameObject.Find ("BigTallCylinder");
		cylinder_position = cylinder.transform.position;
		
	}
	
	// Update is called once per frame

	void Update () {
		player_position = transform.position;


		/**************************Cylinder_Collision_Modile************************/
		float player_cylinder_distance = Mathf.Sqrt(Mathf.Pow((player_position.x-cylinder_position.x),2)+Mathf.Pow((player_position.z-cylinder_position.z),2));
		float cylinder_radius = 0.55f;// true value is 0.5f

		if (player_cylinder_distance <0.85f) {
			player_position_update.x = cylinder_position.x + (0.85f * (player_position.x - cylinder_position.x) )/ player_cylinder_distance;
			player_position_update.z = cylinder_position.z + (0.85f * (player_position.z - cylinder_position.z)) / player_cylinder_distance;
			player_position_update.y = player_position.y;
			transform.position = player_position_update;
		}	
		/**************************Cylinder_Collision_Modile************************/


		/**************************Body_Motion_Modile************************/
		if (Input.GetKey(KeyCode.UpArrow))
		{

			transform.Translate(0, 0, Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{

			transform.Translate(0, 0,  -Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{

			transform.Rotate(Vector3.down,  10f * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{

			transform.Rotate(Vector3.up, 10f * Time.deltaTime);
		}

		/**************************Body_Motion_Modile************************/

		/**************************Body_Jump_Modile************************/


		Vector3 player_position_jump = transform.position;

		falling_speed += -9.8f * Time.deltaTime;

		player_position_jump.y += falling_speed * Time.deltaTime;

		//object oriented y = 0.7f 
		if (player_position_jump.y < 0.7f) {
			falling_speed = 0.0f;
			player_position_jump.y = 0.7f;
		}

		if (Input.GetKey (KeyCode.Space)) {
			falling_speed += 1f;

		}

		transform.position = player_position_jump;
	

			

		/**************************Body_Jump_Modile************************/

	
	}
}
