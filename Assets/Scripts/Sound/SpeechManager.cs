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
	public class SpeechManager 
	{
		private SoundManager snd;

		private Sound lastPlayedSound = null;

		public SpeechManager(SoundManager snd) 
		{
			this.snd = snd;
		}

		public void Play(string id, float delay = -1)
		{
			if(lastPlayedSound != null && lastPlayedSound.isPlaying)
				return;

			if(delay > 0f)
				Timer.DelayAsync(delay, () => Play(id, -1));
			else
				lastPlayedSound = snd.Play(id);
		}

		private void Play(string [] randomIds, float delay = -1)
		{
			if(randomIds == null)
				return;

			Play(randomIds.GetRandom(), delay);
		}

		public void PlayBonus(GMReloaded.Bonuses.Bonus.Behaviour behaviour)
		{
			Play("Dabing_Bonus_" + behaviour);
		}

		public void Killed() { Play(new string[] { Config.Sounds.Dabing.youDied, Config.Sounds.Dabing.hahaha, Config.Sounds.Dabing.looser, /*Config.Sounds.Dabing.buuu,*/ Config.Sounds.Dabing.tryHarder }, 0.4f); }

		public void Suicide() { Play(new string[] { Config.Sounds.Dabing.whatHaveYouDone, Config.Sounds.Dabing.hahaha, Config.Sounds.Dabing.looser, /*Config.Sounds.Dabing.buuu*/ }, 0.6f); }
		 
		public void DoubleKill() { Play(Config.Sounds.Dabing.doubleKill); }

		public void TripleKill() { Play(Config.Sounds.Dabing.tripleKill); }

		public void MultiKill() { Play(Config.Sounds.Dabing.multiKill); }

		public void GrenadeMadness() { Play(Config.Sounds.Dabing.grenadeMadness); }
	}
}
