using MSVentas.Dominio.Puertos.PuertoSalida;

namespace MSVentas.Infraestructura.Creadores
{
    public interface RepositoryCreator<T> //Clase creadora
    {
        public abstract IRepository<T> CreateRepo();

    }
}

