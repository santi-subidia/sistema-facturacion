using System;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

public class LoginTicketHelper
{
    public static string ObtenerLoginTicketCms(string certPath, string certPassword, string servicio)
    {
        try
        {
            TimeZoneInfo argentinaTimeZone;
            try
            {
                // Intentar ID de IANA (Linux/macOS)
                argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            }
            catch
            {
                try
                {
                    // Intentar ID de Windows
                    argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                }
                catch
                {
                    // Fallback a offset fijo -03:00 si nada funciona (poco probable en entornos modernos)
                    argentinaTimeZone = TimeZoneInfo.CreateCustomTimeZone("ART", TimeSpan.FromHours(-3), "Argentina Standard Time", "Argentina Standard Time");
                }
            }
            DateTime ahoraUtc = DateTime.UtcNow.AddSeconds(-60);
            DateTime ahoraArgentina = TimeZoneInfo.ConvertTime(ahoraUtc, argentinaTimeZone);
            
            TimeSpan argentinaOffset = argentinaTimeZone.GetUtcOffset(ahoraUtc);
            DateTimeOffset fechaGeneracion = new DateTimeOffset(ahoraArgentina, argentinaOffset);
            DateTimeOffset fechaExpiracion = fechaGeneracion.AddHours(12);
            
            string uniqueId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string generationTime = fechaGeneracion.ToString("yyyy-MM-ddTHH:mm:ss-03:00");
            string expirationTime = fechaExpiracion.ToString("yyyy-MM-ddTHH:mm:ss-03:00");

            string xmlStr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<loginTicketRequest version=\"1.0\">" +
                                "<header>" +
                                    "<uniqueId>" + uniqueId + "</uniqueId>" +
                                    "<generationTime>" + generationTime + "</generationTime>" +
                                    "<expirationTime>" + expirationTime + "</expirationTime>" +
                                "</header>" +
                                "<service>" + servicio + "</service>" +
                            "</loginTicketRequest>";

            XmlDocument xmlTRA = new XmlDocument();
            xmlTRA.LoadXml(xmlStr);

            if (!System.IO.File.Exists(certPath))
            {
                throw new System.IO.FileNotFoundException("No se encontró el certificado en la ruta especificada.", certPath);
            }

#pragma warning disable SYSLIB0057
            using X509Certificate2 certificado = new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.PersistKeySet);
#pragma warning restore SYSLIB0057

            if (!certificado.HasPrivateKey)
            {
                throw new InvalidOperationException("El certificado no contiene una clave privada.");
            }

            Encoding msgEncoding = Encoding.UTF8;
            byte[] msgBytes = msgEncoding.GetBytes(xmlTRA.OuterXml);

            ContentInfo contentInfo = new ContentInfo(msgBytes);
            SignedCms signedCms = new SignedCms(contentInfo);

            CmsSigner cmsSigner = new CmsSigner(certificado);
            cmsSigner.IncludeOption = X509IncludeOption.EndCertOnly;

            signedCms.ComputeSignature(cmsSigner);
            byte[] cmsEncoded = signedCms.Encode();

            return Convert.ToBase64String(cmsEncoded);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar el LoginTicket CMS: {ex.Message}", ex);
        }
    }
}