using System;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service for managing share tokens and password hashing.
    /// Handles token generation, validation, and lifecycle management.
    /// Satisfies Requirements 2.1, 7.1, 7.3.
    /// </summary>
    public interface IShareTokenService
    {
        /// <summary>
        /// Generates a unique GUID-based token using cryptographically secure randomness.
        /// Validates: Requirements 2.1
        /// </summary>
        Guid GenerateToken();

        /// <summary>
        /// Validates that a token exists and is properly formatted.
        /// </summary>
        bool ValidateToken(Guid token);

        /// <summary>
        /// Retrieves token information including status and lifecycle dates.
        /// </summary>
        ShareToken GetTokenInfo(Guid token);

        /// <summary>
        /// Hashes a password using bcrypt with work factor 12.
        /// Validates: Requirements 7.1
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a password against a bcrypt hash using constant-time comparison.
        /// Validates: Requirements 7.3
        /// </summary>
        bool VerifyPassword(string password, string hash);

        /// <summary>
        /// Transitions a token from Pending to Active status.
        /// Sets ActivatedDate to current timestamp.
        /// </summary>
        void ActivateToken(Guid token, UserContext user);

        /// <summary>
        /// Transitions a token to Expired status.
        /// Sets ExpiryDate to current timestamp.
        /// </summary>
        void ExpireToken(Guid token, UserContext user);

        /// <summary>
        /// Transitions a token to Disabled status.
        /// Sets DisabledDate and DisabledReason.
        /// </summary>
        void DisableToken(Guid token, string reason, UserContext user);

        /// <summary>
        /// Transitions a token to Revoked status.
        /// </summary>
        void RevokeToken(Guid token, UserContext user);
    }
}
