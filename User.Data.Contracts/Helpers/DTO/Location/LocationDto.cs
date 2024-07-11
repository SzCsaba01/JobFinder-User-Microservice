namespace User.Data.Contracts.Helpers.DTO.Location;
public class LocationDto
{
    public string? Region { get; set; }
    public string? SubRegion { get; set; }
    public string? City { get; set; }
    public string? StateCode { get; set; }
    public string? State { get; set; }
    public string? CountryIso2Code { get; set; }
    public string? CountryIso3Code { get; set; }
    public string? Country { get; set; }
}
