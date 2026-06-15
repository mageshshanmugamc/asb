namespace ASB.Repositories.v1.Entities;

/// <summary>
/// Represents a country entity in the database. This class is used by the data access layer to interact with the countries table. It contains properties that correspond to the columns in the database and may include navigation properties for related entities if necessary.
/// </summary> <remarks>
/// The Country class is a simple representation of a country with its name, code, market, and a unique identifier. It is used to store and retrieve country information from the database, and it may be mapped to a CountryDto for use in the service layer and API responses.
/// </remarks>
/// <example>
/// For example, if the country is "United States", the Name property would be "United States", the Code property would be "US", the Market property might be "North America", and the Id property would be a unique integer assigned by the database.
/// </example> 
/// <seealso cref="ASB.Services.v1.Dtos.CountryDto"/>
/// <seealso cref="ASB.Admin.v1.Response.CountryResponse"/>
/// <seealso cref="ASB.Services.v1.Interfaces.ICountryService"/>
/// <seealso cref="ASB.Services.v1.Implementations.CountryService"/>
/// <seealso cref="ASB.Repositories.v1.Interfaces.ICountryRepository"/>
/// <seealso cref="ASB.Repositories.v1.Implementations.CountryRepository"/>
/// <seealso cref="ASB.Repositories.v1.Entities.Country"/>
/// <seealso cref="ASB.Repositories.v1.Entities.Country"/>
public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Market { get; set; } = string.Empty;
}