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
    public class ClasificacionValidador : IResult<Clasificacion> 
    { 
        public Result Validar(Clasificacion clasificacion) 
        { 
            return ValidarNombre(clasificacion.Nombre) 
                ?? ValidarOrigen(clasificacion.Origen) 
                ?? ValidarDescripcion(clasificacion.Descripcion) 
                ?? Result.Ok(); 
        } 
        private static Result? ValidarNombre(string nombre) 
        { 
            return ValidadorTexto.Obligatorio(nombre, "El nombre de la clasificación es obligatorio.") 
                ?? ValidadorTexto.Longitud(nombre, 3, 45, "El nombre debe tener entre 3 y 45 caracteres.") 
                ?? ValidadorTexto.Patron(nombre, @"^[\p{L}\s]+$", "El nombre de la clasificación solo debe contener letras y espacios.") 
                ?? ValidadorTexto.NoRepetido(nombre, "El nombre no puede estar compuesto por un único carácter repetido."); 
        } 
        private static Result? ValidarOrigen(string origen) 
        { 
            return ValidadorTexto.Obligatorio(origen, "El origen es obligatorio.") 
                ?? ValidadorTexto.Longitud(origen, 3, 45, "El origen debe tener entre 3 y 45 caracteres.") 
                ?? ValidadorTexto.Patron(origen, @"^[\p{L}0-9\s]+$", "El origen contiene caracteres inválidos."); 
        } 
        private static Result? ValidarDescripcion(string descripcion) 
        { 
            return ValidadorTexto.Obligatorio(descripcion, "La descripción es obligatoria.") 
                ?? ValidadorTexto.Longitud(descripcion, 5, 100, "La descripción debe tener entre 5 y 100 caracteres."); 
        } 
    } 
}