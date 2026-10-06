using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Dominio.Validadores;
using MSProductos.Aplicacion.InputPorts;
using MSProductos.Aplicacion.Helpers;

namespace MSProductos.Aplicacion.UseCases
{
    public class MedicamentoInteractor : IMedicamentoInputPort
    {
        private readonly IMedicamentoRepository _repository;
        private readonly IResult<Medicamento> _validador;

        public MedicamentoInteractor(
            IMedicamentoRepository repository,
            IResult<Medicamento> validador)
        {
            _repository = repository;
            _validador = validador;
        }

        public IEnumerable<Medicamento> ObtenerTodos()
        {
            return _repository.GetAll();
        }

        public IEnumerable<Medicamento> ObtenerTodos(string filtro)
        {
            return _repository.GetAll(filtro);
        }

        public Medicamento? ObtenerPorId(int id)
        {
            return _repository.GetById(id);
        }

        public Result Crear(
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock,
            int idUsuario
        )
        {
            var medicamento = ConstruirMedicamento(
                0,
                nombre,
                presentacion,
                idClasificacion,
                concentracion,
                precio,
                stock
            );

            medicamento.IdUsuario = idUsuario;
            medicamento.FechaRegistro = ObtenerFechaBolivia();

            var validacion = _validador.Validar(medicamento);

            if (!validacion.IsSuccess)
                return validacion;

            if (_repository.ExisteNombreActivo(medicamento.Nombre))
                return Result.Fail(
                    "Ya existe un medicamento activo con ese nombre."
                );

            if (_repository.Insert(medicamento) <= 0)
                return Result.Fail(
                    "No se pudo registrar el medicamento."
                );

            return Result.Ok();
        }

        public Result Actualizar(
            int id,
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock,
            int idUsuario
        )
        {
            var medicamento = ConstruirMedicamento(
                id,
                nombre,
                presentacion,
                idClasificacion,
                concentracion,
                precio,
                stock
            );

            medicamento.IdUsuario = idUsuario;
            medicamento.UltimaActualizacion = ObtenerFechaBolivia();

            var validacion = _validador.Validar(medicamento);

            if (!validacion.IsSuccess)
                return validacion;

            if (_repository.ExisteNombreActivoExcluyendoId(
                id,
                medicamento.Nombre
            ))
            {
                return Result.Fail(
                    "Ya existe otro medicamento activo con ese nombre."
                );
            }

            if (_repository.Update(medicamento) <= 0)
                return Result.Fail(
                    "No se pudo actualizar el medicamento."
                );

            return Result.Ok();
        }

        public Result EliminarLogicamente(int id, int idUsuario)
        {
            var medicamento = new Medicamento
            {
                Id = id,
                IdUsuario = idUsuario,
                UltimaActualizacion = ObtenerFechaBolivia()
            };

            if (_repository.Delete(medicamento) <= 0)
                return Result.Fail("No se pudo eliminar el medicamento.");

            return Result.Ok();
        }

        private Medicamento ConstruirMedicamento(
            int id,
            string nombre,
            string presentacion,
            int idClasificacion,
            string concentracion,
            decimal precio,
            int stock
        )
        {
            return new Medicamento
            {
                Id = id,
                Nombre = StringHelper.LimpiarEspacios(nombre),
                Presentacion = StringHelper.LimpiarEspacios(presentacion),
                IdClasificacion = idClasificacion,
                Concentracion = StringHelper.LimpiarEspacios(concentracion),
                Precio = precio,
                Stock = stock
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
