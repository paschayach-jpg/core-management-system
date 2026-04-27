using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IWorkflowDataEncryptionService: provides encryption/decryption for sensitive workflow data.
    /// Uses AES-256 encryption for data protection.
    /// Supports Requirements 14.1 (Data encryption for sensitive fields) and PDPA compliance.
    /// </summary>
    public class WorkflowDataEncryptionService : IWorkflowDataEncryptionService
    {
        private readonly ILoggingService _loggingService;
        private readonly IConfigService _configService;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _encryptionIV;

        // Regex patterns for sensitive data detection
        private static readonly Regex EmailPattern = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
        private static readonly Regex PhonePattern = new Regex(@"(\+66|0)[0-9]{8,10}", RegexOptions.Compiled);
        private static readonly Regex IdPattern = new Regex(@"\d{13}", RegexOptions.Compiled); // Thai ID number
        private static readonly Regex CreditCardPattern = new Regex(@"\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}", RegexOptions.Compiled);

        public WorkflowDataEncryptionService(ILoggingService loggingService, IConfigService configService)
        {
            if (loggingService == null) throw new ArgumentNullException(nameof(loggingService));
            if (configService == null) throw new ArgumentNullException(nameof(configService));

            _loggingService = loggingService;
            _configService = configService;

            // Initialize encryption key and IV from configuration
            // In production, these should be stored securely (e.g., Azure Key Vault, HSM)
            _encryptionKey = GetEncryptionKey();
            _encryptionIV = GetEncryptionIV();

            if (_encryptionKey == null || _encryptionKey.Length != 32)
                throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits)");

            if (_encryptionIV == null || _encryptionIV.Length != 16)
                throw new InvalidOperationException("Encryption IV must be 16 bytes");
        }

        /// <summary>
        /// Encrypt sensitive workflow data using AES-256.
        /// </summary>
        public string EncryptData(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _encryptionIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error encrypting data: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Decrypt encrypted workflow data.
        /// </summary>
        public string DecryptData(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _encryptionIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        var encryptedBytes = Convert.FromBase64String(encryptedText);
                        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error decrypting data: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Encrypt workflow input parameters containing sensitive data.
        /// </summary>
        public string EncryptWorkflowParameters(string jsonParameters)
        {
            if (string.IsNullOrEmpty(jsonParameters))
                return jsonParameters;

            // Check if parameters contain sensitive data
            if (ContainsSensitiveData(jsonParameters))
            {
                _loggingService.Info("Encrypting workflow parameters containing sensitive data");
                return EncryptData(jsonParameters);
            }

            return jsonParameters;
        }

        /// <summary>
        /// Decrypt workflow input parameters.
        /// </summary>
        public string DecryptWorkflowParameters(string encryptedParameters)
        {
            if (string.IsNullOrEmpty(encryptedParameters))
                return encryptedParameters;

            try
            {
                // Try to decrypt; if it fails, assume it's not encrypted
                return DecryptData(encryptedParameters);
            }
            catch
            {
                // Not encrypted, return as-is
                return encryptedParameters;
            }
        }

        /// <summary>
        /// Encrypt workflow step output data containing sensitive information.
        /// </summary>
        public string EncryptStepOutputData(string jsonOutputData)
        {
            if (string.IsNullOrEmpty(jsonOutputData))
                return jsonOutputData;

            // Check if output contains sensitive data
            if (ContainsSensitiveData(jsonOutputData))
            {
                _loggingService.Info("Encrypting workflow step output containing sensitive data");
                return EncryptData(jsonOutputData);
            }

            return jsonOutputData;
        }

        /// <summary>
        /// Decrypt workflow step output data.
        /// </summary>
        public string DecryptStepOutputData(string encryptedOutputData)
        {
            if (string.IsNullOrEmpty(encryptedOutputData))
                return encryptedOutputData;

            try
            {
                // Try to decrypt; if it fails, assume it's not encrypted
                return DecryptData(encryptedOutputData);
            }
            catch
            {
                // Not encrypted, return as-is
                return encryptedOutputData;
            }
        }

        /// <summary>
        /// Hash sensitive data for audit trail (one-way, cannot be decrypted).
        /// </summary>
        public string HashSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                    return Convert.ToBase64String(hashedBytes);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error hashing data: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Verify if data matches a hash.
        /// </summary>
        public bool VerifyHashedData(string data, string hash)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                var computedHash = HashSensitiveData(data);
                return computedHash == hash;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error verifying hashed data: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if a string contains sensitive data patterns.
        /// </summary>
        public bool ContainsSensitiveData(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return EmailPattern.IsMatch(text) ||
                   PhonePattern.IsMatch(text) ||
                   IdPattern.IsMatch(text) ||
                   CreditCardPattern.IsMatch(text);
        }

        /// <summary>
        /// Mask sensitive data for display.
        /// </summary>
        public string MaskSensitiveData(string data, string dataType)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            try
            {
                switch (dataType?.ToLower())
                {
                    case "email":
                        return MaskEmail(data);
                    case "phone":
                        return MaskPhone(data);
                    case "id":
                        return MaskId(data);
                    case "creditcard":
                        return MaskCreditCard(data);
                    default:
                        return data;
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error masking sensitive data: {ex.Message}", ex);
                return data;
            }
        }

        // Helper methods for masking

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return $"*@{domain}";

            var masked = localPart[0] + new string('*', localPart.Length - 2) + localPart[localPart.Length - 1];
            return $"{masked}@{domain}";
        }

        private string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 4)
                return phone;

            return new string('*', phone.Length - 4) + phone.Substring(phone.Length - 4);
        }

        private string MaskId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length < 4)
                return id;

            return new string('*', id.Length - 4) + id.Substring(id.Length - 4);
        }

        private string MaskCreditCard(string creditCard)
        {
            if (string.IsNullOrEmpty(creditCard) || creditCard.Length < 4)
                return creditCard;

            var cleaned = creditCard.Replace(" ", "").Replace("-", "");
            if (cleaned.Length < 4)
                return creditCard;

            return new string('*', cleaned.Length - 4) + cleaned.Substring(cleaned.Length - 4);
        }

        // Helper methods for key management

        private byte[] GetEncryptionKey()
        {
            // In production, retrieve from secure storage (e.g., Azure Key Vault)
            // For now, use a default key (should be configured in production)
            var keyString = _configService.GetConfigValue("WorkflowEncryptionKey");
            
            if (string.IsNullOrEmpty(keyString))
            {
                _loggingService.Warn("WorkflowEncryptionKey not configured, using default key");
                // Default key (32 bytes for AES-256) - MUST be changed in production
                keyString = "DefaultWorkflowEncryptionKey1234567890";
            }

            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            
            // Ensure key is exactly 32 bytes
            if (keyBytes.Length < 32)
            {
                Array.Resize(ref keyBytes, 32);
            }
            else if (keyBytes.Length > 32)
            {
                Array.Resize(ref keyBytes, 32);
            }

            return keyBytes;
        }

        private byte[] GetEncryptionIV()
        {
            // In production, retrieve from secure storage
            var ivString = _configService.GetConfigValue("WorkflowEncryptionIV");
            
            if (string.IsNullOrEmpty(ivString))
            {
                _loggingService.Warn("WorkflowEncryptionIV not configured, using default IV");
                // Default IV (16 bytes) - MUST be changed in production
                ivString = "DefaultWorkflowIV";
            }

            var ivBytes = Encoding.UTF8.GetBytes(ivString);
            
            // Ensure IV is exactly 16 bytes
            if (ivBytes.Length < 16)
            {
                Array.Resize(ref ivBytes, 16);
            }
            else if (ivBytes.Length > 16)
            {
                Array.Resize(ref ivBytes, 16);
            }

            return ivBytes;
        }
    }
}
