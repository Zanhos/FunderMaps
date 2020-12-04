using FunderMaps.Core.Types;

namespace FunderMaps.Core.Interfaces
{
    /// <summary>
    ///     Identify the geocoder datasource from the input.
    /// </summary>
    public interface IGeocoderParser
    {
        /// <summary>
        ///     Identify the geocoder datasource from the input.
        /// </summary>
        /// <param name="input">Input identifier.</param>
        /// <returns>Geocoder datasource via <see cref="GeocoderDatasource"/>.</returns>
        GeocoderDatasource FromIdentifier(string input);
    }
}
