
using MSVentas.Infraestructura.Persistencia.Conexion;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Persistencia.Conexion
{
    public class ConexionStringSingletonTests
    {
        [Fact]
        public void Instancia_DebeRetornarUnaInstanciaNoNula()
        {
            // Act
            ConexionStringSingleton resultado =
                ConexionStringSingleton.Instancia;

            // Assert
            Assert.NotNull(resultado);
        }

        [Fact]
        public void Instancia_DebeRetornarSiempreLaMismaInstancia()
        {
            // Act
            ConexionStringSingleton primera =
                ConexionStringSingleton.Instancia;

            ConexionStringSingleton segunda =
                ConexionStringSingleton.Instancia;

            // Assert
            Assert.Same(primera, segunda);
        }

        [Fact]
        public void CadenaConexion_DebeContenerLosParametrosNecesarios()
        {
            // Arrange
            ConexionStringSingleton instancia =
                ConexionStringSingleton.Instancia;

            // Act
            string cadena = instancia.CadenaConexion;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(cadena));
            Assert.Contains("server=", cadena);
            Assert.Contains("port=", cadena);
            Assert.Contains("database=", cadena);
            Assert.Contains("user=", cadena);
            Assert.Contains("password=", cadena);
        }

        [Fact]
        public void Instancia_DebeMantenerLaMismaReferenciaEnAccesosConcurrentes()
        {
            // Arrange
            var instancias =
                new ConexionStringSingleton[100];

            // Act
            Parallel.For(0, instancias.Length, i =>
            {
                instancias[i] = ConexionStringSingleton.Instancia;
            });

            // Assert
            ConexionStringSingleton primera = instancias[0];

            Assert.All(instancias, instancia =>
                Assert.Same(primera, instancia));
        }
    }
}
