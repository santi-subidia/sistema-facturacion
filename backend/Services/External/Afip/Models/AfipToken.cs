using System;

namespace Backend.Services.External.Afip.Models
{
    public class AfipToken
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public bool EsProduccion { get; set; }

        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpirationTime.AddMinutes(-5);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Token) && 
                   !string.IsNullOrEmpty(Sign) && 
                   !IsExpired();
        }
    }
}
