﻿using System.Threading.Tasks;
using FunderMaps.Core.Entities.Fis;
using FunderMaps.Extensions;
using FunderMaps.Interfaces;

namespace FunderMaps.Data.Repositories
{
    public class AddressRepository : EfRepository<FisDbContext, Address>, IAddressRepository
    {
        public AddressRepository(FisDbContext dbContext)
            : base(dbContext)
        {
        }

        public Task<Address> GetOrAddAsync(Address address)
            => _dbContext.Address.GetOrAddAsync(address, s => s.Id == address.Id ||
                    s.StreetName == address.StreetName &&
                    s.BuildingNumber == address.BuildingNumber &&
                    s.BuildingNumberSuffix == address.BuildingNumberSuffix);
    }
}