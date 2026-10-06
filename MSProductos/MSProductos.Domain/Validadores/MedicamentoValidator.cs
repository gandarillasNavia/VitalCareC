using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MSProductos.Dominio.Validadores { 
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
                ?? 
                Result.Ok(); 
        } 
        private static Result? ValidarNombre(string nombre) 
        { 
            return ValidadorTexto.Obligatorio(nombre, "El nombre del medicamento es obligatorio.") 
                ?? ValidadorTexto.Longitud(nombre, 3, 100, "El nombre debe tener entre 3 y 100 caracteres.") 
                ?? ValidadorTexto.Patron(nombre, @"^[\p{L}0-9\s]+$", "El nombre del medicamento no debe contener signos ni caracteres especiales."); 
        } 
        private static Result? ValidarPresentacion(string presentacion) 
        { 
            return ValidadorTexto.Obligatorio(presentacion, "La presentación es obligatoria.") 
                ?? ValidadorTexto.Longitud(presentacion, 3, 50, "La presentación debe tener entre 3 y 50 caracteres.") 
                ?? ValidadorTexto.Patron(presentacion, @"^[\p{L}0-9\s]+$", "La presentación no debe contener signos ni caracteres especiales."); 
        } 
        private static Result? ValidarIdClasificacion(int idClasificacion) 
        { 
            if (idClasificacion <= 0) 
                return Result.Fail("La clasificación es obligatoria."); 
            return null; 
        } 
        private static Result? ValidarConcentracion(string concentracion) 
        { 
            return ValidadorTexto.Obligatorio(concentracion, "La concentración es obligatoria.") 
                ?? ValidadorTexto.Longitud(concentracion, 2, 50, "La concentración debe tener entre 2 y 50 caracteres."); 
        } 
        private static Result? ValidarPrecio(decimal precio) 
        { 
            if (precio < 0)
                return Result.Fail("El precio no puede ser negativo."); 
            return null; 
        } 
        private static Result? ValidarStock(int stock) 
        { 
            if (stock < 0) 
                return Result.Fail("El stock no puede ser negativo."); 
            return null; 
        } 
        private static Result? ValidarIdUsuario(int idUsuario) 
        { 
            if (idUsuario <= 0) 
                return Result.Fail("El usuario es obligatorio."); 
            return null; 
        } 
    } 
}

