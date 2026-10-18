
using MSVentas.Infraestructura.Persistencia.Repositorios;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Persistencia.Repositorios
{
    public class VentaRepositoryTests
    {
        [Fact]
        public void Constructor_DebeCrearInstanciaCorrectamente()
        {
            // Act
            VentaRepository repositorio = new VentaRepository();

            // Assert
            Assert.NotNull(repositorio);
        }
    }
}
