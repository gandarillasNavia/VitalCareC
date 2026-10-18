using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Infraestructura.Creadores;
using MSVentas.Infraestructura.Persistencia.Repositorios;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Creadores
{
    public class VentaRepositoryCreatorTests
    {
        [Fact]
        public void CreateRepo_DebeRetornarUnVentaRepository()
        {
            // Act
            IVentaRepository resultado = VentaRepositoryCreator.CreateRepo();

            // Assert
            Assert.NotNull(resultado);
            Assert.IsType<VentaRepository>(resultado);
        }

        [Fact]
        public void CreateRepo_DebeRetornarUnaInstanciaNueva()
        {
            // Act
            IVentaRepository primero = VentaRepositoryCreator.CreateRepo();
            IVentaRepository segundo = VentaRepositoryCreator.CreateRepo();

            // Assert
            Assert.NotSame(primero, segundo);
        }
    }
}