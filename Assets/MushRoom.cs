using UnityEngine;
using System.Collections;

//TODO: Fix bezier calculations for more interesting forms on cap

public class MushRoom : MeshBuilder
{

	
	MushroomConfig config;

	public MushRoom(MushroomConfig config)
	{
		this.config = config;

		
	}

	public Mesh Create()
	{
		//Stem
		StartAndEnd startAndEnd;
		startAndEnd = this.CreatePipe(config.GetStemPipeConfig());
		//Cap
		this.CreatePipe(config.GetCapPipeConfig(startAndEnd));
		//Gills
		this.CreatePipe(config.GetGillsPipeConfig(startAndEnd));

		return this.BuildMesh();
	}


}

[System.Serializable]
public class MushroomConfig
{
	[Header("Cap")]
	public float baseCapRadius = 2F;
	public float baseCapHeight = 2F;
	public float baseCapPeakHandleLength = 1F;

	public float capRimHandleLength = 1F;
	public float capRimHandleAngle = 45F;
	[Header("CapDiffs")]
	public float capRadiusDiff = 0.7F;
	public float capHeightDiff = 0.7F;
	public float capPeakHandleLengthDiff = 0.35f;

	[Space(10)]
	[Header("Stem")]
	public float baseStemRadius = 0.3f;
	public float baseStemBendAngle = 90;
	public float baseStemHeight = 2;
	[Header("StemDiffs")]
	public float stemRadiusDiff = 0.3f;
	public float stemBendAngleDiff = 0.5f;
	public float stemHeightDiff = 0.3f;

	Vector3 capRim;
	Vector3 capPeak;
	Vector3 rimHandle;
	Vector3 peakHandle;

	float capRadius = 1F;
	float capHeight = 1F;
	float capThickness = 1F;
	float stemRadius = 0.3f;
	float stemBendAngle = 45;
	float stemHeight = 1;
	float capPeakHandleLength = 1F;

	public MushroomConfig()
	{
		Generate();
	}

	public void Generate()
	{
		//Randomizers
		capRadius = baseCapRadius + baseCapRadius * Random.Range(-capRadiusDiff, capRadiusDiff);
		capHeight = baseCapHeight + baseCapHeight * Random.Range(-capHeightDiff, capHeightDiff);
		stemRadius = baseStemRadius + baseStemRadius * Random.Range(-stemRadiusDiff, stemRadiusDiff);
		stemBendAngle = baseStemBendAngle * Random.Range(-stemBendAngleDiff, stemBendAngleDiff);
		stemHeight = baseStemHeight + baseStemHeight * Random.Range(-stemHeightDiff, stemHeightDiff);
		capPeakHandleLength = baseCapPeakHandleLength + baseCapPeakHandleLength * Random.Range(-capPeakHandleLengthDiff, capPeakHandleLengthDiff);

		capThickness = capHeight; //HARDCODE
		capPeak = new Vector3(0.0f, capHeight, 0.0f);
		capRim = new Vector3(capRadius, -capHeight + capThickness, 0.0f);
		peakHandle = new Vector3(capPeakHandleLength, 0.0f, 0.0f);

		float rimAngleRadians = capRimHandleAngle * Mathf.Deg2Rad;
		rimHandle = new Vector3(Mathf.Cos(rimAngleRadians), Mathf.Sin(rimAngleRadians), 0.0f);
		//rimHandle = new Vector3(capRimHandleLength, capRimHandleAngle, 0.0f);

		capPeak = new Vector3(0.0f, capThickness, 0.0f);
		capRim = new Vector3(capRadius, -capHeight + capThickness, 0.0f);
	}

	public PipeConfig GetStemPipeConfig()
	{
		PipeConfig config = new PipeConfig();
		config.bendAngle = stemBendAngle;
		config.height = stemHeight;
		config.radius = stemRadius;
		return config;
	}

	public PipeConfig GetCapPipeConfig(StartAndEnd startAndEnd = default(StartAndEnd))
	{
		PipeConfig config = new PipeConfig();
		config.createCaps = false;
		config.bendAngle = 0; //Random?
		config.height = capHeight;
		config.radius = capRadius;
		config.WidthModifier = CapWidth;
		config.offset = startAndEnd.endPoint.position;
		config.offsetRotation = startAndEnd.endPoint.rotation;

		return config;
	}

	public PipeConfig GetGillsPipeConfig(StartAndEnd startAndEnd = default(StartAndEnd))
	{
		PipeConfig config = new PipeConfig();
		config.createCaps = false;
		config.bendAngle = 0; //Random?
		config.height = capHeight - capThickness;
		config.radius = capRadius;
		config.flipNormals = true;
		config.WidthModifier = CapWidth;
		config.offset = startAndEnd.endPoint.position;
		config.offsetRotation = startAndEnd.endPoint.rotation;

		return config;
	}

	public float CapWidth(float t)
	{
		Vector3 res = MeshBuilder.GetBezierPoint(t, capRim, rimHandle, peakHandle, capPeak);
		return res.x;
	}

	public float GillsWidth(float t)
	{
		Vector3 gillsPeak = capPeak;
		gillsPeak.y -= capThickness;
		Vector3 gillsRimHandle = new Vector3(-rimHandle.y, rimHandle.x, 0.0f);
		Vector3 res = MeshBuilder.GetBezierPoint(t, capRim, gillsRimHandle, peakHandle, gillsPeak);
		return res.x;
	}

}