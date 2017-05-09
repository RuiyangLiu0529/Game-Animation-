using UnityEngine;
using System.Collections;

public class Rigid_Bunny : MonoBehaviour 
{
	public bool isLaunched = false;
	public bool flag = false; // stop the collision 
	public bool hasCollision  = false;

	public Vector3 x;							// position
	public Vector3 v    = new Vector3(0, 0, 0);	// velocity
	public Vector3 w    = new Vector3(0, 0, 2);	// angular velocity


	public float m;
	public float mass;							// mass
	public Matrix4x4 I_body;					// body inertia
	public float linear_damping;				// for damping
	public float angular_damping;
	public float restitution;					// for collision

	public Quaternion q = Quaternion.identity;	// quaternion

	public float g;

	// Rotation 
	public Vector4 w4;							
	public Quaternion newW;						
	public Quaternion newQ;


	//Collision 					
	public Matrix4x4 RotationMatrix;			// rotation matrix
	Vector3 N = new Vector3 (0, 1, 0);			// z direction
	Vector3 N1 = new Vector3 (1, 0, 0);			// x direction

	public Matrix4x4 K;							
	public Vector3 riSum;
	public Vector3 ri;
	public Vector3 vi;
	public Vector3 xi;
	public Vector3 j;


	// Use this for initialization
	void Start () 
	{
		//Initialize coefficients
		w  = new Vector3 (0, 0, 0);
		w4 = new Vector4 (w[0], w[1], w[2], 0);	
		x  = new Vector3 (0, 0.6f, 0);
		v  = new Vector3 (4, 2, 0);
		q  = Quaternion.identity;
		linear_damping  = 0.999f;
		angular_damping = 0.98f;
		restitution 	= 0.5f;					//elastic collision
		m 				= 1;
		mass 			= 0; 
		g               = -9.8f;
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		for (int i=0; i<vertices.Length; i++) 
		{
			mass += m;
			float diag = m*vertices[i].sqrMagnitude;
			I_body[0, 0]+=diag;
			I_body[1, 1]+=diag;
			I_body[2, 2]+=diag;
			I_body[0, 0]-=m*vertices[i][0]*vertices[i][0];
			I_body[0, 1]-=m*vertices[i][0]*vertices[i][1];
			I_body[0, 2]-=m*vertices[i][0]*vertices[i][2];
			I_body[1, 0]-=m*vertices[i][1]*vertices[i][0];
			I_body[1, 1]-=m*vertices[i][1]*vertices[i][1];
			I_body[1, 2]-=m*vertices[i][1]*vertices[i][2];
			I_body[2, 0]-=m*vertices[i][2]*vertices[i][0];
			I_body[2, 1]-=m*vertices[i][2]*vertices[i][1];
			I_body[2, 2]-=m*vertices[i][2]*vertices[i][2];
		}
		I_body [3, 3] = 1;
	}


	/******************Some Math Operations with Matrix*****************/

	Matrix4x4 Get_Cross_Matrix(Vector3 a)
	{
		//Get the cross product matrix of vector a
		Matrix4x4 A = Matrix4x4.zero;
		A [0, 0] = 0; 
		A [0, 1] = -a[2]; 
		A [0, 2] = a[1]; 
		A [1, 0] = a[2]; 
		A [1, 1] = 0; 
		A [1, 2] = -a[0]; 
		A [2, 0] = -a[1]; 
		A [2, 1] = a[0]; 
		A [2, 2] = 0; 
		A [3, 3] = 1;
		return A;
	}

	Matrix4x4 Quaternion2Matrix(Quaternion q) {
		//covert the quaternion matrix to the rotation matrix
		// equation (4)

		Matrix4x4 R = Matrix4x4.zero;
		R [0, 0] = q [3] * q [3] + q [0] * q [0] - q [1] * q [1] - q [2] * q [2];
		R [0, 1] = 2 * (q [0] * q [1] - q [3] * q [2]);
		R [0, 2] = 2 * (q [0] * q [2] + q [3] * q [1]);
		R [1, 0] = 2 * (q [0] * q [1] + q [3] * q [2]);
		R [1, 1] = q [3] * q [3] - q [0] * q [0] + q [1] * q [1] - q [2] * q [2];
		R [1, 2] = 2 * (q [1] * q [2] - q [0] * q [3]);
		R [2, 0] = 2 * (q [0] * q [2] - q [3] * q [1]);
		R [2, 1] = 2 * (q [1] * q [2] + q [0] * q [3]);
		R [2, 2] = q [3] * q [3] - q [0] * q [0] - q [1] * q [1] + q [2] * q [2];
		R [3, 3] = 1;
		return R;
	}


