using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Wompi;

/// <summary>
/// El tipo de documento viaja con el codigo DIAN del catalogo del pais (pendings/663): la configuracion que lo compara
/// usa ese codigo y no las abreviaturas viejas (CC, NIT).
/// </summary>
public class DocumentTypeCodesConfigurationTest
{
    [Fact]
    public void Appsettings_LegalIdTypeCodes_UsesDianCodes()
    {
        // Arrange
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CodeDesignPlus.Net.Microservice.Payments.sln")))
        {
            directory = directory.Parent;
        }

        var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(directory!.FullName, "src", "entrypoints", "CodeDesignPlus.Net.Microservice.Payments.Rest", "appsettings.json")).Build();

        // Act
        var codes = configuration.GetSection("Wompi:LegalIdTypeCodes").GetChildren().ToDictionary(x => x.Key, x => x.Value);

        // Assert
        Assert.Equal(("CC", "CE", "NIT", "PP", "TI"), (codes["13"], codes["22"], codes["31"], codes["41"], codes["12"]));
    }
}
