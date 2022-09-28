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
using System;
using System.Collections;

public class Timer 
{
	public Timer()
	{
		time = 0.0f;
	}	

	public static float unscaledDeltaTime
	{
		get
		{ 
			if(Time.timeScale == 0.0f)
				return 0.0f;

			return Time.deltaTime / Time.timeScale;
		}
	}

	public float time { get; set; }

	public bool started = false;
	private bool _swap = false;

	public void Delay(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		if(time < _maxTime)
		{
			time += Time.deltaTime;

			if(_onWait != null)
				_onWait(time / _maxTime);
		}
		else
		{
			time = 0.0f;

			if(_onTimerAction != null)
				_onTimerAction();
		}
	}

	public void DelayFixed(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		if(time < _maxTime)
		{
			time += Time.fixedDeltaTime;

			if(_onWait != null)
				_onWait(time / _maxTime);
		}
		else
		{
			time = 0.0f;

			if(_onTimerAction != null)
				_onTimerAction();
		}
	}

	public void DelayIndependent(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		if(time < _maxTime)
		{
			time += Independent.Timer.deltaTime;

			if(_onWait != null)
				_onWait(time / _maxTime);
		}
		else
		{
			time = 0.0f;

			if(_onTimerAction != null)
				_onTimerAction();
		}
	}

	public float LoopIndependent(float _start, float _end, float _speed)
	{
		if(!started)
		{
			time = _start;
			started = true;
		}

		if(_swap)
		{			
			if(time > _start)
			{
				time -= Independent.Timer.deltaTime * _speed;
			}
			else
			{
				_swap = false;
			}
		}
		else
		{
			if(time < _end)
			{
				time += Independent.Timer.deltaTime * _speed;
			}
			else
			{
				_swap = true;
			}
		}

		return time;
	}

	public float Loop(float _start, float _end, float _speed)
	{
		if(!started)
		{
			time = _start;
			started = true;
		}

		if(_swap)
		{			
			if(time > _start)
			{
				time -= Time.deltaTime * _speed;
			}
			else
			{
				_swap = false;
			}
		}
		else
		{
			if(time < _end)
			{
				time += Time.deltaTime * _speed;
			}
			else
			{
				_swap = true;
			}
		}

		return time;
	}

	public void Loop(float _start, float _end, float _speedIn, float _speedOut, Action<float> _onLoopAction)
	{
		if(!started)
		{
			time = _start;
			started = true;
		}

		if(_swap)
		{			
			if(time > _start)
			{
				time -= Time.deltaTime * _speedOut;
			}
			else
			{
				_swap = false;
			}
		}
		else
		{
			if(time < _end)
			{
				time += Time.deltaTime * _speedIn;
			}
			else
			{
				_swap = true;
			}
		}

		if(_onLoopAction != null)
			_onLoopAction(time);
	}

	public static UnityEngine.Coroutine DelayAsync(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		return Independent.Coroutine.GetInstance().ProcessCoroutine(_DelayAsync(_maxTime, _onTimerAction, _onWait));
	}

	private static IEnumerator _DelayAsync(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		float time = 0.0f;


		while(time < _maxTime)
		{
			time += Time.deltaTime;

			if(_onWait != null)
				_onWait(time / _maxTime);

			yield return null;
		}

		if(_onTimerAction != null)
			_onTimerAction();
	}

	public static void DelayAsyncIndependent(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		Independent.Coroutine.GetInstance().ProcessCoroutine(_DelayAsyncIndependent(_maxTime, _onTimerAction, _onWait));
	}

	private static IEnumerator _DelayAsyncIndependent(float _maxTime, Action _onTimerAction, Action<float> _onWait = null)
	{
		float time = 0.0f;


		while(time < _maxTime)
		{
			time += Independent.Timer.deltaTime;

			if(_onWait != null)
				_onWait(time / _maxTime);

			yield return null;
		}

		if(_onTimerAction != null)
			_onTimerAction();
	}

	public static void Stop(Coroutine c)
	{
		Independent.Coroutine.GetInstance().StopCoroutine(c);
	}
}
