using MSVentas.Dominio.Validadores;

namespace MSVentas.App.Interfaces
{
    public interface IResult<in T>
    {
        Result Validar(T entidad);
    }
}
