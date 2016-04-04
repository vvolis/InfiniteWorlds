using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {

	
	public bool holdGen = false;
	public int sinStrenght;
	public MeshBuilder meshBuilder;
	public GameObject objectHolder;
	public int generation = 0;
	
	public MushroomConfig config;

	//public Vector3 fenceMiddle;
	void Start()
	{
		config = new MushroomConfig();
		StartCoroutine(ReGenerate(1F));
		//Generate();

	}

	IEnumerator ReGenerate(float waitTime)
	{
		while (true)
		{
			yield return new WaitForSeconds(waitTime);
			if (!holdGen)
			{
				Generate();
			}
		}
	}

	void Generate()
	{

		if (generation > 0)
		{
			//mesh.Clear();
			//filter.sharedMesh.Clear();
			//filter.mesh.Clear();
		}

		//Debug.Log("Generation" + generation);
		//this.CreateFence();
		MushRoom shroom = new MushRoom(config);
		//Create the mesh:
		Mesh mesh = shroom.Create();
		mesh.RecalculateNormals();
		MeshFilter filter = objectHolder.GetComponent<MeshFilter>();
		filter.sharedMesh = filter.mesh = mesh; //TODO:Which one do we need and why?
		generation++;

	}
}
