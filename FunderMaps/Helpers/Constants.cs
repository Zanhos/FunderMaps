﻿using System;
using Microsoft.AspNetCore.Identity;

namespace FunderMaps.Helpers
{
    /// <summary>
    /// Application constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default password policy.
        /// </summary>
        public static readonly PasswordOptions PasswordPolicy = new PasswordOptions
        {
            RequireDigit = false,
            RequireLowercase = false,
            RequireNonAlphanumeric = false,
            RequireUppercase = false,
            RequiredLength = 6,
            RequiredUniqueChars = 1,
        };

        /// <summary>
        /// Default lockout policy.
        /// </summary>
        public static readonly LockoutOptions LockoutOptions = new LockoutOptions
        {
            DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30),
            MaxFailedAccessAttempts = 10,
        };

        public const string AdministratorRole = "Administrator";
        public const string SuperuserRole = "Superuser";
        public const string VerifierRole = "Verifier";
        public const string WriterRole = "Writer";
        public const string ReaderRole = "Reader";
    }
}
