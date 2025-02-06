using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Semantica {
    public class Variable {
        public enum TipoDato {
            Char,
            Int,
            Float,
        }

        TipoDato tipo;
        String nombre;
        float valor;

        public Variable (TipoDato tipo, String nombre, float valor = 0){
            this.tipo = tipo;
            this.nombre = nombre;
            this.valor = valor;
        }

        public void setValor(float valor, int linea, int col) {
            if (valorToTipoDato(valor) <= tipo) {
                this.valor = valor;
            } else {
                throw new Error("Semántico: no se puede asignar un " + valorToTipoDato(valor) + " a un " + tipo, linea, col);
            }
        }

        public static TipoDato valorToTipoDato(float valor) {
            if (!float.IsInteger(valor)) {
                return TipoDato.Float;
            } else if(valor <= 255) {
                return TipoDato.Char;
            } else if(valor <= 65535) {
                return TipoDato.Int;
            } else {
                return TipoDato.Float;
            }
         }

        public float getValor() {
            return valor;
        }

        public String getNombre () {
            return nombre;
        }

        public TipoDato getTipoDato() {
            return tipo;
        }
    }
}