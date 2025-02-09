using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Semantica {
    public class Error : Exception
    {
        public Error(string message) : base( message) { }
        public Error(string message, StreamWriter log) : base("Error " + message) {
            log.WriteLine("Error " + message);
        }
        public Error(string message, int linea, int col) : base( message +" en linea " + linea + " columna " + col) { }
        public Error(string message, StreamWriter log, int linea) : base("Error " + message + ", linea " + linea) {
            log.WriteLine("Error " + message + " linea " + linea);
        }
    }
}