using UnityEngine;
using System.Collections;

public class MushRoom : MeshBuilder
{

	
	MushroomConfig config;

	public MushRoom(MushroomConfig config)
	{
		this.config = config;

		
	}

	public Mesh Create()
	{
		config.capPeak = new Vector3(0.0f, config.capThickness, 0.0f);
		config.capRim = new Vector3(config.capRadius, -config.capHeight + config.capThickness, 0.0f);

		//Stem
		StartAndEnd startAndEnd;
		startAndEnd = this.CreatePipe(config.GetStemPipeConfig());
		//Cap
		this.CreatePipe(config.GetCapPipeConfig(startAndEnd));
		//Gills
		//this.CreatePipe(config.GetGillsPipeConfig(startAndEnd));

		return this.BuildMesh();
	}


}