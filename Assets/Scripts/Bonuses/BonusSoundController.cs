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
using System.Collections.Generic;

namespace GMReloaded.Bonuses
{
	public class BonusSoundController
	{
		private class BonusSound
		{
			private Sound bonusSound;
			private int usedCount;

			protected GMReloaded.SoundManager snd
			{
				get { return GMReloaded.SoundManager.GetInstance(); }
			}

			public BonusSound(string sndId)
			{
				bonusSound = snd.Load(sndId);

				if(bonusSound != null)
					bonusSound.SetLooping(true);
			}

			public void Play(Transform parent)
			{
				if(bonusSound != null)
				{
					if(usedCount < 1)
						bonusSound.Play(parent);

					usedCount++;
				}
			}

			public void Stop()
			{
				if(bonusSound != null)
				{
					usedCount--;

					if(usedCount <= 0)
					{
						bonusSound.Stop();
					}
				}
			}
		}

		private Dictionary<string, BonusSound> soundUsage = new Dictionary<string, BonusSound>();

		private BonusSound GetSound(string sndId)
		{
			BonusSound bonusSound = null;

			soundUsage.TryGetValue(sndId, out bonusSound);

			if(bonusSound == null)
			{
				bonusSound = new BonusSound(sndId);
				soundUsage[sndId] = bonusSound;
			}

			return bonusSound;
		}

		public void Play(string sndId, Transform parent)
		{
			BonusSound bonusSound = GetSound(sndId);

			if(bonusSound != null)
				bonusSound.Play(parent);
		}

		public void Stop(string sndId)
		{
			BonusSound bonusSound = GetSound(sndId);

			if(bonusSound != null)
				bonusSound.Stop();
		}
	}
	
}
