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

namespace GMReloaded.API
{
	public class Score : MonoSingletonPersistent<Score> 
	{
		// shoduji se s sloupcama v mysql + nektery z nich jsou ve steamu
		public const string Deaths = "deaths";
		public const string Kills = "kills";
		public const string Experience = "experience";
		public const string Rounds = "rounds";
		public const string RobotPoints = "robot_pts";

		private float dispatchTime = Config.Score.dispatchTime;

		private float dispatchTimer = 0f;

		private ScoreObserver _score = null;
		private ScoreObserver score
		{
			get
			{ 
				if(_score == null)
					_score = new ScoreObserver();

				return _score;
			}
		}

		#if STEAM_ENABLED
		private Steam.Steamworks steamworks { get { return Steam.Steamworks.Instance; } }
		private Steam.SteamStats steamworksStats { get { return Steam.Steamworks.Instance.stats; } }
		#endif

		protected override void Awake()
		{
			base.Awake();

			score.Init();

		}

		protected virtual void Update()
		{
			if(dispatchTimer < dispatchTime)
				dispatchTimer += Time.deltaTime;

			if(dispatchTimer >= dispatchTime)
			{
				Dispatch(true);

				dispatchTimer = 0f;
			}
		}

		private void OnApplicationQuit() 
		{
			Dispatch();
		}

		private void Dispatch(bool calledFromUpdate = false)
		{
			#if STEAM_ENABLED

			if(steamworks != null)
				steamworks.StoreStats();

			#endif

			score.Dispatch(!calledFromUpdate);
		}

		public void LogRelativeValue(string id, float value)
		{
			score.LogRelativeValue(id, value);
		}

		public void LogRelativeDeaths(float value)
		{
			score.LogRelativeValue(Score.Deaths, value);

			#if STEAM_ENABLED

			if(steamworksStats != null)
				steamworksStats.IncrementStat(Score.Deaths, ((int)value));
			
			#endif
		}

		public void LogRelativeKills(float value)
		{
			score.LogRelativeValue(Score.Kills, value);

			#if STEAM_ENABLED

			if(steamworksStats != null)
				steamworksStats.IncrementStat(Score.Kills, ((int)value));

			Steam.Steamworks.Instance.UpdateRelativeLeaderboardScore(Config.Steam.leaderboardId, (int)value);

			#endif
		}

		public void LogRelativeExperience(float value)
		{
			score.LogRelativeValue(Score.Experience, value);
		}

		public void LogRelativeRounds(float value)
		{
			score.LogRelativeValue(Score.Rounds, value);

			#if STEAM_ENABLED

			if(steamworksStats != null)
				steamworksStats.IncrementStat(Score.Rounds, ((int)value));

			#endif
		}

		public void LogRelativeRobotPoints(float value)
		{
			score.LogRelativeValue(Score.RobotPoints, value);

			#if STEAM_ENABLED

			//TODO: robotpoints v steam stats
			//if(steamworksStats != null)
			//	steamworksStats.IncrementStat(Score.Rounds, ((int)value));

			#endif
		}
	}
}
