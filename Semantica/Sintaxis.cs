using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Semantica {
    public class Sintaxis : Lexico {
        public Sintaxis() : base() {
            nexToken();
        }
        public Sintaxis(string name) : base(name) {
            nexToken();
        }
        public void match(string contenido) {
            if (contenido == Contenido) {
                nexToken();
            } else {
                throw new Error("Sintaxis: se espera un " + contenido, linea, col);
            }
        }
        public void match(Tipos clasificacion) {
            if (clasificacion == Clasificacion) {
                nexToken();
            } else {
                throw new Error("Sintaxis: se esperaba un " + clasificacion, linea, col);
            }
        }
    }
}