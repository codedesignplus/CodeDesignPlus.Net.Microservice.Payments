using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Constants;

/// <summary>
/// Represents the available document types used in the PayU payment gateway.
/// </summary>
public class PayuDocumentTypes
{
    /// <summary>
    /// List of document types available in the PayU payment gateway.
    /// </summary>
    public List<PayuDocumentType> DocumentTypes { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PayuDocumentTypes"/> class with predefined document types.
    /// </summary>
    public PayuDocumentTypes()
    {
        DocumentTypes.Add(new PayuDocumentType { Code = "CC", Description = "Cédula de ciudadanía.", Country = "Colombia", CountryCode = "CO" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CE", Description = "Cédula de extranjería.", Country = "Colombia, Perú", CountryCode = "CO, PE" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CEL", Description = "En caso de identificarse a través de la línea del móvil.", Country = "Colombia", CountryCode = "CO" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CEP", Description = "Comprobante Electrónico de Pago.", Country = "México", CountryCode = "MX" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CI", Description = "Cédula de Identidad.", Country = "Argentina, Chile", CountryCode = "AR, CL" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CNPJ", Description = "Registro Nacional de Personas Jurídicas.", Country = "Brasil", CountryCode = "BR" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CPF", Description = "Registro de Personas físicas.", Country = "Brasil", CountryCode = "BR" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CUIL", Description = "Código Único de Identificación Laboral.", Country = "Argentina", CountryCode = "AR" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CUIT", Description = "Código Único de Identificación Tributaria.", Country = "Argentina", CountryCode = "AR" });
        DocumentTypes.Add(new PayuDocumentType { Code = "CURP", Description = "Clave Única de Registro de Población.", Country = "México", CountryCode = "MX" });
        DocumentTypes.Add(new PayuDocumentType { Code = "DE", Description = "Documento de identificación extranjero.", Country = "Perú", CountryCode = "PE" });
        DocumentTypes.Add(new PayuDocumentType { Code = "DL", Description = "Licencia de Conducción.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "DNI", Description = "Documento Nacional de Identidad.", Country = "Argentina, Perú, Chile", CountryCode = "AR, PE, CL" });
        DocumentTypes.Add(new PayuDocumentType { Code = "DNIE", Description = "Documento Nacional de Identidad - Electrónico.", Country = "Argentina, Perú, Chile", CountryCode = "AR, PE, CL" });
        DocumentTypes.Add(new PayuDocumentType { Code = "EIN", Description = "Número de identificación del empleador.", Country = "Perú", CountryCode = "PE" });
        DocumentTypes.Add(new PayuDocumentType { Code = "ID", Description = "Identificación.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "IDC", Description = "Identificador único de cliente, para el caso de ID’s únicos de clientes / usuarios de servicios públicos.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "IFE", Description = "Instituto Federal Electoral.", Country = "México", CountryCode = "MX" });
        DocumentTypes.Add(new PayuDocumentType { Code = "LC", Description = "Libreta Cívica.", Country = "Argentina", CountryCode = "AR" });
        DocumentTypes.Add(new PayuDocumentType { Code = "LE", Description = "Libreta de Enrolamiento.", Country = "Argentina", CountryCode = "AR" });
        DocumentTypes.Add(new PayuDocumentType { Code = "NIF", Description = "Número de Identificación Financiera.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "NIT", Description = "Número de Identificación Tributaria.", Country = "Colombia", CountryCode = "CO" });
        DocumentTypes.Add(new PayuDocumentType { Code = "PP", Description = "Pasaporte.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RC", Description = "Registro civil de nacimiento.", Country = "Colombia", CountryCode = "CO" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RDE", Description = "Tipo de documento RDE.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RE", Description = "Tipo de documento RE.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RFC", Description = "Registro Federal de Contribuyentes.", Country = "México", CountryCode = "MX" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RIF", Description = "Registro de Información Fiscal.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RM", Description = "Registro Mercantil.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RMC", Description = "Registro Mercantil Consular.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RNC", Description = "Registro Nacional de Contribuyentes.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RUC", Description = "Registro Único de Contribuyentes.", Country = "Perú", CountryCode = "PE" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RUN", Description = "Rol Único Nacional.", Country = "Chile", CountryCode = "CL" });
        DocumentTypes.Add(new PayuDocumentType { Code = "RUT", Description = "Rol Único Tributario.", Country = "Chile", CountryCode = "CL" });
        DocumentTypes.Add(new PayuDocumentType { Code = "SC", Description = "Salvoconducto.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "SIEM", Description = "Sistema de Información Empresarial Mexicano.", Country = "México", CountryCode = "MX" });
        DocumentTypes.Add(new PayuDocumentType { Code = "SSN", Description = "Número de Seguridad Social.", Country = "", CountryCode = "" });
        DocumentTypes.Add(new PayuDocumentType { Code = "TI", Description = "Tarjeta de Identidad.", Country = "Colombia", CountryCode = "CO" });
    }
}

/// <summary>
/// Represents a document type used in the PayU payment gateway.
/// </summary>
public class PayuDocumentType
{
    /// <summary>
    /// Código del tipo de documento.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Descripción del tipo de documento.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Países donde se utiliza este tipo de documento.
    /// </summary>
    public string Country { get; set; } = null!;
    /// <summary>
    /// Código del país donde se utiliza este tipo de documento.
    /// </summary>
    public string CountryCode { get; set; } = null!;
}