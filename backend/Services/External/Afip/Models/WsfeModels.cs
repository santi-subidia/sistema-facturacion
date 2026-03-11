namespace Backend.Services.External.Afip.Models
{
    public class FEAuthRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public long Cuit { get; set; }
    }

    public class FEErr
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
    }

    public class FEEvent
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
    }

    public class FEDummyResponse
    {
        public string? AppServer { get; set; }
        public string? DbServer { get; set; }
        public string? AuthServer { get; set; }
    }

    public class FECompUltimoAutorizadoRequest
    {
        public FEAuthRequest Auth { get; set; } = new();
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
    }

    public class FECompUltimoAutorizadoResponse
    {
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
        public long CbteNro { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class FECAERequest
    {
        public FEAuthRequest Auth { get; set; } = new();
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
        public List<FECAEDetRequest> FeDetReq { get; set; } = new();
    }

    public class FECAEDetRequest
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public long CbteDesde { get; set; }
        public long CbteHasta { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public decimal ImpTotal { get; set; }
        public decimal ImpTotConc { get; set; }
        public decimal ImpNeto { get; set; }
        public decimal ImpOpEx { get; set; }
        public decimal ImpTrib { get; set; }
        public decimal ImpIVA { get; set; }
        public string FchServDesde { get; set; } = string.Empty;
        public string FchServHasta { get; set; } = string.Empty;
        public string FchVtoPago { get; set; } = string.Empty;
        public string MonId { get; set; } = "PES";
        public decimal MonCotiz { get; set; } = 1;
        public int CondicionIVAReceptorId { get; set; }

        public List<AlicIva>? Iva { get; set; }
        public List<Tributo>? Tributos { get; set; }
        public List<CbteAsoc>? CbtesAsoc { get; set; }
        public List<Opcional>? Opcionales { get; set; }
    }

    public class AlicIva
    {
        public int Id { get; set; }
        public decimal BaseImp { get; set; }
        public decimal Importe { get; set; }
    }

    public class Tributo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public decimal BaseImp { get; set; }
        public decimal Alic { get; set; }
        public decimal Importe { get; set; }
    }

    public class CbteAsoc
    {
        public int Tipo { get; set; }
        public int PtoVta { get; set; }
        public long Nro { get; set; }
        public string? Cuit { get; set; }
    }

    public class Opcional
    {
        public string Id { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class FECAEResponse
    {
        public FECAECabResponse? FeCabResp { get; set; }
        public List<FECAEDetResponse>? FeDetResp { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class FECAECabResponse
    {
        public long Cuit { get; set; }
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
        public string FchProceso { get; set; } = string.Empty; // Format: YYYYMMDDHHmmss (timestamp)
        public int CantReg { get; set; }
        public string Resultado { get; set; } = string.Empty; // A = Approved, R = Rejected, P = Partial
        public string? Reproceso { get; set; }
    }

    public class FECAEDetResponse
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public long CbteDesde { get; set; }
        public long CbteHasta { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty; // A = Approved, R = Rejected
        public string? CAE { get; set; }
        public string? CAEFchVto { get; set; } // Format: YYYYMMDD
        public List<FEErr>? Observaciones { get; set; }
    }

    public class FECompConsultarRequest
    {
        public FEAuthRequest Auth { get; set; } = new();
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
        public long CbteNro { get; set; }
    }

    public class FECompConsultarResponse
    {
        public FECompConsultar? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class FECompConsultar
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public long CbteDesde { get; set; }
        public long CbteHasta { get; set; }
        public string CbteFch { get; set; } = string.Empty;
        public decimal ImpTotal { get; set; }
        public decimal ImpTotConc { get; set; }
        public decimal ImpNeto { get; set; }
        public decimal ImpOpEx { get; set; }
        public decimal ImpTrib { get; set; }
        public decimal ImpIVA { get; set; }
        public string FchServDesde { get; set; } = string.Empty;
        public string FchServHasta { get; set; } = string.Empty;
        public string FchVtoPago { get; set; } = string.Empty;
        public string MonId { get; set; } = string.Empty;
        public decimal MonCotiz { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string? CAE { get; set; }
        public string? CAEFchVto { get; set; }
        public int PtoVta { get; set; }
        public int CbteTipo { get; set; }
        public List<AlicIva>? Iva { get; set; }
        public List<Tributo>? Tributos { get; set; }
    }

    public class FEParamGetTiposCbteResponse
    {
        public List<CbteTipo>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class CbteTipo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string? FchDesde { get; set; }
        public string? FchHasta { get; set; }
    }

    public class FEParamGetTiposDocResponse
    {
        public List<DocTipo>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class DocTipo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string? FchDesde { get; set; }
        public string? FchHasta { get; set; }
    }

    public class FEParamGetTiposIvaResponse
    {
        public List<IvaTipo>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class IvaTipo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string? FchDesde { get; set; }
        public string? FchHasta { get; set; }
    }

    public class FEParamGetTiposMonedasResponse
    {
        public List<Moneda>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class Moneda
    {
        public string Id { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string? FchDesde { get; set; }
        public string? FchHasta { get; set; }
    }

    public class FEParamGetTiposConceptoResponse
    {
        public List<ConceptoTipo>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class ConceptoTipo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string? FchDesde { get; set; }
        public string? FchHasta { get; set; }
    }

    public class FEParamGetPtosVentaResponse
    {
        public List<PtoVenta>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class PtoVenta
    {
        public int Nro { get; set; }
        public string? EmisionTipo { get; set; }
        public string? Bloqueado { get; set; }
        public string? FchBaja { get; set; }
    }

    public class FEParamGetTiposTributosResponse
    {
        public List<TributoTipo>? ResultGet { get; set; }
        public List<FEErr>? Errors { get; set; }
        public List<FEEvent>? Events { get; set; }
    }

    public class TributoTipo
    {
        public int Id { get; set; }
        public string Desc { get; set; } = string.Empty;
        public string? FchDesde { get; set; }
        public string? FchHasta { get; set; }
    }
}
