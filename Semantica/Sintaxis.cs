using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sintaxis_1 {
    public class Sintaxis : Lexico {
        public Sintaxis() : base() {
            nexToken();
        }
        public Sintaxis(string name) : base(name) {
            nexToken();
        }
        public void match(string contenido) {
            if (contenido == getContenido()) {
                nexToken();
            } else {
                throw new Error("Sintaxis: se espera un " + contenido, line);
            }
        }
        public void match(Tipos clasificacion) {
            if (clasificacion == getClasificacion()) {
                nexToken();
            } else {
                throw new Error("Sintaxis: se esperaba " + clasificacion, line);
            }
        }
    }
}