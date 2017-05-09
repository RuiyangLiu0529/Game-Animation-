using UnityEngine;
using System.Collections;

public class cloth_motion: MonoBehaviour {

	float 		t;
	int[] 		edge_list;
	float 		mass;
	float		damping;
	float 		stiffness;
	float[] 	L0;
	Vector3[] 	velocities;

	Vector3	    gravity;

	Vector3[]	x_e;
	Vector3[]	x_new;
	float[]		edges;

	GameObject  Sphere;
	Vector3		Sphere_Center;

	public Vector3[] forces;
	public Vector3 force;



	// Use this for initialization
	void Start () 
	{
		t 			= 0.075f;
		mass 		= 1.0f;
		damping 	= 0.99f;
		stiffness 	= 1000.0f;
		gravity = new Vector3 (0, -9.8f, 0);

		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		int[] 		triangles = mesh.triangles;
		Vector3[] 	vertices = mesh.vertices;
		x_e	= new Vector3[vertices.Length];
		forces = new Vector3[vertices.Length];
		x_new = vertices;

		//Construct the original edge list
		int[] original_edge_list = new int[triangles.Length*2];
		for (int i=0; i<triangles.Length; i+=3) 
		{
			original_edge_list[i*2+0]=triangles[i+0];
			original_edge_list[i*2+1]=triangles[i+1];
			original_edge_list[i*2+2]=triangles[i+1];
			original_edge_list[i*2+3]=triangles[i+2];
			original_edge_list[i*2+4]=triangles[i+2];
			original_edge_list[i*2+5]=triangles[i+0];
		}
		//Reorder the original edge list
		for (int i=0; i<original_edge_list.Length; i+=2)
			if(original_edge_list[i] > original_edge_list[i + 1]) 
				Swap(ref original_edge_list[i], ref original_edge_list[i+1]);
		//Sort the original edge list using quicksort
		Quick_Sort (ref original_edge_list, 0, original_edge_list.Length/2-1);

		int count = 0;
		for (int i=0; i<original_edge_list.Length; i+=2)
			if (i == 0 || 
			    original_edge_list [i + 0] != original_edge_list [i - 2] ||
			    original_edge_list [i + 1] != original_edge_list [i - 1]) 
					count++;

		edge_list = new int[count * 2];
		edges = new float[edge_list.Length / 2];
		int r_count = 0;
		for (int i=0; i<original_edge_list.Length; i+=2)
			if (i == 0 || 
			    original_edge_list [i + 0] != original_edge_list [i - 2] ||
				original_edge_list [i + 1] != original_edge_list [i - 1]) 
			{
				edge_list[r_count*2+0]=original_edge_list [i + 0];
				edge_list[r_count*2+1]=original_edge_list [i + 1];
				r_count++;
			}


		L0 = new float[edge_list.Length/2];
		for (int e=0; e<edge_list.Length/2; e++) 
		{
			int v0 = edge_list[e*2+0];
			int v1 = edge_list[e*2+1];
			L0[e]=(vertices[v0]-vertices[v1]).magnitude;
		}

		velocities = new Vector3[vertices.Length];
		for (int v=0; v<vertices.Length; v++)
			velocities [v] = new Vector3 (0, 0, 0);

		//for(int i=0; i<edge_list.Length/2; i++)
		//	Debug.Log ("number"+i+" is" + edge_list [i*2] + "and"+ edge_list [i*2+1]);
	}

	void Quick_Sort(ref int[] a, int l, int r)
	{
		int j;
		if(l<r)
		{
			j=Quick_Sort_Partition(ref a, l, r);
			Quick_Sort (ref a, l, j-1);
			Quick_Sort (ref a, j+1, r);
		}
	}

	int  Quick_Sort_Partition(ref int[] a, int l, int r)
	{
		int pivot_0, pivot_1, i, j;
		pivot_0 = a [l * 2 + 0];
		pivot_1 = a [l * 2 + 1];
		i = l;
		j = r + 1;
		while (true) 
		{
			do ++i; while( i<=r && (a[i*2]<pivot_0 || a[i*2]==pivot_0 && a[i*2+1]<=pivot_1));
			do --j; while(  a[j*2]>pivot_0 || a[j*2]==pivot_0 && a[j*2+1]> pivot_1);
			if(i>=j)	break;
			Swap(ref a[i*2], ref a[j*2]);
			Swap(ref a[i*2+1], ref a[j*2+1]);
		}
		Swap (ref a [l * 2 + 0], ref a [j * 2 + 0]);
		Swap (ref a [l * 2 + 1], ref a [j * 2 + 1]);
		return j;
	}

