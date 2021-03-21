using Bogus;
using Bogus.Extensions;
using FunderMaps.AspNetCore.DataTransferObjects;
using FunderMaps.Core.Entities;
using FunderMaps.Testing.Extensions;
using System;

namespace FunderMaps.Testing.Faker
{
    /// <summary>
    ///     Faker for <see cref="Organization"/>.
    /// </summary>
    public class OrganizationDtoFaker : Faker<OrganizationDto>
    {
        /// <summary>
        ///     Create new instance.
        /// </summary>
        public OrganizationDtoFaker()
        {
            RuleFor(f => f.Id, f => f.Random.Uuid());
            RuleFor(f => f.Name, f => f.Company.CompanyName());
            RuleFor(f => f.Email, (f, o) => f.Internet.Email(provider: o.Name));
            RuleFor(f => f.PhoneNumber, f => f.Phone.PhoneNumber("###########").OrNull(f, .3f));
            RuleFor(f => f.BrandingLogo, f => f.Internet.RemoteFileWithSecureUrl());
            RuleFor(f => f.HomeStreet, f => f.Address.StreetName());
            RuleFor(f => f.HomeAddressNumber, f => Convert.ToInt32(f.Address.BuildingNumber()));
            RuleFor(f => f.HomeAddressNumberPostfix, f => f.Address.SecondaryAddress());
            RuleFor(f => f.HomeCity, f => f.Address.City());
            RuleFor(f => f.HomePostbox, f => f.Address.ZipCode());
            RuleFor(f => f.HomeZipcode, f => f.Address.ZipCode());
        }
    }
}
