using System;
using System.Collections.Generic;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Generic validation result DTO
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// List of validation warnings
        /// </summary>
        public List<string> Warnings { get; set; }

        public ValidationResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
            IsValid = true;
        }
    }
}
