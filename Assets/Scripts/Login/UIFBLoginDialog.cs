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

public class UIFBLoginDialog : MonoBehaviour 
{
	[SerializeField]
	private tk2dUIItem loginButton;

	private void Awake()
	{
		#if FB_LOGIN

		Assert.IsAssigned(loginButton);

		loginButton.OnClick += OnClick;
		FB.Init(SetInit, OnHideUnity);

		#else

		OnLoggedIn();

		#endif
	}


	#if FB_LOGIN

	private void SetInit()
	{
		enabled = true;
		if (FB.IsLoggedIn)
		{
			Debug.Log("SetInit()");
			OnLoggedIn();
		}
	}

	private void OnClick()
	{
		FB.Login("email", LoginCallback);
	}

	private void OnHideUnity(bool isGameShown)
	{
		Debug.Log("OnHideUnity()");
	}

	private void LoginCallback(FBResult result)
	{
		if (FB.IsLoggedIn)
		{
			OnLoggedIn();
		}
	}

	#endif

	private void OnLoggedIn()
	{
		//TODO: zmena sceny
		Application.LoadLevel("UIMain");
	}

}
