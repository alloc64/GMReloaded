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

namespace GMReloaded
{
	public class Layer 
	{
		public static int Default = LayerMask.NameToLayer("Default");
		public static int UI = LayerMask.NameToLayer("UI");

		public static int DestroyableEntity = LayerMask.NameToLayer("DestroyableEntity");

		public static int EntityContainerEmpty = LayerMask.NameToLayer("EntityContainerEmpty");
		public static int EntityContainerOccupied = LayerMask.NameToLayer("EntityContainerOccupied");

		public static int Bonus = LayerMask.NameToLayer("Bonus");
		public static int Player = LayerMask.NameToLayer("Player");
		public static int Ragdoll = LayerMask.NameToLayer("Ragdoll");
		public static int WeaponNotCollidingWithPlayer = LayerMask.NameToLayer("WeaponNotCollidingWithPlayer");
		public static int Knife = LayerMask.NameToLayer("Knife");//Not used
		public static int Taser = LayerMask.NameToLayer("Taser");
		public static int Decal = LayerMask.NameToLayer("Decal");
		public static int WeaponCollidingWithPlayer = LayerMask.NameToLayer("WeaponCollidingWithPlayer");

	}
}
