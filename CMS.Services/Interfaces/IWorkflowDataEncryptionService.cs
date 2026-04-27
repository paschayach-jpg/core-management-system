using System;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow data encryption service for protecting sensitive fields.
    /// Implements Requirements 14.1 (Data encryption for sensitive fields).
    /// Supports PDPA compliance by encrypting personal data.
    /// </summary>
    public interface IWorkflowDataEncryptionService
    {
        /// <summary>
        /// Encrypt sensitive workflow data (e.g., personal information, approval decisions).
        /// </summary>
        string EncryptData(string plainText);

        /// <summary>
        /// Decrypt encrypted workflow data.
        /// </summary>
        string DecryptData(string encryptedText);

        /// <summary>
        /// Encrypt workflow input parameters containing sensitive data.
        /// </summary>
        string EncryptWorkflowParameters(string jsonParameters);

        /// <summary>
        /// Decrypt workflow input parameters.
        /// </summary>
        string DecryptWorkflowParameters(string encryptedParameters);

        /// <summary>
        /// Encrypt workflow step output data containing sensitive information.
        /// </summary>
        string EncryptStepOutputData(string jsonOutputData);

        /// <summary>
        /// Decrypt workflow step output data.
        /// </summary>
        string DecryptStepOutputData(string encryptedOutputData);

        /// <summary>
        /// Hash sensitive data for audit trail (one-way, cannot be decrypted).
        /// Used for storing sensitive data references without exposing actual values.
        /// </summary>
        string HashSensitiveData(string data);

        /// <summary>
        /// Verify if data matches a hash (for audit trail verification).
        /// </summary>
        bool VerifyHashedData(string data, string hash);

        /// <summary>
        /// Check if a string contains sensitive data patterns (email, phone, ID number).
        /// </summary>
        bool ContainsSensitiveData(string text);

        /// <summary>
        /// Mask sensitive data for display (e.g., show only last 4 digits of ID).
        /// </summary>
        string MaskSensitiveData(string data, string dataType);
    }
}
