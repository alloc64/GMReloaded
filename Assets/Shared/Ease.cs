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

using Independent;
using UnityEngine;
using System;
using System.Collections;

namespace TouchOrchestra
{
	public enum EaseType 
	{
		None, 
		In, 
		Out, 
		InOut
	}

	public class Ease : MonoSingletonPersistent<Ease>
	{
		protected override void Awake()
		{
			base.Awake();

			Independent.Timer.GetInstance();
		}

		public static float EaseProcess(float t, EaseType easeType) 
		{
			if (easeType == EaseType.None)
				return t;
			else if (easeType == EaseType.In)
				return Mathf.Lerp(0.0f, 1.0f, 1.0f - Mathf.Cos(t * Mathf.PI * .5f));
			else if (easeType == EaseType.Out)
				return Mathf.Lerp(0.0f, 1.0f, Mathf.Sin(t * Mathf.PI * .5f));
			else
				return Mathf.SmoothStep(0.0f, 1.0f, t);
		}

		/// <summary>
		/// Ease.GetInstance().Alpha(1.0f, 0.2f, 0.5f, EaseType.In, (float _time) => SetAlpha(_time)); 
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="timer">Timer.</param>
		/// <param name="easeType">Ease type.</param>
		/// <param name="OnEase">On ease.</param>

		public void Alpha(float start, float end, float timer, EaseType easeType, Action<float> OnEase, Action OnEaseEnd = null, bool _scaledTime = false) 
		{
			StartCoroutine(_Alpha(start, end, timer, easeType, OnEase, OnEaseEnd, _scaledTime));
		}

		private IEnumerator _Alpha(float start, float end, float timer, EaseType easeType, Action<float> OnEase, Action OnEaseEnd, bool _scaledTime = false) 
		{
			if(OnEase == null)
				yield break;

			float t = 0.0f;

			while (t < 1.0f) 
			{
				t += (_scaledTime ? Time.deltaTime : Independent.Timer.deltaTime) * (1.0f / timer);

				OnEase(Mathf.Lerp(start, end, EaseProcess(t, easeType)));
				yield return null;
			}

			if(OnEaseEnd != null)
			{
				try
				{
					OnEaseEnd();
				}
				catch(Exception _e)
				{
					Debug.LogError("Caught exception + " + _e + " in method _Alpha!");
				}
			}
		}

		public void Colors(Color start, Color end, float timer, EaseType easeType, Action<Color> OnEase, Action OnEaseEnd = null) 
		{
			StartCoroutine(_Colors(start, end, timer, easeType, OnEase, OnEaseEnd));
		}

		private IEnumerator _Colors(Color start, Color end, float timer, EaseType easeType, Action<Color> OnEase, Action OnEaseEnd) 
		{
			if(OnEase == null)
				yield break;

			float t = 0.0f;

			while (t < 1.0f) 
			{
				t += Independent.Timer.deltaTime * (1.0f / timer);

				OnEase(Color.Lerp(start, end, EaseProcess(t, easeType)));
				yield return null;
			}

			if(OnEaseEnd != null)
			{
				try
				{
					OnEaseEnd();
				}
				catch(Exception _e)
				{
					Debug.LogError("Caught exception + " + _e + " in method _Colors!");
				}
			}
		}

		public UnityEngine.Coroutine Vector(Vector2 start, Vector2 end, float timer, EaseType easeType, Action<Vector2> OnEase, Action OnEaseEnd = null, bool _scaledTime = false)
		{
			return StartCoroutine(_Vector(start, end, timer, easeType, OnEase, OnEaseEnd, _scaledTime));
			                                 
		}

