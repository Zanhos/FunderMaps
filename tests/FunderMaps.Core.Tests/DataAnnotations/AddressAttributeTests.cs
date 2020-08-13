﻿using FunderMaps.Core.DataAnnotations;
using Xunit;

namespace FunderMaps.Core.Tests.DataAnnotations
{
    public class AddressAttributeTests
    {
        [Fact]
        public void IsValidOnInput()
        {
            // Arrange
            var attr = new AddressAttribute();

            // Assert
            Assert.True(attr.IsValid("gfm-12345"));
        }

        [Fact]
        public void IsInvalidOnInput()
        {
            // Arrange
            var attr = new AddressAttribute();

            // Assert
            Assert.False(attr.IsValid("ggfm-12345"));
            Assert.False(attr.IsValid("gfm12345"));
            Assert.False(attr.IsValid("GFM-12345"));
        }
    }
}
