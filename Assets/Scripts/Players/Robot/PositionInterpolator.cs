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
	public interface IInterpolationState
	{
		double timestamp { get; set; }
	}

	public interface IInterpolationCallback<T> where T : IInterpolationState
	{
		void OnUpdateInterpolated(T lhs, T rhs, float dt);
		void OnUpdateLerped(T latest, float dt);
	}

	public class PositionInterpolator<T> where T : IInterpolationState
	{
		public const double interpolationBackTime = Config.interpolationBackTime;

		protected T[] bufferedInterpState = new T[20];
		protected int timestampCount;

		private IInterpolationCallback<T> callback;

		private float fraction = 0f;

		public PositionInterpolator(IInterpolationCallback<T> callback)
		{
			ResetData();
			this.callback = callback;
		}

		protected virtual void ResetData()
		{
			bufferedInterpState = new T[20];
			timestampCount = 0;
		}

		public void ReadData(T state)
		{
			for (int i = bufferedInterpState.Length - 1; i >= 1; i--)
				bufferedInterpState[i] = bufferedInterpState[i - 1];
			
			bufferedInterpState[0] = state;

			fraction = 0;        

			timestampCount = Mathf.Min(timestampCount + 1, bufferedInterpState.Length);

			// Check integrity, lowest numbered state in the buffer is newest and so on
			for (int i = 0; i < timestampCount - 1; i++)
			{
				if (bufferedInterpState[i].timestamp < bufferedInterpState[i + 1].timestamp)
					Debug.Log("State inconsistent");
			}
		}

		public void Interpolate(float dt)
		{
			double currentTime = PhotonNetwork.time;
			double interpolationTime = currentTime - interpolationBackTime;

			if (bufferedInterpState[0].timestamp > interpolationTime)
			{
				for (int i = 0; i < timestampCount; i++)
				{
					if (bufferedInterpState[i].timestamp <= interpolationTime || i == timestampCount - 1)
					{
						T rhs = bufferedInterpState[Mathf.Max(i - 1, 0)];
						T lhs = bufferedInterpState[i];

						double length = rhs.timestamp - lhs.timestamp;
						float t = 0.0F;

						if (length > 0.0001)
							t = (float)((interpolationTime - lhs.timestamp) / length);

						OnUpdateInterpolated(lhs, rhs, t);

						return;
					}
				}
			}
			else
			{
				//Debug.Log("using extrapolation " + (bufferedInterpState[0].timestamp - interpolationTime));

				fraction = fraction + Time.deltaTime * 9f;

				OnUpdateLerped(bufferedInterpState[0], fraction);
			}
		}

		protected virtual void OnUpdateInterpolated(T lhs, T rhs, float dt)
		{
			if(callback != null)
				callback.OnUpdateInterpolated(lhs, rhs, dt);
		}

		protected virtual void OnUpdateLerped(T latest, float dt)
		{
			if(callback != null)
				callback.OnUpdateLerped(latest, dt);	
		}
	}
	
}
