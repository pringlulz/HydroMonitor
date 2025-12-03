using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Services
{
    public class GeolocationService
    {
        //per MS documentation
        private CancellationTokenSource _cancelTokenSource;

		public async Task<Boolean> IsGeolocationEnabled()
		{
			//Microsoft.Maui.Devices.Sensors.Geolocation.
			PermissionStatus locationInUsePermission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
			if (locationInUsePermission == PermissionStatus.Granted)
			{
				return true;

			} 
			else
			{
				locationInUsePermission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
				if (locationInUsePermission == PermissionStatus.Granted)
				{
					return true;
				}
            }
			//#if ANDROID
			//			Microsoft.Maui.ApplicationModel.Platform.AppContext.Android.Locations.LocationManager.GpsProvider
			//#endif
			return false;
		}

        public async Task<Location> GetCurrentLocation() 
		{
			try
			{
				GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
				_cancelTokenSource = new CancellationTokenSource();
				Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
				return location;
				//
			}
			catch (Exception ex)
			{
                //unable to get location
                System.Diagnostics.Debug.WriteLine("Unable to get location", ex);
                throw;
			}

        }

		public void CancelRequest()
		{
			if ( _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested)
			{
				_cancelTokenSource.Cancel();
			}
		}


    }
}
