using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
namespace MSProductos.Dominio.Validadores 
{ public static class ValidadorTexto { 
        private static readonly TimeSpan TimeoutRegex = TimeSpan.FromMilliseconds(100); 
        public static Result? Obligatorio(string valor, string mensaje) 
        { 
            if (string.IsNullOrWhiteSpace(valor)) 
                return Result.Fail(mensaje); 
            return null; 
        } 
        public static Result? Longitud(string valor, int minimo, int maximo, string mensaje) 
        { 
            valor = valor.Trim(); 
            if (valor.Length < minimo || valor.Length > maximo) 
                return Result.Fail(mensaje); 
            return null; 
        } 
        public static Result? Patron(string valor, string patron, string mensaje) 
        { 
            if (!Regex.IsMatch(valor.Trim(), patron, RegexOptions.None, TimeoutRegex)) 
            { 
                return Result.Fail(mensaje); 
            } 
            return null; 
        } 
        public static Result? NoRepetido(string valor, string mensaje) 
        {
            if (Regex.IsMatch(valor.Trim(), @"^(.)\1+$", RegexOptions.None, TimeoutRegex)) 
            { 
                return Result.Fail(mensaje); 
            } 
            return null; 
        } 
    } 
}
