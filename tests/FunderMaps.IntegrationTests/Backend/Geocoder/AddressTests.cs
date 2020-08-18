﻿using FunderMaps.Core.Entities;
using FunderMaps.IntegrationTests.Faker;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace FunderMaps.IntegrationTests.Backend.Geocoder
{
    // TODO: Use DTO
    public class AddressTests : IClassFixture<AuthBackendWebApplicationFactory>
    {
        private readonly AuthBackendWebApplicationFactory _factory;

        public AddressTests(AuthBackendWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAddressByIdReturnSingleAddress()
        {
            // Arrange
            var expectedAddress = new AddressFaker().Generate();
            var client = _factory
                .WithAuthentication()
                .WithAuthenticationStores()
                .WithDataStoreList(expectedAddress)
                .CreateClient();

            // Act
            var response = await client.GetAsync($"api/address/{expectedAddress.Id}");
            var returnObject = await response.Content.ReadFromJsonAsync<Address>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedAddress.Id, returnObject.Id);
        }

        internal static readonly IList<Address> Addresses = new List<Address>
        {
            new Address
            {
                Id = "gfm-0000a5dce4114767a57d8facaddf53d6",
                BuildingNumber = "102 C ",
                PostalCode = "1017PP",
                Street = "Leidsekade",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0363200012151882",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-0a687b09a74a43b1b60b8a649a4d2c1b",
                BuildingNumber = "9",
                PostalCode = "5707KM",
                Street = "Mr. Rietkerklaan",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0794200000077355",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67f8fd79bf3a461c923e7c24c0fb479f",
                BuildingNumber = "725",
                PostalCode = "8446DR",
                Street = "Heerenhage",
                IsActive = false,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0074200000377867",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67f8fe46717947d8bcd22bee5ca092ae",
                BuildingNumber = "116",
                PostalCode = "4621EV",
                Street = "Beukstraat",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0748200000039597",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67f9b843843846a89c5700f048dfe5a0",
                BuildingNumber = "8 A",
                PostalCode = "1016PE",
                Street = "Tweede Rozendwarsstraat",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0363200012071002",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67fcde96c989437f95dd4a903681661e",
                BuildingNumber = "3",
                PostalCode = "1073XN",
                Street = "Tweede Jacob van Campenstraat",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0363200000423238",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67fce470f8e343bba28b443faf5495cb",
                BuildingNumber = "34 A 02",
                PostalCode = "6227BZ",
                Street = "Einsteinstraat",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0935200000097589",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67fcf92aede845258d80acec7df12f14",
                BuildingNumber = "279 C ",
                PostalCode = "3028AD",
                Street = "Franselaan",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.0599200000270883",
                ExternalSource = "bag",
            },
            new Address
            {
                Id = "gfm-67fd1215c9224ef1b6eb2af0b75b9ccd",
                BuildingNumber = "15  ",
                PostalCode = "2271TT",
                Street = "van Deventerlaan",
                IsActive = true,
                ExternalId = "NL.IMBAG.NUMMERAANDUIDING.1916200000035516",
                ExternalSource = "bag",
            },
        };

        [Theory]
        [InlineData("kade")]
        [InlineData("4621E")]
        [InlineData("5707KM")]
        [InlineData("straat")]
        [InlineData("hage")]
        public async Task GetAllAddressByQueryReturnMatchingAddressList(string query)
        {
            // Arrange
            var client = _factory
                .WithAuthentication()
                .WithAuthenticationStores()
                .WithDataStoreList(Addresses)
                .CreateClient();

            // Act
            var response = await client.GetAsync($"api/address/suggest?query={query}");
            var returnList = await response.Content.ReadFromJsonAsync<List<Address>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(returnList.Count > 0);
        }

        [Fact]
        public async Task GetLimitedAddressByQueryReturnMatchingAddressList()
        {
            // Arrange
            var client = _factory
                .WithAuthentication()
                .WithAuthenticationStores()
                .WithDataStoreList(Addresses)
                .CreateClient();

            // Act
            var response = await client.GetAsync($"api/address/suggest?query=laan&limit=2");
            var returnList = await response.Content.ReadFromJsonAsync<List<Address>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, returnList.Count);
        }
    }
}