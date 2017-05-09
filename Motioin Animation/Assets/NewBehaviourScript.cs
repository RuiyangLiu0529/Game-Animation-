using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
	Animator anim;
	int jumpHash = Animator.StringToHash("Jump");
	int runStateHash = Animator.StringToHash("Base Layer.Real_Run");
	int waveHash = Animator.StringToHash("Wave");
	int reviveHash = Animator.StringToHash("Reviving");
	int dyingHash = Animator.StringToHash("Dying");
	bool isStand;
	bool ifRun;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		isStand = true;
		ifRun = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		float move = Input.GetAxis ("Vertical");
		anim.SetFloat("Speed", move);
		float horizontal = Input.GetAxis ("Horizontal");
		anim.SetFloat ("Direction", horizontal, 0.25f, Time.deltaTime);


		AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

		if(Input.GetKeyDown(KeyCode.Space) && stateInfo.nameHash == runStateHash) {
			anim.SetTrigger (jumpHash);	
		}

		if(Input.GetKeyDown("1") && isStand == true) {
			anim.SetTrigger (dyingHash);
			isStand = false;
		}

		if (Input.GetKeyDown ("2") && isStand == false) {
			anim.SetTrigger (reviveHash);
			isStand = true;
		}

		if (Input.GetKeyDown ("3") && isStand == true) {
			anim.SetTrigger (waveHash);
		}

		if (move > 0.5f) {
			ifRun = true;
		} else {
			ifRun = false;
		}
		anim.SetBool ("ifRun", ifRun);


	}
}
