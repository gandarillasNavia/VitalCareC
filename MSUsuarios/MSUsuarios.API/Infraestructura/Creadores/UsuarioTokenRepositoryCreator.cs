using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Repositorios;
using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.Infraestructura.Creadores
{
[ExcludeFromCodeCoverage] 
    public class UsuarioTokenRepositoryCreator
    {
        public IUsuarioTokenRepository CreateRepo()
        {
            return new UsuarioTokenRepository();
        }
    }
}
