﻿using Laixer.Identity.Dapper;

namespace FunderMaps.Data
{
    /// <summary>
    /// Set the custom queries for FunderMapsUser and FunderMapsRole.
    /// </summary>
    public class FunderMapsCustomQuery : ICustomQueryRepository
    {
        /// <summary>
        /// Configure custom queries.
        /// </summary>
        /// <param name="queryRepository">Exiting query repository.</param>
        public void Configure(IQueryRepository queryRepository)
        {
            queryRepository.GetUserNameAsync = $@"
                SELECT email
                FROM application.user
                WHERE id=@Id
                LIMIT 1";

            queryRepository.GetNormalizedUserNameAsync = $@"
                SELECT normalized_email
                FROM application.user
                WHERE id=@Id
                LIMIT 1";

            queryRepository.SetNormalizedUserNameAsync = $@"
                UPDATE application.user
                SET normalized_email=@NormalizedUserName
                WHERE id=@Id";

            queryRepository.FindByNameAsync = @"
                SELECT *
                FROM application.user
                WHERE normalized_email=@NormalizedUserName";

            queryRepository.GetUserIdAsync = @"
                SELECT id
                FROM application.user
                WHERE normalized_email=@NormalizedEmail
                LIMIT 1";

            queryRepository.UpdateAsync = @"
                UPDATE application.user
                SET    email = @Email,
                       normalized_email = @NormalizedEmail,
                       email_confirmed = @EmailConfirmed,
                       password_hash = @PasswordHash,
                       phone_number = @PhoneNumber,
                       phone_number_confirmed = @PhoneNumberConfirmed,
                       two_factor_enabled = @TwoFactorEnabled,
                       lockout_end = @LockoutEnd,
                       lockout_enabled = @LockoutEnabled,
                       access_failed_count = @AccessFailedCount
                WHERE  id = @Id";

            queryRepository.AddToRoleAsync = $@"
                UPDATE application.user
                SET role = @Role
                WHERE id=@Id";

            queryRepository.GetRolesAsync = $@"
                SELECT role
                FROM application.user
                WHERE id=@Id";

            queryRepository.GetUsersInRoleAsync = $@"
                SELECT *
                FROM application.user
                WHERE role=@Role";

            queryRepository.RemoveFromRoleAsync = $@"
                UPDATE application.user
                SET role = 'user'
                WHERE id=@Id";
        }
    }
}