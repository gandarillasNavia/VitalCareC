using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Repositorios;
using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.Infraestructura.Creadores
{
    [ExcludeFromCodeCoverage]
    public class UsuarioRepositoryCreator
    {
        public IUsuarioRepository CreateRepo()
        {
            return new UsuarioRepository();
        }
    }
}
