using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Tailwind.Mail.Services;

/// <summary>
/// Service for handling data encryption and protection
/// </summary>
public interface IDataProtectionService
{
    /// <summary>
    /// Encrypts sensitive data
    /// </summary>
    /// <param name="plaintext">The plaintext to encrypt</param>
    /// <returns>The encrypted data</returns>
    string Protect(string plaintext);
    
    /// <summary>
    /// Decrypts sensitive data
    /// </summary>
    /// <param name="protectedData">The encrypted data</param>
    /// <returns>The original plaintext</returns>
    string Unprotect(string protectedData);
    
    /// <summary>
    /// Hashes data (one-way)
    /// </summary>
    /// <param name="input">The input to hash</param>
    /// <returns>The hash</returns>
    string Hash(string input);
    
    /// <summary>
    /// Anonymizes data while maintaining a consistent length/pattern
    /// </summary>
    /// <param name="input">The input to anonymize</param>
    /// <returns>Anonymized representation</returns>
    string Anonymize(string input);
}

/// <summary>
/// Implementation of data protection service using ASP.NET Core's DataProtection APIs
/// </summary>
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;
    
    public DataProtectionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Tailwind.Mail.PersonalData");
    }
    
    /// <summary>
    /// Encrypts sensitive data using the app's data protection system
    /// </summary>
    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;
            
        return _protector.Protect(plaintext);
    }
    
    /// <summary>
    /// Decrypts data that was protected with the Protect method
    /// </summary>
    public string Unprotect(string protectedData)
    {
        if (string.IsNullOrEmpty(protectedData))
            return protectedData;
            
        return _protector.Unprotect(protectedData);
    }
    
    /// <summary>
    /// Creates a one-way hash of the input
    /// </summary>
    public string Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
    
    /// <summary>
    /// Anonymizes data by preserving the structure but hiding the actual content
    /// For emails: The domain is preserved, but the local part is anonymized
    /// </summary>
    public string Anonymize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
            
        // Special handling for email addresses
        if (input.Contains('@'))
        {
            var parts = input.Split('@');
            if (parts.Length == 2)
            {
                var localPart = parts[0];
                var domain = parts[1];
                
                // Create a deterministic but anonymized local part
                var hash = Hash(localPart).Substring(0, 8);
                return $"anon-{hash}@{domain}";
            }
        }
        
        // General anonymization for other types of data
        var prefix = input.Length <= 4 ? input.Substring(0, 1) : input.Substring(0, 2);
        return $"{prefix}***{input.Substring(input.Length - 1, 1)}";
    }
}
