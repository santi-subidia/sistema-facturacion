using System.Text;
using System.Xml.Linq;
using Backend.Services.External.Afip.Models;
using Backend.Services.External.Afip.Interfaces;
using Backend.Data;
using Backend.DTOs.PuntoVenta;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.External.Afip.Services
{
    public class AfipWsfeService : IAfipWsfeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AfipAuthService _authService;
        private readonly AppDbContext _db;

        public AfipWsfeService(HttpClient httpClient, IConfiguration configuration, AfipAuthService authService, AppDbContext db)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _authService = authService;
            _db = db;
        }

        private async Task<(long Cuit, string Url)> ObtenerConfiguracionAsync()
        {
            var config = await _db.AfipConfiguraciones
                .Where(c => c.Activa)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                 // Fallback legacy (si fuera necesario, pero mejor obligar DB)
                 throw new InvalidOperationException("No se encontró una configuración activa de AFIP en la base de datos.");
            }

            if (!long.TryParse(config.Cuit, out long cuit))
            {
                throw new InvalidOperationException($"El CUIT '{config.Cuit}' en la configuración activa no tiene un formato válido.");
            }

            string url = config.EsProduccion
                ? (_configuration["Afip:WsfeUrlProd"] ?? "https://servicios1.afip.gov.ar/wsfev1/service.asmx")
                : (_configuration["Afip:WsfeUrl"] ?? "https://wswhomo.afip.gov.ar/wsfev1/service.asmx");

            return (cuit, url);
        }

        public async Task<FEDummyResponse> FEDummyAsync()
        {
            var (_, url) = await ObtenerConfiguracionAsync();

            var soapRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FEDummy/>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FEDummy");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            return ParseFEDummyResponse(responseXml);
        }

        public async Task<FECompUltimoAutorizadoResponse> FECompUltimoAutorizadoAsync(int ptoVta, int cbteTipo)
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();
            
            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FECompUltimoAutorizado>
            <ar:Auth>
                <ar:Token>{token.Token}</ar:Token>
                <ar:Sign>{token.Sign}</ar:Sign>
                <ar:Cuit>{cuit}</ar:Cuit>
            </ar:Auth>
            <ar:PtoVta>{ptoVta}</ar:PtoVta>
            <ar:CbteTipo>{cbteTipo}</ar:CbteTipo>
        </ar:FECompUltimoAutorizado>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            return ParseFECompUltimoAutorizadoResponse(responseXml);
        }

        public async Task<FECAEResponse> FECAESolicitarAsync(FECAERequest request)
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();

            var soapBuilder = new StringBuilder();
            soapBuilder.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FECAESolicitar>
            <ar:Auth>
                <ar:Token>");
            soapBuilder.Append(token.Token);
            soapBuilder.Append("</ar:Token>\n                <ar:Sign>");
            soapBuilder.Append(token.Sign);
            soapBuilder.Append("</ar:Sign>\n                <ar:Cuit>");
            soapBuilder.Append(cuit);
            soapBuilder.Append(@"</ar:Cuit>
            </ar:Auth>
            <ar:FeCAEReq>
                <ar:FeCabReq>
                    <ar:CantReg>");
            soapBuilder.Append(request.FeDetReq.Count);
            soapBuilder.Append("</ar:CantReg>\n                    <ar:PtoVta>");
            soapBuilder.Append(request.PtoVta);
            soapBuilder.Append("</ar:PtoVta>\n                    <ar:CbteTipo>");
            soapBuilder.Append(request.CbteTipo);
            soapBuilder.Append("</ar:CbteTipo>\n                </ar:FeCabReq>\n");

            foreach (var det in request.FeDetReq)
            {
                soapBuilder.Append(@"                <ar:FeDetReq>
                    <ar:FECAEDetRequest>
                        <ar:Concepto>");
                soapBuilder.Append(det.Concepto);
                soapBuilder.Append("</ar:Concepto>\n                        <ar:DocTipo>");
                soapBuilder.Append(det.DocTipo);
                soapBuilder.Append("</ar:DocTipo>\n                        <ar:DocNro>");
                soapBuilder.Append(det.DocNro);
                soapBuilder.Append("</ar:DocNro>\n                        <ar:CbteDesde>");
                soapBuilder.Append(det.CbteDesde);
                soapBuilder.Append("</ar:CbteDesde>\n                        <ar:CbteHasta>");
                soapBuilder.Append(det.CbteHasta);
                soapBuilder.Append("</ar:CbteHasta>\n                        <ar:CbteFch>");
                soapBuilder.Append(det.CbteFch);
                soapBuilder.Append("</ar:CbteFch>\n                        <ar:ImpTotal>");
                soapBuilder.Append(det.ImpTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:ImpTotal>\n                        <ar:ImpTotConc>");
                soapBuilder.Append(det.ImpTotConc.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:ImpTotConc>\n                        <ar:ImpNeto>");
                soapBuilder.Append(det.ImpNeto.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:ImpNeto>\n                        <ar:ImpOpEx>");
                soapBuilder.Append(det.ImpOpEx.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:ImpOpEx>\n                        <ar:ImpTrib>");
                soapBuilder.Append(det.ImpTrib.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:ImpTrib>\n                        <ar:ImpIVA>");
                soapBuilder.Append(det.ImpIVA.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:ImpIVA>\n");

                if (!string.IsNullOrEmpty(det.FchServDesde))
                {
                    soapBuilder.Append("                        <ar:FchServDesde>");
                    soapBuilder.Append(det.FchServDesde);
                    soapBuilder.Append("</ar:FchServDesde>\n");
                }
                if (!string.IsNullOrEmpty(det.FchServHasta))
                {
                    soapBuilder.Append("                        <ar:FchServHasta>");
                    soapBuilder.Append(det.FchServHasta);
                    soapBuilder.Append("</ar:FchServHasta>\n");
                }
                if (!string.IsNullOrEmpty(det.FchVtoPago))
                {
                    soapBuilder.Append("                        <ar:FchVtoPago>");
                    soapBuilder.Append(det.FchVtoPago);
                    soapBuilder.Append("</ar:FchVtoPago>\n");
                }

                soapBuilder.Append("                        <ar:MonId>");
                soapBuilder.Append(det.MonId);
                soapBuilder.Append("</ar:MonId>\n                        <ar:MonCotiz>");
                soapBuilder.Append(det.MonCotiz.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                soapBuilder.Append("</ar:MonCotiz>\n");
                
                soapBuilder.Append("                        <ar:CondicionIVAReceptorId>");
                soapBuilder.Append(det.CondicionIVAReceptorId);
                soapBuilder.Append("</ar:CondicionIVAReceptorId>\n");

                if (det.Iva != null && det.Iva.Count > 0)
                {
                    foreach (var iva in det.Iva)
                    {
                        soapBuilder.Append(@"                        <ar:Iva>
                            <ar:Id>");
                        soapBuilder.Append(iva.Id);
                        soapBuilder.Append("</ar:Id>\n                            <ar:BaseImp>");
                        soapBuilder.Append(iva.BaseImp.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                        soapBuilder.Append("</ar:BaseImp>\n                            <ar:Importe>");
                        soapBuilder.Append(iva.Importe.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                        soapBuilder.Append("</ar:Importe>\n                        </ar:Iva>\n");
                    }
                }

                if (det.Tributos != null && det.Tributos.Count > 0)
                {
                    foreach (var trib in det.Tributos)
                    {
                        soapBuilder.Append(@"                        <ar:Tributos>
                            <ar:Id>");
                        soapBuilder.Append(trib.Id);
                        soapBuilder.Append("</ar:Id>\n                            <ar:Desc>");
                        soapBuilder.Append(System.Security.SecurityElement.Escape(trib.Desc));
                        soapBuilder.Append("</ar:Desc>\n                            <ar:BaseImp>");
                        soapBuilder.Append(trib.BaseImp.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                        soapBuilder.Append("</ar:BaseImp>\n                            <ar:Alic>");
                        soapBuilder.Append(trib.Alic.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                        soapBuilder.Append("</ar:Alic>\n                            <ar:Importe>");
                        soapBuilder.Append(trib.Importe.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                        soapBuilder.Append("</ar:Importe>\n                        </ar:Tributos>\n");
                    }
                }

                if (det.CbtesAsoc != null && det.CbtesAsoc.Count > 0)
                {
                    soapBuilder.Append("                        <ar:CbtesAsoc>\n");
                    foreach (var cbteAsoc in det.CbtesAsoc)
                    {
                        soapBuilder.Append(@"                            <ar:CbteAsoc>
                                <ar:Tipo>");
                        soapBuilder.Append(cbteAsoc.Tipo);
                        soapBuilder.Append("</ar:Tipo>\n                                <ar:PtoVta>");
                        soapBuilder.Append(cbteAsoc.PtoVta);
                        soapBuilder.Append("</ar:PtoVta>\n                                <ar:Nro>");
                        soapBuilder.Append(cbteAsoc.Nro);
                        soapBuilder.Append("</ar:Nro>\n");
                        if (!string.IsNullOrEmpty(cbteAsoc.Cuit))
                        {
                            soapBuilder.Append("                                <ar:Cuit>");
                            soapBuilder.Append(cbteAsoc.Cuit);
                            soapBuilder.Append("</ar:Cuit>\n");
                        }
                        soapBuilder.Append("                            </ar:CbteAsoc>\n");
                    }
                    soapBuilder.Append("                        </ar:CbtesAsoc>\n");
                }

                if (det.Opcionales != null && det.Opcionales.Count > 0)
                {
                    foreach (var opc in det.Opcionales)
                    {
                        soapBuilder.Append(@"                        <ar:Opcionales>
                            <ar:Id>");
                        soapBuilder.Append(System.Security.SecurityElement.Escape(opc.Id));
                        soapBuilder.Append("</ar:Id>\n                            <ar:Valor>");
                        soapBuilder.Append(System.Security.SecurityElement.Escape(opc.Valor));
                        soapBuilder.Append("</ar:Valor>\n                        </ar:Opcionales>\n");
                    }
                }

                soapBuilder.Append("                    </ar:FECAEDetRequest>\n                </ar:FeDetReq>\n");
            }

            soapBuilder.Append(@"            </ar:FeCAEReq>
        </ar:FECAESolicitar>
    </soap:Body>
</soap:Envelope>");

            var soapXml = soapBuilder.ToString();
            
            var content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FECAESolicitar");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            
            return ParseFECAEResponse(responseXml);
        }

        public async Task<FECompConsultarResponse> FECompConsultarAsync(int ptoVta, int cbteTipo, long cbteNro)
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();

            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FECompConsultar>
            <ar:Auth>
                <ar:Token>{token.Token}</ar:Token>
                <ar:Sign>{token.Sign}</ar:Sign>
                <ar:Cuit>{cuit}</ar:Cuit>
            </ar:Auth>
            <ar:FeCompConsReq>
                <ar:PtoVta>{ptoVta}</ar:PtoVta>
                <ar:CbteTipo>{cbteTipo}</ar:CbteTipo>
                <ar:CbteNro>{cbteNro}</ar:CbteNro>
            </ar:FeCompConsReq>
        </ar:FECompConsultar>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FECompConsultar");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            return ParseFECompConsultarResponse(responseXml);
        }

        public async Task<FEParamGetTiposCbteResponse> FEParamGetTiposCbteAsync()
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();

            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FEParamGetTiposCbte>
            <ar:Auth>
                <ar:Token>{token.Token}</ar:Token>
                <ar:Sign>{token.Sign}</ar:Sign>
                <ar:Cuit>{cuit}</ar:Cuit>
            </ar:Auth>
        </ar:FEParamGetTiposCbte>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FEParamGetTiposCbte");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            return ParseFEParamGetTiposCbteResponse(responseXml);
        }

        public async Task<FEParamGetTiposDocResponse> FEParamGetTiposDocAsync()
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();

            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FEParamGetTiposDoc>
            <ar:Auth>
                <ar:Token>{token.Token}</ar:Token>
                <ar:Sign>{token.Sign}</ar:Sign>
                <ar:Cuit>{cuit}</ar:Cuit>
            </ar:Auth>
        </ar:FEParamGetTiposDoc>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FEParamGetTiposDoc");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            return ParseFEParamGetTiposDocResponse(responseXml);
        }

        public async Task<FEParamGetTiposIvaResponse> FEParamGetTiposIvaAsync()
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();

            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FEParamGetTiposIva>
            <ar:Auth>
                <ar:Token>{token.Token}</ar:Token>
                <ar:Sign>{token.Sign}</ar:Sign>
                <ar:Cuit>{cuit}</ar:Cuit>
            </ar:Auth>
        </ar:FEParamGetTiposIva>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FEParamGetTiposIva");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            return ParseFEParamGetTiposIvaResponse(responseXml);
        }

        public async Task<List<PuntoVentaDTO>> FEParamGetPtosVentaAsync()
        {
            var token = await _authService.GetTokenAsync("wsfe");
            var (cuit, url) = await ObtenerConfiguracionAsync();

            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
    <soap:Header/>
    <soap:Body>
        <ar:FEParamGetPtosVenta>
            <ar:Auth>
                <ar:Token>{token.Token}</ar:Token>
                <ar:Sign>{token.Sign}</ar:Sign>
                <ar:Cuit>{cuit}</ar:Cuit>
            </ar:Auth>
        </ar:FEParamGetPtosVenta>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ar.gov.afip.dif.FEV1/FEParamGetPtosVenta");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseXml = await response.Content.ReadAsStringAsync();
            var parsedResponse = ParseFEParamGetPtosVentaResponse(responseXml);

            if (parsedResponse.Errors != null && parsedResponse.Errors.Any())
            {
                var errorMessages = string.Join("; ", parsedResponse.Errors.Select(e => $"{e.Code}: {e.Msg}"));
                throw new InvalidOperationException($"Error de AFIP al obtener puntos de venta: {errorMessages}");
            }

            var result = new List<PuntoVentaDTO>();
            if (parsedResponse.ResultGet != null)
            {
                result = parsedResponse.ResultGet
                    .Where(p => string.Equals(p.EmisionTipo, "RECE", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(p.EmisionTipo, "Web Services", StringComparison.OrdinalIgnoreCase))
                    .Select(p => new PuntoVentaDTO
                    {
                        Nro = p.Nro,
                        EmisionTipo = p.EmisionTipo,
                        Bloqueado = p.Bloqueado,
                        FchBaja = p.FchBaja
                    })
                    .ToList();
            }

            return result;
        }

        private FEDummyResponse ParseFEDummyResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var result = doc.Descendants(ns + "FEDummyResult").FirstOrDefault();
            if (result == null)
            {
                throw new InvalidOperationException("Invalid FEDummy response");
            }

            return new FEDummyResponse
            {
                AppServer = result.Element(ns + "AppServer")?.Value,
                DbServer = result.Element(ns + "DbServer")?.Value,
                AuthServer = result.Element(ns + "AuthServer")?.Value
            };
        }

        private FECompUltimoAutorizadoResponse ParseFECompUltimoAutorizadoResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var response = doc.Descendants(ns + "FECompUltimoAutorizadoResponse").FirstOrDefault();
            if (response == null)
            {
                throw new InvalidOperationException("Invalid FECompUltimoAutorizado response");
            }

            var result = response.Descendants(ns + "FECompUltimoAutorizadoResult").FirstOrDefault();
            
            return new FECompUltimoAutorizadoResponse
            {
                PtoVta = int.Parse(result?.Element(ns + "PtoVta")?.Value ?? "0"),
                CbteTipo = int.Parse(result?.Element(ns + "CbteTipo")?.Value ?? "0"),
                CbteNro = long.Parse(result?.Element(ns + "CbteNro")?.Value ?? "0"),
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private FECAEResponse ParseFECAEResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var response = doc.Descendants(ns + "FECAESolicitarResponse").FirstOrDefault();
            if (response == null)
            {
                throw new InvalidOperationException("Invalid FECAESolicitar response");
            }

            var result = response.Descendants(ns + "FECAESolicitarResult").FirstOrDefault();
            
            return new FECAEResponse
            {
                FeCabResp = ParseFECAECabResponse(result, ns),
                FeDetResp = ParseFECAEDetResponses(result, ns),
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private FECAECabResponse? ParseFECAECabResponse(XElement? result, XNamespace ns)
        {
            var cab = result?.Element(ns + "FeCabResp");
            if (cab == null) return null;

            return new FECAECabResponse
            {
                Cuit = long.Parse(cab.Element(ns + "Cuit")?.Value ?? "0"),
                PtoVta = int.Parse(cab.Element(ns + "PtoVta")?.Value ?? "0"),
                CbteTipo = int.Parse(cab.Element(ns + "CbteTipo")?.Value ?? "0"),
                FchProceso = cab.Element(ns + "FchProceso")?.Value ?? string.Empty,
                CantReg = int.Parse(cab.Element(ns + "CantReg")?.Value ?? "0"),
                Resultado = cab.Element(ns + "Resultado")?.Value ?? string.Empty,
                Reproceso = cab.Element(ns + "Reproceso")?.Value
            };
        }

        private List<FECAEDetResponse>? ParseFECAEDetResponses(XElement? result, XNamespace ns)
        {
            var detList = result?.Descendants(ns + "FECAEDetResponse");
            if (detList == null || !detList.Any()) return null;

            return detList.Select(det => new FECAEDetResponse
            {
                Concepto = int.Parse(det.Element(ns + "Concepto")?.Value ?? "0"),
                DocTipo = int.Parse(det.Element(ns + "DocTipo")?.Value ?? "0"),
                DocNro = long.Parse(det.Element(ns + "DocNro")?.Value ?? "0"),
                CbteDesde = long.Parse(det.Element(ns + "CbteDesde")?.Value ?? "0"),
                CbteHasta = long.Parse(det.Element(ns + "CbteHasta")?.Value ?? "0"),
                CbteFch = det.Element(ns + "CbteFch")?.Value ?? string.Empty,
                Resultado = det.Element(ns + "Resultado")?.Value ?? string.Empty,
                CAE = det.Element(ns + "CAE")?.Value,
                CAEFchVto = det.Element(ns + "CAEFchVto")?.Value,
                Observaciones = ParseObservaciones(det, ns)
            }).ToList();
        }

        private FECompConsultarResponse ParseFECompConsultarResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var response = doc.Descendants(ns + "FECompConsultarResponse").FirstOrDefault();
            if (response == null)
            {
                throw new InvalidOperationException("Invalid FECompConsultar response");
            }

            var result = response.Descendants(ns + "FECompConsultarResult").FirstOrDefault();
            
            return new FECompConsultarResponse
            {
                ResultGet = ParseFECompConsultar(result, ns),
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private FECompConsultar? ParseFECompConsultar(XElement? result, XNamespace ns)
        {
            var get = result?.Element(ns + "ResultGet");
            if (get == null) return null;

            return new FECompConsultar
            {
                Concepto = int.Parse(get.Element(ns + "Concepto")?.Value ?? "0"),
                DocTipo = int.Parse(get.Element(ns + "DocTipo")?.Value ?? "0"),
                DocNro = long.Parse(get.Element(ns + "DocNro")?.Value ?? "0"),
                CbteDesde = long.Parse(get.Element(ns + "CbteDesde")?.Value ?? "0"),
                CbteHasta = long.Parse(get.Element(ns + "CbteHasta")?.Value ?? "0"),
                CbteFch = get.Element(ns + "CbteFch")?.Value ?? string.Empty,
                ImpTotal = decimal.Parse(get.Element(ns + "ImpTotal")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                ImpTotConc = decimal.Parse(get.Element(ns + "ImpTotConc")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                ImpNeto = decimal.Parse(get.Element(ns + "ImpNeto")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                ImpOpEx = decimal.Parse(get.Element(ns + "ImpOpEx")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                ImpTrib = decimal.Parse(get.Element(ns + "ImpTrib")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                ImpIVA = decimal.Parse(get.Element(ns + "ImpIVA")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                MonId = get.Element(ns + "MonId")?.Value ?? string.Empty,
                MonCotiz = decimal.Parse(get.Element(ns + "MonCotiz")?.Value ?? "1", System.Globalization.CultureInfo.InvariantCulture),
                Resultado = get.Element(ns + "Resultado")?.Value ?? string.Empty,
                CAE = get.Element(ns + "CAE")?.Value,
                CAEFchVto = get.Element(ns + "CAEFchVto")?.Value,
                PtoVta = int.Parse(get.Element(ns + "PtoVta")?.Value ?? "0"),
                CbteTipo = int.Parse(get.Element(ns + "CbteTipo")?.Value ?? "0")
            };
        }

        private FEParamGetTiposCbteResponse ParseFEParamGetTiposCbteResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var response = doc.Descendants(ns + "FEParamGetTiposCbteResponse").FirstOrDefault();
            var result = response?.Descendants(ns + "FEParamGetTiposCbteResult").FirstOrDefault();
            
            var tipos = result?.Descendants(ns + "CbteTipo").Select(t => new CbteTipo
            {
                Id = int.Parse(t.Element(ns + "Id")?.Value ?? "0"),
                Desc = t.Element(ns + "Desc")?.Value ?? string.Empty,
                FchDesde = t.Element(ns + "FchDesde")?.Value,
                FchHasta = t.Element(ns + "FchHasta")?.Value
            }).ToList();

            return new FEParamGetTiposCbteResponse
            {
                ResultGet = tipos,
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private FEParamGetTiposDocResponse ParseFEParamGetTiposDocResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var response = doc.Descendants(ns + "FEParamGetTiposDocResponse").FirstOrDefault();
            var result = response?.Descendants(ns + "FEParamGetTiposDocResult").FirstOrDefault();
            
            var tipos = result?.Descendants(ns + "DocTipo").Select(t => new DocTipo
            {
                Id = int.Parse(t.Element(ns + "Id")?.Value ?? "0"),
                Desc = t.Element(ns + "Desc")?.Value ?? string.Empty,
                FchDesde = t.Element(ns + "FchDesde")?.Value,
                FchHasta = t.Element(ns + "FchHasta")?.Value
            }).ToList();

            return new FEParamGetTiposDocResponse
            {
                ResultGet = tipos,
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private FEParamGetTiposIvaResponse ParseFEParamGetTiposIvaResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";
            
            var response = doc.Descendants(ns + "FEParamGetTiposIvaResponse").FirstOrDefault();
            var result = response?.Descendants(ns + "FEParamGetTiposIvaResult").FirstOrDefault();
            
            var tipos = result?.Descendants(ns + "IvaTipo").Select(t => new IvaTipo
            {
                Id = int.Parse(t.Element(ns + "Id")?.Value ?? "0"),
                Desc = t.Element(ns + "Desc")?.Value ?? string.Empty,
                FchDesde = t.Element(ns + "FchDesde")?.Value,
                FchHasta = t.Element(ns + "FchHasta")?.Value
            }).ToList();

            return new FEParamGetTiposIvaResponse
            {
                ResultGet = tipos,
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private FEParamGetPtosVentaResponse ParseFEParamGetPtosVentaResponse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://ar.gov.afip.dif.FEV1/";

            var response = doc.Descendants(ns + "FEParamGetPtosVentaResponse").FirstOrDefault();
            var result = response?.Descendants(ns + "FEParamGetPtosVentaResult").FirstOrDefault();
            
            var ptos = result?.Descendants(ns + "PtoVenta").Select(p => new PtoVenta
            {
                Nro = int.Parse(p.Element(ns + "Nro")?.Value ?? "0"),
                EmisionTipo = p.Element(ns + "EmisionTipo")?.Value,
                Bloqueado = p.Element(ns + "Bloqueado")?.Value,
                FchBaja = p.Element(ns + "FchBaja")?.Value
            }).ToList();

            return new FEParamGetPtosVentaResponse
            {
                ResultGet = ptos,
                Errors = ParseErrors(result, ns),
                Events = ParseEvents(result, ns)
            };
        }

        private List<FEErr>? ParseErrors(XElement? element, XNamespace ns)
        {
            var errors = element?.Descendants(ns + "Err");
            if (errors == null || !errors.Any()) return null;

            return errors.Select(e => new FEErr
            {
                Code = int.Parse(e.Element(ns + "Code")?.Value ?? "0"),
                Msg = e.Element(ns + "Msg")?.Value ?? string.Empty
            }).ToList();
        }

        private List<FEEvent>? ParseEvents(XElement? element, XNamespace ns)
        {
            var events = element?.Descendants(ns + "Evento");
            if (events == null || !events.Any()) return null;

            return events.Select(e => new FEEvent
            {
                Code = int.Parse(e.Element(ns + "Code")?.Value ?? "0"),
                Msg = e.Element(ns + "Msg")?.Value ?? string.Empty
            }).ToList();
        }

        private List<FEErr>? ParseObservaciones(XElement? element, XNamespace ns)
        {
            var obs = element?.Descendants(ns + "Obs");
            if (obs == null || !obs.Any()) return null;

            return obs.Select(o => new FEErr
            {
                Code = int.Parse(o.Element(ns + "Code")?.Value ?? "0"),
                Msg = o.Element(ns + "Msg")?.Value ?? string.Empty
            }).ToList();
        }
    }
}
