using UnityEngine;
using System.Collections;
public delegate float ModifierDelegate(float step);

public class PipeConfig
{
	public bool createCaps = true;
	public int bendAngle = 0;
	public Vector3 offset = default(Vector3);
	public Quaternion offsetRotation = default(Quaternion);
	public ModifierDelegate WidthModifier = null;
	public ModifierDelegate HeightModifier = null;
	public bool flipNormals = false;
	public int radialSegmentCount = 8;
	public float height = 1;
	public float radius = 1;
	public int heightSegmentCount = 8;
}
[System.Serializable]
public class MushroomConfig
{
	public float capThickness = 1F;
	public float capRadius = 1F;
	public float capHeight = 1F;

	public float capPeakHandleLength = 1F;
	public float capRimHandleLength = 1F;
	public float capRimHandleAngle = 45F;
	public int stemBendAndle = 45;
	public int stemHeight = 1;

	public Vector3 capRim;
	//public float rimHandle;
	public Vector3 capPeak;
	public Vector3 rimHandle;
	public Vector3 peakHandle;

	public MushroomConfig()
	{
		capPeak = new Vector3(0.0f, capHeight, 0.0f);
		capRim = new Vector3(capRadius, -capHeight + capThickness, 0.0f);
		peakHandle = new Vector3(capPeakHandleLength, 0.0f, 0.0f);

		float rimAngleRadians = capRimHandleAngle * Mathf.Deg2Rad;
		//rimHandle = new Vector3(Mathf.Cos(rimAngleRadians), Mathf.Sin(rimAngleRadians), 0.0f);
		rimHandle = new Vector3(capRimHandleLength, capRimHandleAngle, 0.0f);

	}

	public PipeConfig GetStemPipeConfig()
	{
		PipeConfig config = new PipeConfig();
		config.bendAngle = stemBendAndle;
		config.height = stemHeight;
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
		Debug.Log("capheight" + capHeight);
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