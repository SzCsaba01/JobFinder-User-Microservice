using User.Data.Contracts.Helpers.DTO.Location;

namespace User.Services.Contracts;
public interface ILocationCommunicationService
{
    public Task<ICollection<LocationDto>> GetCountriesByNamesAsync(ICollection<string> countryNames);
    public Task<ICollection<LocationDto>> GetStatesByNamesAsync(ICollection<string> stateNames);
    public Task<ICollection<LocationDto>> GetCitiesByCityAndCountryNames(ICollection<CountryStateCityRegionDto> countryStateCityRegions);
    public Task<ICollection<LocationDto>> GetStatesByCountryAndStateNamesAsync(ICollection<CountryStateCityRegionDto> countryStateCityRegions);
}