		private IEnumerator _Vector(Vector2 start, Vector2 end, float timer, EaseType easeType, Action<Vector2> OnEase, Action OnEaseEnd, bool _scaledTime)
		{
			if (OnEase == null)
				yield break;

			float t = 0.0f;

			float currentTime = Time.realtimeSinceStartup;
			float accumulator = 0.0f;

			float dt = 0.01f;

			while (t < 1.0f)
			{
				if (_scaledTime)
				{
					t += Time.deltaTime;
				}
				else
				{
					float newTime = Time.realtimeSinceStartup;

					float frameTime = newTime - currentTime;

					if(frameTime > 0.25f)
						frameTime = 0.25f;

					currentTime = newTime;

					accumulator += frameTime;

					while (accumulator >= dt)
					{
						OnEase(Vector2.Lerp(start, end, EaseProcess(t, easeType)));

						accumulator -= dt;
						t += dt * (1.0f / timer);

						if(t >= 1f)
							break;
					}
				}


				yield return null;
			}

			if (OnEaseEnd != null)
			{
				try
				{
					OnEaseEnd();
				}
				catch (Exception _e)
				{
					Debug.LogError("Caught exception + " + _e + " in method _Vector!");
				}
			}
		}

		public UnityEngine.Coroutine  Vector3(Vector3 start, Vector3 end, float timer, EaseType easeType, Action<Vector3> OnEase, Action OnEaseEnd = null, bool _scaledTime = false)
	    {
			return StartCoroutine(_Vector3(start, end, timer, easeType, OnEase, OnEaseEnd, _scaledTime));
	    }

	    private static IEnumerator _Vector3(Vector3 start, Vector3 end, float timer, EaseType easeType, Action<Vector3> OnEase, Action OnEaseEnd, bool _scaledTime)
	    {
	        if (OnEase == null)
	            yield break;

	        float t = 0.0f;

	        float currentTime = Time.realtimeSinceStartup;
	        float accumulator = 0.0f;

	        float dt = 0.01f;

	        while (t < 1.0f)
	        {
	            if (_scaledTime)
	            {
	                t += Time.deltaTime;
	            }
	            else
	            {
	                float newTime = Time.realtimeSinceStartup;
	                float frameTime = newTime - currentTime;
	                currentTime = newTime;

	                accumulator += frameTime;

	                while (accumulator >= dt)
	                {
	                    OnEase(UnityEngine.Vector3.Lerp(start, end, EaseProcess(t, easeType)));

	                    accumulator -= dt;
	                    t += dt * (1.0f / timer);
	                }
	            }

	            yield return null;
	        }

			OnEase(UnityEngine.Vector3.Lerp(start, end, EaseProcess(1f, easeType)));

	        if (OnEaseEnd != null)
	        {
	            try
	            {
	                OnEase(end);
	                OnEaseEnd();
	            }
	            catch (Exception _e)
	            {
	                Debug.LogError("Caught exception + " + _e + " in method _Vector!");
	            }
	        }
	    }


		public void InOutLoop(float start, float end, float timer, bool _expression, Predicate<bool> _predicate, EaseType easeType, Action<float> OnEaseIn, Action<float> OnEaseOut)
		{
			StartCoroutine(_InOut(start, end, timer, _expression, _predicate, easeType, OnEaseIn, OnEaseOut));
		}

		private IEnumerator _InOut(float start, float end, float timer, bool _expresion, Predicate<bool> _predicate, EaseType easeType, Action<float> OnEaseIn, Action<float> OnEaseOut) 
		{
			if(OnEaseIn == null || OnEaseOut == null)
				yield break;

			bool _lerpOut = Mathf.Sign(end - start) == -1;
			float t = 0.0f;

			while(_predicate(_expresion)) 
			{
				t += Independent.Timer.deltaTime * (1.0f / timer);

				if(t >= 0.9999f)
				{
					_lerpOut = !_lerpOut;
					t = 0.0f;
				}

				if(_lerpOut)
					OnEaseIn(Mathf.Lerp(start, end, EaseProcess(t, easeType)));
				else
					OnEaseOut(Mathf.Lerp(end, start, EaseProcess(t, easeType)));

				yield return null;
			}
		}
	}
}
