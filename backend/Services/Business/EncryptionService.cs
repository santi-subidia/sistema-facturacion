using Backend.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Backend.Services.Business
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IDataProtector _newProtector;
        private readonly IDataProtector _oldProtector;

        public EncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            _newProtector = dataProtectionProvider.CreateProtector("SensitiveData.v1");
            _oldProtector = dataProtectionProvider.CreateProtector("EmailPassword");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
                
            return _newProtector.Protect(plainText);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;
            
            try
            {
                // Intentar descifrar con el nuevo protector
                return _newProtector.Unprotect(encryptedText);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                try
                {
                    // Fallback al protector viejo (para datos previos a la migración)
                    return _oldProtector.Unprotect(encryptedText);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    // Si ambos fallan, asumimos que es texto plano (migración inicial)
                    return encryptedText;
                }
            }
        }
    }
}
