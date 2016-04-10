using UnityEngine;
using System.Collections;
public delegate float ModifierDelegate(float step);

public class PipeConfig
{
	public bool createCaps = true;
	public float bendAngle = 0;
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
