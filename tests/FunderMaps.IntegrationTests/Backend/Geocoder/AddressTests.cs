﻿using FunderMaps.AspNetCore.DataTransferObjects;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace FunderMaps.IntegrationTests.Backend.Geocoder
{
    public class AddressTests : IClassFixture<AuthBackendWebApplicationFactory>
    {
        private AuthBackendWebApplicationFactory Factory { get; }

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public AddressTests(AuthBackendWebApplicationFactory factory)
            => Factory = factory;

        [Theory]
        [InlineData("gfm-6d70df27db5347f88d932faa3a72d3b3", "gfm-6d70df27db5347f88d932faa3a72d3b3")]
        [InlineData("NL.IMBAG.NUMMERAANDUIDING.0503200000018943", "gfm-09e3b90972de425ea140ae27e49d60b5")]
        [InlineData("0503200000019289", "gfm-9ecb0f685cb84355ae464e2a358ac158")]
        public async Task GetAddressByIdReturnSingleAddress(string address, string expected)
        {
            // Arrange
            using var client = Factory.CreateClient();

            // Act
            var response = await client.GetAsync($"api/address/{address}");
            var returnObject = await response.Content.ReadFromJsonAsync<AddressBuildingDto>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expected, returnObject.AddressId);
        }
    }
}
