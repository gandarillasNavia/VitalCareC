using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Dominio.Validadores;
using MSProductos.Aplicacion.InputPorts;
using MSProductos.Aplicacion.Helpers;

namespace MSProductos.Aplicacion.UseCases
{
    public class ClasificacionInteractor : IClasificacionInputPort
    {
        private readonly IClasificacionRepository _repository;
        private readonly IResult<Clasificacion> _validador;

        public ClasificacionInteractor(
            IClasificacionRepository repository,
            IResult<Clasificacion> validador)
        {
            _repository = repository;
            _validador = validador;
        }

        public IEnumerable<Clasificacion> ObtenerTodos()
        {
            return _repository.GetAll();
        }

        public IEnumerable<Clasificacion> ObtenerTodos(string filtro)
        {
            return _repository.GetAll(filtro);
        }

        public Clasificacion? ObtenerPorId(int id)
        {
            return _repository.GetById(id);
        }

        public Result Crear(
            string nombre,
            string origen,
            string descripcion,
            int idUsuario
        )
        {
            var clasificacion = ConstruirClasificacion(
                0,
                nombre,
                origen,
                descripcion
            );

            clasificacion.IdUsuario = idUsuario;

            clasificacion.FechaRegistro =
                ObtenerFechaBolivia();

            var validacion = _validador.Validar(clasificacion);

            if (!validacion.IsSuccess)
                return validacion;

            if (_repository.ExisteNombreActivo(clasificacion.Nombre))
                return Result.Fail(
                    "Ya existe una clasificación activa con ese nombre."
                );

            if (_repository.Insert(clasificacion) <= 0)
                return Result.Fail(
                    "No se pudo registrar la clasificación."
                );

            return Result.Ok();
        }

        public Result Actualizar(
            int id,
            string nombre,
            string origen,
            string descripcion,
            int idUsuario
        )
        {
            var clasificacion = ConstruirClasificacion(
                id,
                nombre,
                origen,
                descripcion
            );

            clasificacion.IdUsuario = idUsuario;

            clasificacion.UltimaActualizacion =
                ObtenerFechaBolivia();

            var validacion = _validador.Validar(clasificacion);

            if (!validacion.IsSuccess)
                return validacion;

            if (_repository.ExisteNombreActivoExcluyendoId(
                id,
                clasificacion.Nombre
            ))
            {
                return Result.Fail(
                    "Ya existe otra clasificación activa con ese nombre."
                );
            }

            if (_repository.Update(clasificacion) <= 0)
                return Result.Fail(
                    "No se pudo actualizar la clasificación."
                );

            return Result.Ok();
        }

        public Result EliminarLogicamente(int id, int idUsuario)
        {
            
            if (_repository.TieneMedicamentosActivosAsociados(id))
                return Result.Fail(
                    "No se puede eliminar porque tiene medicamentos activos asociados."
                );

            var clasificacion = new Clasificacion
            {
                Id = id,
                IdUsuario = idUsuario,
                UltimaActualizacion = ObtenerFechaBolivia()
            };

            if (_repository.Delete(clasificacion) <= 0)
                return Result.Fail("No se pudo eliminar la clasificación.");

            return Result.Ok();
        }

        private Clasificacion ConstruirClasificacion(
            int id,
            string nombre,
            string origen,
            string descripcion
        )
        {
            return new Clasificacion
            {
                Id = id,

                Nombre = StringHelper.LimpiarEspacios(nombre),

                Origen = StringHelper.LimpiarEspacios(origen),

                Descripcion = StringHelper.LimpiarEspacios(descripcion)
            };
        }

        private static DateTime ObtenerFechaBolivia()
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "SA Western Standard Time"
            );
        }
    }
}