using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Infraestructura.Persistencia.Repositorios;

namespace MSVentas.Infraestructura.Creadores
{
    public static class VentaRepositoryCreator
    {
        public static IVentaRepository CreateRepo()
        {
            return new VentaRepository();
        }
    }
}
