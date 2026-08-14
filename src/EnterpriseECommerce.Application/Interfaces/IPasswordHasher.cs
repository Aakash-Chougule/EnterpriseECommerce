namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Provides secure password hashing and verification operations.
///
/// The Application layer depends on this abstraction rather than
/// directly depending on a specific hashing library.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Creates a secure hash from a plain-text password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain-text password against an existing hash.
    /// </summary>
    bool Verify(string password, string passwordHash);
}