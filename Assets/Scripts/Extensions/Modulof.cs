using UnityEngine;
using System.Collections;

public class Modulof 
{
	// Floating-point modulo
	// The result (the remainder) has same sign as the divisor.
	// Similar to matlab's mod(); Not similar to fmod() -   Mod(-3,4)= 1   fmod(-3,4)= -3
	public static float Modf(float x, float y)
	{
		if (0.0f == y)
			return x;

		float m = x - y * Mathf.Floor(x/y);

		// handle boundary cases resulted from floating-point cut off:

		if (y > 0.0f)              // modulo range: [0..y)
		{
			if (m>=y)           // Mod(-1e-16             , 360.    ): m= 360.
				return 0.0f;

			if (m < 0.0f)
			{
				if (y+m == y)
					return 0.0f; // just in case...
				else
					return y+m; // Mod(106.81415022205296 , _TWO_PI ): m= -1.421e-14 
			}
		}
		else                    // modulo range: (y..0]
		{
			if (m<=y)           // Mod(1e-16              , -360.   ): m= -360.
				return 0.0f;

			if (m>0.0f)
			{
				if (y+m == y)
					return 0.0f; // just in case...
				else
					return y+m; // Mod(-106.81415022205296, -_TWO_PI): m= 1.421e-14 
			}
		}

		return m;
	}
}
