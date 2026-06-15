namespace ASB.Admin.v1.Response
{
    /// <summary>
    /// Represents a country with its code and name. This model is used in API responses when retrieving country information.
    /// </summary>
    public class CountryResponse
    {
        /// <summary>
        /// The ISO 3166-1 alpha-2 country code (e.g., "US" for United States).
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// The full name of the country (e.g., "United States").
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The market associated with the country (e.g., "North America").
        /// </summary>
        public required string Market { get; set; }

        /// <summary>
        /// The unique identifier for the country in the database. This is used internally and may not be exposed in all API responses.
        /// </summary>
        public int Id { get; set; }
    }
}