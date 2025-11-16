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
        public async  Task<string> GetCurrentLocation() {

			try
			{
				GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
				_cancelTokenSource = new CancellationTokenSource();
				Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
				return location.ToString();
			}
			catch (Exception ex)
			{
				//unable to get location
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
