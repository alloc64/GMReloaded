/************************************************************************
 * Copyright (c) 2014 Milan Jaitner                                     *
 * This program is free software: you can redistribute it and/or modify *
 * it under the terms of the GNU General Public License as published by *
 * the Free Software Foundation, either version 3 of the License, or    * 
 * any later version.													*
																		*
 * This program is distributed in the hope that it will be useful,      *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of       *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the         *
 * GNU General Public License for more details.							*
																		*
 * You should have received a copy of the GNU General Public License	*
 * along with this program.  If not, see http://www.gnu.org/licenses/	*
 ***********************************************************************/


using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public class LargeScreenShot 
{
	static int id = 0;

	static LargeScreenShot(){

	}

	#if UNITY_EDITOR
	[MenuItem("TouchOrchestra/Screen/1x")]
	private static void Screen1 ()
	{
		 TakeLargeScreenshot(1);
	}

	[MenuItem("TouchOrchestra/Screen/2x")]
	private static void Screen2 ()
	{
		TakeLargeScreenshot(2);
	}

	[MenuItem("TouchOrchestra/Screen/3x")]
	private static void Screen3 ()
	{
		TakeLargeScreenshot(3);
	}

	[MenuItem("TouchOrchestra/Screen/4x")]
	private static void Screen4 ()
	{
		TakeLargeScreenshot(4);
	}

	[MenuItem("TouchOrchestra/Screen/5x")]
	private static void Screen5 ()
	{
		TakeLargeScreenshot(5);
	}
	#endif

	public static void TakeLargeScreenshot (int scale)
	{
		Application.CaptureScreenshot ("screenshot_" + scale + "_" + (id++) + ".png", scale);

	}

}