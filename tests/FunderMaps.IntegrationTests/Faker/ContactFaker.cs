﻿using Bogus;
using Bogus.Extensions;
using FunderMaps.Core.Entities;

namespace FunderMaps.IntegrationTests.Faker
{
    public class ContactFaker : Faker<Contact>
    {
        public ContactFaker()
        {
            RuleFor(f => f.Email, f => f.Internet.Email());
            RuleFor(f => f.Name, f => f.Person.FullName.OrNull(f, .3f));
            RuleFor(f => f.PhoneNumber, f => f.Phone.PhoneNumber("###########").OrNull(f, .3f));
        }
    }
}
