namespace MSProductos.Dominio.Validadores
{
    public interface IResult<in T>
    {
        Result Validar(T entidad);
    }
}