	void Swap(ref int a, ref int b)
	{
		int temp = a;
		a = b;
		b = temp;
	}

	/*
	 * temp - x[] <- 0
	 * temp - N[] <- 0
	 * for every edge e={i,j}
	 * get every edge get xie xje
	 * temp - x[i] += xie;
	 * temp - n[i]++;
	 * (same for j);
	 * for every vertex i 
	 * xnew[i] <= (0.2x[i]+tempx[i])/0.2+temp - N[i]
	 * v[i]<-v[i]+1/deta t(xnew-xi)
	 * x[i] <- xnew[i]
	*/
	void Strain_Limiting(Vector3[] vertices)
	{
		//variable: x_e x_new edges


		// initialize 
		for (int i = 0; i < vertices.Length; i++) {
			if (i == 0 || i == 10)
				continue;
			x_e [i] = new Vector3 (0, 0, 0);
			x_new [i] = new Vector3 (0, 0, 0);	
		}
		for (int i = 0; i < edges.Length; i++) {
			edges [i] = 0f;
		}

		for (int i = 0; i < edge_list.Length / 2; i++) {
			int xi = edge_list [i * 2 + 0];
			int xj = edge_list [i * 2 + 1];
			x_e [xi] += 0.5f * (vertices [xi] + vertices [xj] + L0 [i] * ((vertices [xi] - vertices [xj]) / Vector3.Magnitude (vertices [xi] - vertices [xj])));
			edges [xi]++;
			x_e [xj] += 0.5f * (vertices [xi] + vertices [xj] + L0 [i] * ((vertices [xj] - vertices [xi]) / Vector3.Magnitude (vertices [xj] - vertices [xi])));
			edges [xj]++;
		}

		for (int i = 0; i < vertices.Length; i++) {
			if (i == 0 || i == 10)
				continue;
			x_new [i] = (1.0f / (0.2f + edges [i])) * (0.2f * vertices [i] + x_e [i]);
			velocities [i] += (x_new [i] - vertices [i]) / t;
			vertices [i] = x_new [i];
		}

    
	}

	void Spring(Vector3[] vertices) {
		for (int i = 0; i < vertices.Length; i++) {
			forces[i] = mass * gravity;
		}

		for (int i = 0; i < edge_list.Length / 2; i++) {

			int xi = edge_list[2 * i];
			int xj = edge_list[2 * i + 1];

			// fi = -k(1-l0/||xi-xj||)(xi-xj)
			force =  (1 -L0[i] / Vector3.Magnitude(vertices[xi] - vertices[xj])) * (vertices[xi] - vertices[xj]);

			Vector3  force0 = -stiffness  * force;
			forces[xi] += force0;
			forces[xj] -= force0;
		}

		for (int i = 0; i < vertices.Length; i++)
		{
			if (i == 0 || i == 10) 
				continue;
			velocities[i] = (velocities[i] + t / mass * forces[i]) * damping;
			vertices[i] = vertices[i] + t * velocities[i];
		} 

	}

	void Collision_Handling(Vector3[] vertices)
	{
		for (int i = 0; i < vertices.Length; ++i) {
			if (i == 0 || i == 10) 
				continue;
			// set r to be 2.8f 
			if (Vector3.Magnitude(vertices[i] - Sphere_Center) < 2.8f) {
				x_new [i] = Sphere_Center + 2.8f * (vertices [i] - Sphere_Center) / Vector3.Magnitude (vertices [i] - Sphere_Center);
				velocities [i] += (x_new [i] - vertices [i]) / t;
				vertices [i] = x_new [i];
			}
		}

	}



	// Update is called once per frame
	void Update () 
	{
		Sphere = GameObject.Find ("Sphere");
		Sphere_Center = Sphere.transform.position;

		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Vector3[] vertices = mesh.vertices;

		//Basic Set Up

		for (int i = 0; i < vertices.Length; i++) {
			if (i == 0 || i == 10)
				continue;
			// accelararte the velocity
			velocities [i] += gravity * t;
			//damp
			velocities [i] *= damping;
			//update vertice
			vertices   [i] += velocities[i] * t;
		}

		//Spring 
		//Spring(vertices);
        
	
		// Strain Limiting
		for (int i = 0; i < 64; ++i) {
			Strain_Limiting (vertices);
		}
			
		// Collision Handling
		Collision_Handling (vertices);
		
		mesh.vertices = vertices;
		mesh.RecalculateNormals ();

	}
}
