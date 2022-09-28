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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Independent
{
    public class Coroutine : MonoSingletonPersistent<Coroutine>
    {

        public UnityEngine.Coroutine ProcessCoroutine(IEnumerator iterationResult)
        {
            return StartCoroutine(ExecuteCoroutine(iterationResult));
        }

        private IEnumerator ExecuteCoroutine(IEnumerator iterationResult)
        {
            yield return StartCoroutine(iterationResult);
        }
    }

}
