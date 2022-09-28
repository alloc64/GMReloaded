using UnityEngine;
using System.Collections;

public class PackExtensions 
{
	//Helper method to emulate GLSL
	public static float fract(float value)
	{
		return (float)Modulof.Modf(value, 1.0f);
	}

	//Helper method to go from a float to packed char
	private static byte ConvertChar(float value)
	{
		//Scale and bias
		value = (value + 1.0f) * 0.5f;
		return (byte)(value * 255.0f);
	}

	//Pack 3 values into 1 float
	public static float PackToFloat(byte x, byte y, byte z)
	{
		uint packedColor = (uint)((x << 16) | (y << 8) | z);
		float packedFloat = (float) ( ((double)packedColor) / ((double) (1 << 24)) );  

		return packedFloat;
	}

	public static float PackToFloat(Vector3 normal)
	{
		return PackToFloat(ConvertChar(normal.x), ConvertChar(normal.y), ConvertChar(normal.z));
	}

	//UnPack 3 values from 1 float
	public static Vector3 UnPackFloat(float src)
	{
		float r = fract(src);
		float g = fract(src * 256.0f);
		float b = fract(src * 65536.0f);

		return new Vector3((r * 2.0f) - 1.0f, (g * 2.0f) - 1.0f, (b * 2.0f) - 1.0f);
	}
}
