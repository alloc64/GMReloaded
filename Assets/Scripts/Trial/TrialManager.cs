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
using System;

#if NETFX_CORE
using Windows.ApplicationModel.Store;
using Windows.UI.Core;

#if INAPP_DEBUG
using CurrApp = Windows.ApplicationModel.Store.CurrentAppSimulator;
#else
using CurrApp = Windows.ApplicationModel.Store.CurrentApp;
#endif

#endif

namespace GMReloaded.Trial
{
	public class TrialManager : MonoSingletonPersistent<TrialManager> 
	{
		#if !(UNITY_METRO && NETFX_CORE)
		private bool _isLocked = true;
		#endif

		public bool isLocked
		{
			get
			{
				#if UNITY_METRO && NETFX_CORE

				try
				{
					if(licenseInformation == null)
						return true;

					if (licenseInformation.IsActive)
					{
						return licenseInformation.IsTrial;
					}
					else
					{
						return true;
					}
				}
				catch(Exception e)
				{
					return true;
				}

				#else

				return _isLocked;

				#endif
			}
		}

		public int remainingDays
		{
			get
			{ 
				int days = 0;

                #if NETFX_CORE

				try
				{
					if (licenseInformation != null && licenseInformation.IsActive && licenseInformation.IsTrial)
					{
						days = (licenseInformation.ExpirationDate - DateTime.Now).Days;
					}
				}
				catch(Exception e)
				{

				}

				#endif
                return days;
			}
		}

		#if NETFX_CORE

		private LicenseInformation _licenseInformation = null;
		private LicenseInformation licenseInformation
		{
			get
			{
				if(_licenseInformation == null)
				{
					try
					{
						_licenseInformation = CurrApp.LicenseInformation;
					}
					catch(Exception e)
					{
						Debug.LogError("Failed to get CurrApp.LicenseInformation");
						Debug.LogException(e);
						_licenseInformation = null;
					}
				}

				return _licenseInformation;
			}
		}

        public void BuyFullLicense(Action<string> onBought, Action onUserStopped, Action onFailed)
        {
            try
			{
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(new CoreDispatcherPriority(), () =>
                { 
                    _BuyFullLicense(onBought, onUserStopped, onFailed);
                });
            }        
            catch(Exception e)
            {
				Debug.LogError("Error occured while purchasing app...");
				Debug.LogException(e);

                if(onFailed != null)
					Loom.QueueOnMainThread(onFailed);
            }
        }

		private async void _BuyFullLicense(Action<string> onBought, Action onUserStopped, Action onFailed)
		{
			try
			{
				if(licenseInformation == null)
				{
					Debug.Log("licenseInformation == null");

					if(onFailed != null)
						Loom.QueueOnMainThread(onFailed);

					return;
				}

				Debug.Log("Trying to get full license...");

				try
				{
					string receipt = await CurrApp.RequestAppPurchaseAsync(true);

					if (!licenseInformation.IsTrial && licenseInformation.IsActive)
					{
						if(onBought != null)
							Loom.QueueOnMainThread(() => onBought(receipt));
					}
					else
					{
						if(onUserStopped != null)
							Loom.QueueOnMainThread(onUserStopped);
					}
				}
				catch (Exception)
				{
					Debug.LogError("The upgrade transaction failed. You still have a trial license for this app.");

					if(onFailed != null)
						Loom.QueueOnMainThread(onFailed);
				}
			}
			catch(Exception e)
			{
				Debug.LogError("Error occured while purchasing app...");
				Debug.LogException(e);

				if(onFailed != null)
					Loom.QueueOnMainThread(onFailed);
			}
		}
#else

        public void BuyFullLicense(Action<string> onBought, Action onUserStopped, Action onFailed)
		{
			Debug.Log("BuyFullLicense mock");

			Timer.DelayAsync(2f, () => 
			{ 
				_isLocked = false;

				onBought("123"); 
			});
		}

		#endif
	}
}