	Quaternion NormalizeQuanternion(Quaternion q) {
		// Normalize the Quanternion
		// keep x2+y2+z2+w2 = 1;
		Quaternion NormalizedQ;
		float size = Mathf.Sqrt (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

		// normalization
		NormalizedQ.x = q.x / size;
		NormalizedQ.y = q.y / size;
		NormalizedQ.z = q.z / size;
		NormalizedQ.w = q.w / size;

		return NormalizedQ;
	}

	Quaternion Multiply(Quaternion q1, Quaternion q2) {
		// multiply q1q2
		//q1q2 means rotation by q2 then rotation by q1;
		// q1q2 = [w1v2+w2v1+v1xv2, w1w2-v1v2];

		// set  axis v1,v2;
		Vector3 v1 = new Vector3 (q1.x, q1.y, q1.z);
		Vector3 v2 = new Vector3 (q2.x, q2.y, q2.z);

		// set scalar w1, w2

		float w1 = q1.w;
		float w2 = q2.w;

		Quaternion multipleQ = Quaternion.identity;

		//calculete 
		Vector3 multipleV = w1 * v2 + w2 * v1 + Vector3.Cross (v1, v2);
		float w = w1 * w2 - Vector3.Dot (v1, v2);

		//set value
		multipleQ.x = multipleV.x;
		multipleQ.y = multipleV.y;
		multipleQ.z = multipleV.z;
		multipleQ.w = w;

		return multipleQ;


	}

	/******************Some Math Operation with Matrix*****************/


	/*********Collision Handler***********/
	void Collision_Handler(float dt)
	{
		bool isGround = false;
		bool isWall = false;

		RotationMatrix= Quaternion2Matrix(q);


		riSum = new Vector3 (0, 0, 0);
		float c = 0;
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;


		for (int i = 0; i < vertices.Length; ++i) 
		{
			ri =  RotationMatrix * vertices[i];				
			xi = x + ri;
			vi = Get_Cross_Matrix(w) * ri;
			vi += v;
			// collision dectection
			// Ground
			if ((Vector3.Dot(xi, N) < 0) && (Vector3.Dot(vi, N) < 0))
			{
				riSum += ri;
				c     += 1f;
				isGround = true;
			}
			// Wall 2 is the position of the wall
			if((Vector3.Dot(xi,N1) >= 2) && (Vector3.Dot(vi, N1) > 0)) {
				riSum += ri;
				c     += 1f;
				isWall = true;
				
			}

		}
		// collision response
		if(c != 0)
		{
			hasCollision = true;

			ri = riSum / c;
			vi = Get_Cross_Matrix(w) * ri;	
			vi += v;

			Matrix4x4 RStar = Get_Cross_Matrix(ri);
			I_body =  RotationMatrix * I_body * Matrix4x4.Transpose ( RotationMatrix);
			K = Matrix4x4.Transpose (RStar) * Matrix4x4.Inverse (I_body) * RStar;

			for (int i = 0; i <= 3; ++i) 
			{
				K [i, i] += 1 / mass;
			}

			if(isGround == true){
				j = Matrix4x4.Inverse(K) * (-vi - restitution * Vector3.Dot(vi, N) * N);	
			}else {
				j = Matrix4x4.Inverse(K) * (-vi - restitution * Vector3.Dot(vi, N1) * N1);	
			}
			v += j / mass;
			w4 += Matrix4x4.Inverse (I_body) * RStar * j;
			w = new Vector3 (w4[0], w4[1], w4[2]);


		}
		else
		{	

			if(isLaunched == true)
			{
				v += new Vector3 (0, g, 0) * dt; // if no collision the bunny drop as free-falling
			}
		}
	}
	/*********Collision Handler***********/

	// Update is called once per frame
	void Update()
	{
		float dt = 0.02f;
		w4 = new Vector4 (w[0], w[1], w[2], 0);

		if(Input.GetKey("r")) 
		{
			x  = new Vector3 (0, 0.6f, 0);
			w  = new Vector3 (0, 0, 2);

			restitution = 0.5f;
			isLaunched    = false;
			hasCollision = false;
			flag = false;
		}

		if(Input.GetKey("l")) 
		{
			v  = new Vector3 (4, 2, 0);
			isLaunched 	= true;
			hasCollision         = false;
			flag        = false;
		}
		// Part I: Update velocities

		// Part II: Collision Handler

		Collision_Handler (dt);

		// Part III: Update position & orientation
		//Update linear status
		if(isLaunched == true) 
		{
			v *= linear_damping;
			x += v * Time.deltaTime;
		}
		//Update angular status
		// if(col == false)
		// {
		newW   = new Quaternion (w4 [0], w4 [1], w4 [2], w4 [3]);
		newQ = Multiply (newW, q);
		for (int i = 0; i < 4; i++) {
			q [i] += 0.5f * Time.deltaTime * newQ [i];
		}
		q = NormalizeQuanternion (q);
		// }

		if(hasCollision == true)
		{	
			w *= angular_damping;

			if(Mathf.Abs(Vector3.Dot(vi, N)) < 0.01f)
			{	
				restitution = 0; // if don't set the restitution here the bunny will waggle slowly through the ground

			}
			if((Mathf.Abs(Vector3.Dot(vi, N)) < 0.0005f) && (Vector3.SqrMagnitude(w) < 0.08f))
			{	

				flag = true; // collision stop flag
			}
		}

		// Part IV: Assign to the bunny object


		if ((hasCollision == false) || (flag == false))
		{
			transform.position = x;
			transform.rotation = q;

		}
	}
}
