using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Sintaxis_1 {
    public class Error : Exception
    {
        public Error(string message, int line) : base( message +" en linea " + line) { }
        public Error(string message, StreamWriter log) : base("Error de" + message) {
            log.WriteLine("Error de" + message);
        }
        public Error(string message, StreamWriter log, int line) : base("Error de " + message + ", linea " + line) {
            log.WriteLine("Error de " + message + " linea " + line);
        }
    }
}