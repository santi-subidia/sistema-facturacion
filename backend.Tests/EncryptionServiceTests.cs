using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Backend.Services.Business;

namespace backend.Tests
{
    public class EncryptionServiceTests
    {
        [Fact]
        public void Encrypt_And_Decrypt_Should_Work_Correctly()
        {
            // Arrange
            var provider = new EphemeralDataProtectionProvider();
            var service = new EncryptionService(provider);
            var plainText = "mi_password_secreto_123";

            // Act
            var encrypted = service.Encrypt(plainText);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBe(plainText); // La versión encriptada debe ser distinta
            decrypted.Should().Be(plainText);    // Al desencriptar debe volver a ser el original
        }

        [Fact]
        public void Decrypt_Should_Return_OriginalString_When_Not_Encrypted()
        {
            // Arrange
            var provider = new EphemeralDataProtectionProvider();
            var service = new EncryptionService(provider);
            var plainText = "password_viejo_en_texto_plano";

            // Act
            // Le pasamos un string que no está encriptado (fallará CryptographicException internamente)
            var decrypted = service.Decrypt(plainText);

            // Assert
            // El comportamiento esperado (fallback) es que devuelva el mismo string
            decrypted.Should().Be(plainText); 
        }

        [Fact]
        public void Encrypt_Should_Return_NullOrEmpty_When_Input_Is_NullOrEmpty()
        {
            var provider = new EphemeralDataProtectionProvider();
            var service = new EncryptionService(provider);

            service.Encrypt(null).Should().BeNull();
            service.Encrypt("").Should().BeEmpty();
        }
    }
}
