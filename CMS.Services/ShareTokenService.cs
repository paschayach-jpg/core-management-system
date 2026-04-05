using System;
using System.Security.Cryptography;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IShareTokenService: generates secure tokens, hashes passwords using bcrypt,
    /// and manages token lifecycle transitions.
    /// Satisfies Requirements 2.1, 7.1, 7.3.
    /// </summary>
    public class ShareTokenService : IShareTokenService
    {
        private readonly IShareTokenRepository _shareTokenRepository;
        private readonly ILoggingService _loggingService;

        public ShareTokenService(IShareTokenRepository shareTokenRepository, ILoggingService loggingService)
        {
            if (shareTokenRepository == null) throw new ArgumentNullException("shareTokenRepository");
            if (loggingService == null) throw new ArgumentNullException("loggingService");

            _shareTokenRepository = shareTokenRepository;
            _loggingService = loggingService;
        }

        // -----------------------------------------------------------------------
        // GenerateToken (Req 2.1)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates a unique GUID-based token using cryptographically secure randomness.
        /// Uses System.Security.Cryptography.RNGCryptoServiceProvider for GUID generation.
        /// Validates: Requirements 2.1
        /// </summary>
        public Guid GenerateToken()
        {
            try
            {
                // Use RNGCryptoServiceProvider for cryptographically secure random bytes
                using (var rng = new RNGCryptoServiceProvider())
                {
                    byte[] guidBytes = new byte[16];
                    rng.GetBytes(guidBytes);
                    
                    // Create GUID from random bytes
                    var token = new Guid(guidBytes);
                    
                    _loggingService.Debug(string.Format(
                        "ShareTokenService.GenerateToken: Generated token {0}", token));
                    
                    return token;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.GenerateToken FAILED: {0}", ex.Message), ex);
                throw;
            }
        }

        // -----------------------------------------------------------------------
        // ValidateToken
        // -----------------------------------------------------------------------

        /// <summary>
        /// Validates that a token exists and is properly formatted.
        /// </summary>
        public bool ValidateToken(Guid token)
        {
            try
            {
                if (token == Guid.Empty)
                {
                    _loggingService.Warn("ShareTokenService.ValidateToken: Token is empty GUID");
                    return false;
                }

                var shareToken = _shareTokenRepository.GetByTokenValue(token);
                bool isValid = shareToken != null;

                _loggingService.Debug(string.Format(
                    "ShareTokenService.ValidateToken: Token {0} is {1}", token, isValid ? "valid" : "invalid"));

                return isValid;
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.ValidateToken FAILED for token {0}: {1}", token, ex.Message), ex);
                return false;
            }
        }

        // -----------------------------------------------------------------------
        // GetTokenInfo
        // -----------------------------------------------------------------------

        /// <summary>
        /// Retrieves token information including status and lifecycle dates.
        /// </summary>
        public ShareToken GetTokenInfo(Guid token)
        {
            try
            {
                if (token == Guid.Empty)
                    throw new ArgumentException("Token cannot be empty GUID", "token");

                var shareToken = _shareTokenRepository.GetByTokenValue(token);

                if (shareToken == null)
                {
                    _loggingService.Warn(string.Format(
                        "ShareTokenService.GetTokenInfo: Token {0} not found", token));
                    return null;
                }

                _loggingService.Debug(string.Format(
                    "ShareTokenService.GetTokenInfo: Retrieved token {0} with status {1}", token, shareToken.Status));

                return shareToken;
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.GetTokenInfo FAILED for token {0}: {1}", token, ex.Message), ex);
                throw;
            }
        }

        // -----------------------------------------------------------------------
        // HashPassword (Req 7.1)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Hashes a password using PBKDF2 with SHA256.
        /// Uses 10000 iterations for security.
        /// Validates: Requirements 7.1
        /// </summary>
        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("Password cannot be null or empty", "password");

                // Generate a random salt
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }

                // Use PBKDF2 with SHA256 for password hashing
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
                {
                    byte[] hash = pbkdf2.GetBytes(32);
                    
                    // Combine salt and hash for storage
                    byte[] hashWithSalt = new byte[salt.Length + hash.Length];
                    Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
                    Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);
                    
                    string result = Convert.ToBase64String(hashWithSalt);

                    _loggingService.Debug("ShareTokenService.HashPassword: Password hashed successfully using PBKDF2");

                    return result;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.HashPassword FAILED: {0}", ex.Message), ex);
                throw;
            }
        }

        // -----------------------------------------------------------------------
        // VerifyPassword (Req 7.3)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies a password against a PBKDF2 hash using constant-time comparison.
        /// Validates: Requirements 7.3
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    _loggingService.Warn("ShareTokenService.VerifyPassword: Password is null or empty");
                    return false;
                }

                if (string.IsNullOrEmpty(hash))
                {
                    _loggingService.Warn("ShareTokenService.VerifyPassword: Hash is null or empty");
                    return false;
                }

                // Decode the stored hash
                byte[] hashWithSalt = Convert.FromBase64String(hash);
                
                // Extract salt (first 16 bytes)
                byte[] salt = new byte[16];
                Array.Copy(hashWithSalt, 0, salt, 0, 16);
                
                // Extract stored hash (remaining bytes)
                byte[] storedHash = new byte[hashWithSalt.Length - 16];
                Array.Copy(hashWithSalt, 16, storedHash, 0, storedHash.Length);
                
                // Compute hash of provided password with same salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
                {
                    byte[] computedHash = pbkdf2.GetBytes(32);
                    
                    // Constant-time comparison
                    bool isValid = ConstantTimeComparison(computedHash, storedHash);

                    _loggingService.Debug(string.Format(
                        "ShareTokenService.VerifyPassword: Password verification result: {0}", isValid));

                    return isValid;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.VerifyPassword FAILED: {0}", ex.Message), ex);
                return false;
            }
        }

        /// <summary>
        /// Performs constant-time comparison of two byte arrays to prevent timing attacks.
        /// </summary>
        private bool ConstantTimeComparison(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        // -----------------------------------------------------------------------
        // ActivateToken
        // -----------------------------------------------------------------------

        /// <summary>
        /// Transitions a token from Pending to Active status.
        /// Sets ActivatedDate to current timestamp.
        /// </summary>
        public void ActivateToken(Guid token, UserContext user)
        {
            try
            {
                if (token == Guid.Empty)
                    throw new ArgumentException("Token cannot be empty GUID", "token");
                if (user == null)
                    throw new ArgumentNullException("user");

                var shareToken = _shareTokenRepository.GetByTokenValue(token);
                if (shareToken == null)
                    throw new InvalidOperationException(string.Format("Token {0} not found", token));

                if (shareToken.Status != "Pending")
                    throw new InvalidOperationException(string.Format(
                        "Cannot activate token with status '{0}'. Only Pending tokens can be activated.", shareToken.Status));

                shareToken.Status = "Active";
                shareToken.ActivatedDate = DateTime.Now;

                _shareTokenRepository.Update(shareToken);

                _loggingService.Info(string.Format(
                    "ShareTokenService.ActivateToken: Token {0} activated by user {1}", token, user.ADUserName));
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.ActivateToken FAILED for token {0}: {1}", token, ex.Message), ex);
                throw;
            }
        }

        // -----------------------------------------------------------------------
        // ExpireToken
        // -----------------------------------------------------------------------

        /// <summary>
        /// Transitions a token to Expired status.
        /// Sets ExpiryDate to current timestamp.
        /// </summary>
        public void ExpireToken(Guid token, UserContext user)
        {
            try
            {
                if (token == Guid.Empty)
                    throw new ArgumentException("Token cannot be empty GUID", "token");
                if (user == null)
                    throw new ArgumentNullException("user");

                var shareToken = _shareTokenRepository.GetByTokenValue(token);
                if (shareToken == null)
                    throw new InvalidOperationException(string.Format("Token {0} not found", token));

                shareToken.Status = "Expired";
                shareToken.ExpiryDate = DateTime.Now;

                _shareTokenRepository.Update(shareToken);

                _loggingService.Info(string.Format(
                    "ShareTokenService.ExpireToken: Token {0} expired by user {1}", token, user.ADUserName));
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.ExpireToken FAILED for token {0}: {1}", token, ex.Message), ex);
                throw;
            }
        }

        // -----------------------------------------------------------------------
        // DisableToken
        // -----------------------------------------------------------------------

        /// <summary>
        /// Transitions a token to Disabled status.
        /// Sets DisabledDate and DisabledReason.
        /// </summary>
        public void DisableToken(Guid token, string reason, UserContext user)
        {
            try
            {
                if (token == Guid.Empty)
                    throw new ArgumentException("Token cannot be empty GUID", "token");
                if (user == null)
                    throw new ArgumentNullException("user");

                var shareToken = _shareTokenRepository.GetByTokenValue(token);
                if (shareToken == null)
                    throw new InvalidOperationException(string.Format("Token {0} not found", token));

                shareToken.Status = "Disabled";
                shareToken.DisabledDate = DateTime.Now;
                shareToken.DisabledByUserID = user.UserID;
                shareToken.DisabledReason = reason;

                _shareTokenRepository.Update(shareToken);

                _loggingService.Info(string.Format(
                    "ShareTokenService.DisableToken: Token {0} disabled by user {1}. Reason: {2}", 
                    token, user.ADUserName, reason));
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.DisableToken FAILED for token {0}: {1}", token, ex.Message), ex);
                throw;
            }
        }

        // -----------------------------------------------------------------------
        // RevokeToken
        // -----------------------------------------------------------------------

        /// <summary>
        /// Transitions a token to Revoked status.
        /// </summary>
        public void RevokeToken(Guid token, UserContext user)
        {
            try
            {
                if (token == Guid.Empty)
                    throw new ArgumentException("Token cannot be empty GUID", "token");
                if (user == null)
                    throw new ArgumentNullException("user");

                var shareToken = _shareTokenRepository.GetByTokenValue(token);
                if (shareToken == null)
                    throw new InvalidOperationException(string.Format("Token {0} not found", token));

                shareToken.Status = "Revoked";

                _shareTokenRepository.Update(shareToken);

                _loggingService.Info(string.Format(
                    "ShareTokenService.RevokeToken: Token {0} revoked by user {1}", token, user.ADUserName));
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "ShareTokenService.RevokeToken FAILED for token {0}: {1}", token, ex.Message), ex);
                throw;
            }
        }
    }
}
