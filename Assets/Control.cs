using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {

	
	public bool holdGen = false;
	public int sinStrenght;
	public MeshBuilder meshBuilder;
	public GameObject objectHolder;
	public int generation = 0;
	
	public MushroomConfig config;
	public GameObject[] objects;
	public Material vertexMaterial;

	//public Vector3 fenceMiddle;
	void Start()
	{
		objects = new GameObject[100];
		for (int x = 0; x < 10; ++x)
		{
			for (int z = 0; z < 10; ++z)
			{
				GameObject shroomObj = new GameObject();
				shroomObj.transform.position = new Vector3(x * 5, 0, z * 5);
				shroomObj.AddComponent<MeshFilter>();
				shroomObj.AddComponent<MeshRenderer>();
				shroomObj.GetComponent<MeshRenderer>().material = vertexMaterial;
				objects[x * 10 + z] = shroomObj;
			}
		}


		Generate();
		//Generate();

	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.X)) {
			Generate();
		}
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
		else
		{
			config = new MushroomConfig();
		}

		//Debug.Log("Generation" + generation);
		//this.CreateFence();

		foreach (GameObject obj in objects)
		{
			config.Generate();
			MushRoom shroom = new MushRoom(config);
			//Create the mesh:
			Mesh mesh = shroom.Create();
			mesh.RecalculateNormals();

			Color[] colors = new Color[mesh.vertices.Length];
			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				colors[i] = new Color(Random.value, Random.value, Random.value); //Color.red;
			}
			mesh.colors = colors;


			MeshFilter filter = obj.GetComponent<MeshFilter>();
			filter.sharedMesh = filter.mesh = mesh; //TODO:Which one do we need and why?
			generation++;
		}

	}
}
