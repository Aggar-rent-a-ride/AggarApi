using CORE.DTOs.Geoapify;
using CORE.Services.IServices;
using DATA.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class GeoapifyService : IGeoapifyService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<GeoapifyAddressRequest> _geoapifyAddressRequest;

        public GeoapifyService(HttpClient httpClient, IOptions<GeoapifyAddressRequest> geoapifyAddressRequest)
        {
            _httpClient = httpClient;
            _geoapifyAddressRequest = geoapifyAddressRequest;
        }
        public async Task<GeoapifyAddressResponse> GetAddressByLocationAsync(Location location)
        {
            var request = _geoapifyAddressRequest.Value;
            request.Lat = location.Latitude;
            request.Lon = location.Longitude;

            var response = await _httpClient.GetAsync(request.ToString());
            if (response.IsSuccessStatusCode)
            {

                var content = await response.Content.ReadAsStringAsync();
                JObject jsonObject = JObject.Parse(content);

                var result = jsonObject["results"].Children().ToList().FirstOrDefault();
                if (result == null)
                    return new GeoapifyAddressResponse();
                var address = result.ToObject<GeoapifyAddressResponse>();
                if (address != null)
                    return address;
            }
            return new GeoapifyAddressResponse();
        }
    }
}
