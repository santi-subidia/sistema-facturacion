namespace Backend.Constants
{
    public static class AfipConstants
    {
        // Tipos de Documento AFIP
        public const int TipoDocumentoCuit = 80;
        public const int TipoDocumentoDni = 96;
        public const int TipoDocumentoConsumidorFinal = 99;

        // Condición IVA por defecto
        public const int CondicionIvaConsumidorFinal = 5;

        // Códigos AFIP de Notas de Crédito (A=3, B=8, C=13)
        public static readonly int[] CodigosNotaDeCredito = { 3, 8, 13 };

        // Resultado de autorización CAE
        public const string ResultadoAprobado = "A";
    }
}
