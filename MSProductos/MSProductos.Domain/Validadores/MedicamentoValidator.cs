using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MSProductos.Dominio.Validadores
{
    public class MedicamentoValidator : IResult<Medicamento>
    {
        public Result Validar(Medicamento medicamento)
        {
            return ValidarNombre(medicamento.Nombre)
                ?? ValidarPresentacion(medicamento.Presentacion)
                ?? ValidarIdClasificacion(medicamento.IdClasificacion)
                ?? ValidarConcentracion(medicamento.Concentracion)
                ?? ValidarPrecio(medicamento.Precio)
                ?? ValidarStock(medicamento.Stock)
                ?? ValidarIdUsuario(medicamento.IdUsuario)
                ?? Result.Ok();
        }

        private Result? ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Fail("El nombre del medicamento es obligatorio.");

            nombre = nombre.Trim();

            if (nombre.Length < 3 || nombre.Length > 100)
                return Result.Fail("El nombre debe tener entre 3 y 100 caracteres.");

            string patron = @"^[\p{L}0-9\s]+$";

            if (!Regex.IsMatch(
                    nombre,
                    patron,
                    RegexOptions.None,
                    TimeSpan.FromMilliseconds(100)))
            {
                return Result.Fail(
                    "El nombre del medicamento no debe contener signos ni caracteres especiales.");
            }

            return null;
        }

        private Result? ValidarPresentacion(string presentacion)
        {
            if (string.IsNullOrWhiteSpace(presentacion))
                return Result.Fail("La presentación es obligatoria.");

            presentacion = presentacion.Trim();

            if (presentacion.Length < 3 || presentacion.Length > 50)
                return Result.Fail("La presentación debe tener entre 3 y 50 caracteres.");

            string patron = @"^[\p{L}0-9\s]+$";
            if (!Regex.IsMatch(presentacion, patron))
                return Result.Fail("La presentación no debe contener signos ni caracteres especiales.");

            return null;
        }

        private Result? ValidarIdClasificacion(int idClasificacion)
        {
            if (idClasificacion <= 0)
                return Result.Fail("La clasificación es obligatoria.");

            return null;
        }

        private Result? ValidarConcentracion(string concentracion)
        {
            if (string.IsNullOrWhiteSpace(concentracion))
                return Result.Fail("La concentración es obligatoria.");

            concentracion = concentracion.Trim();

            if (concentracion.Length < 2 || concentracion.Length > 50)
                return Result.Fail("La concentración debe tener entre 2 y 50 caracteres.");

            return null;
        }

        private Result? ValidarPrecio(decimal precio)
        {
            if (precio < 0)
                return Result.Fail("El precio no puede ser negativo.");

            return null;
        }

        private Result? ValidarStock(int stock)
        {
            if (stock < 0)
                return Result.Fail("El stock no puede ser negativo.");

            return null;
        }

        private Result? ValidarIdUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                return Result.Fail("El usuario es obligatorio.");

            return null;
        }
    }
}
