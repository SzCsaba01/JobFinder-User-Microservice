using User.Data.Access.Helpers;
using User.Data.Contracts.Helpers.DTO.Location;
using User.Services.Contracts;
using System.Text;
using System.Text.Json;

namespace User.Services.Business;
public class LocationCommunicationService : ILocationCommunicationService
{
    private readonly HttpClient _httpClient;

    public LocationCommunicationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ICollection<LocationDto>> GetCitiesByCityAndCountryNames(ICollection<CountryStateCityRegionDto> countryStateCityRegions)
    {
        var content = new StringContent(JsonSerializer.Serialize(countryStateCityRegions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(AppConstants.CITY_API_URL + "GetCitiesByCityAndCountryNames", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error while fetching cities by city and country names");
        }

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<ICollection<LocationDto>>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
    }

    public async Task<ICollection<LocationDto>> GetCountriesByNamesAsync(ICollection<string> countryNames)
    {
        var content = new StringContent(JsonSerializer.Serialize(countryNames), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(AppConstants.COUNTRY_API_URL + "GetCountriesByNames", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error while fetching countries by names");
        }

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<ICollection<LocationDto>>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
    }

    public async Task<ICollection<LocationDto>> GetStatesByNamesAsync(ICollection<string> stateNames)
    {
        var content = new StringContent(JsonSerializer.Serialize(stateNames), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(AppConstants.STATE_API_URL + "GetStatesByNames", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error while fetching states by names");
        }

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<ICollection<LocationDto>>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions);
    }
}
