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
using TouchOrchestra.Net;
using System.Collections.Generic;
using System;

namespace GMReloaded.API
{
	public class GMHTTP : HTTP
	{
		protected override System.Collections.IEnumerator WaitForRequest(WWW www, Action<WWW> OnResponse, Action<WWW> OnError)
		{
			float timeout = 20f;

			float timer = 0f; 
			bool failed = false;

			while(!www.isDone)
			{
				if(timer >= timeout && !failed)
				{ 
					failed = true; 

					Debug.Log(www.url + "\n" + www.error);

					if(OnError != null)
					{
						OnError(www);
					}
				}

				timer += Time.deltaTime;
				yield return null;
			}

			if(!failed)
			{
				if(www.error == null)
				{
					if(OnResponse != null)
					{
						OnResponse(www);
					}
				}
				else
				{
					Debug.Log(www.url + "\n" + www.error);

					if(OnError != null)
					{
						OnError(www);
					}
				}
			}
		}
	}
	
}