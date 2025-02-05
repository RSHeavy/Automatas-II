using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using Microsoft.VisualBasic;

namespace Semantica {
    public class Lexico : Token,  IDisposable {
        const int F = -1;
        const int E = -2;
        protected int linea = 1;
        protected int col = 0;
        protected StreamReader archivo;
        protected StreamWriter log;
        protected StreamWriter asm;

        int[,] TRAND = {
            {  0,  1,  2, 33,  1, 12, 14,  8,  9, 10, 11, 23, 16, 16, 18, 20, 21, 26, 25, 27, 29, 32, 34,  0,  F, 33  },
            {  F,  1,  1,  F,  1,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  2,  3,  5,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  E,  E,  4,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E  },
            {  F,  F,  4,  F,  5,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  E,  E,  7,  E,  E,  6,  6,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E  },
            {  E,  E,  7,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E  },
            {  F,  F,  7,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F, 13,  F,  F,  F,  F,  F, 13,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F, 13,  F,  F,  F,  F, 13,  F,  F,  F,  F,  F,  F, 15,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 17,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 19,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 19,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 22,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 24,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 24,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 24,  F,  F,  F,  F,  F,  F, 24,  F,  F,  F,  F,  F,  F,  F  },
            { 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28, 27, 27, 27, 27,  E, 27  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30  },
            {  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E,  E, 31,  E,  E,  E,  E,  E  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F, 32,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F  },
            {  F,  F,  F,  F,  F,  F,  F,  F,  F,  F,  F, 17, 36,  F,  F,  F,  F,  F,  F,  F,  F,  F, 35,  F,  F,  F  },
            { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35,  0, 35, 35  },
            { 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 37, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36  },
            { 36, 36, 36, 36, 36, 36, 35, 36, 36, 36, 36, 36, 37, 36, 36, 36, 36, 36, 36, 36, 36, 36,  0, 36, 36, 36  }
        };
        //Constructor de la clase lexico
        public Lexico(string nombreArchivo = "prueba.cpp", string archivoLog = "prueba.log", string ArchivoASM = "prueba.asm"){

            log = new StreamWriter(archivoLog);
            log.AutoFlush = true;

            if (File.Exists(nombreArchivo)) {
                archivo = new StreamReader(nombreArchivo);
            } else {
                throw new Error("El archivo " + nombreArchivo + " no existe", log);
            }

            if (Path.GetExtension(nombreArchivo) == ".cpp") {
                asm = new StreamWriter(ArchivoASM);
                asm.AutoFlush = true;
            } else {
                throw new Error("Extensión invalida del archivo " + nombreArchivo, log);
            }

            log.WriteLine("Programa: {0}", nombreArchivo);
            fechaHora();
        }
        /*//Constructor sobrecargado
        public Lexico(string nombreArchivo) { 
            log = new StreamWriter(Path.ChangeExtension(nombreArchivo,  ".log"));
            log.AutoFlush = true;

            if (File.Exists(Path.ChangeExtension(nombreArchivo,  ".cpp"))) {
                archivo = new StreamReader(nombreArchivo);
            } else {
                throw new Error("El archivo " + nombreArchivo + " no existe",  log);
            }

            if (Path.GetExtension(nombreArchivo) == ".cpp") {
                asm = new StreamWriter(Path.ChangeExtension(nombreArchivo,  ".asm"));
                asm.AutoFlush = true;
            } else {
                throw new Error("El archivo tiene extension invalida",  log);
            }
            log.WriteLine("Programa: {0}", nombreArchivo);
            fechaHora();
        }
        */
        //Destructor de la clase lexico
        public void Dispose() {
            log.WriteLine("Total de lineas {0}",  linea);

            log.Close();
            archivo.Close();
            asm.Close();
        }
        private int columna(char c)
        {
            if (c == '\n') {
                return 23;
            } else if (char.IsWhiteSpace(c)) {
                return 0; 
            } else if (char.ToLower(c) == 'e') {
                return 4;
            } else if (char.IsLetter(c)) {
                return 1;
            }  else if (char.IsDigit(c)) {
                return 2;
            } else if (c == '.') {
                return 3;
            } else if (c == '+') {
                return 5;
            } else if (c == '-') {
                return 6;
            } else if (c == ';') {
                return 7;
            } else if (c == '{') {
                return 8;
            } else if (c == '}') {
                return 9;
            } else if (c == '?') {
                return 10;
            } else if (c == '=') {
                return 11;
            } else if (c == '*') {
                return 12;
            } else if (c == '%') {
                return 13;
            } else if (c == '&') {
                return 14;
            } else if (c == '|') {
                return 15;
            } else if (c == '!') {
                return 16;
            } else if (c == '<') {
                return 17;
            } else if (c == '>') {
                return 18;
            } else if (c == '"') {
                return 19;
            } else if (c == '\'') {
                return 20;
            } else if (c == '#') {
                return 21;
            } else if (c == '/') {
                return 22;
            } else if (finArchivo()) {
                return 24;
            }
            return 25;
        }
        private void clasificacion(int estado) {
            switch (estado) {
                case 1: 
                    Clasificacion = Tipos.Identificador; 
                    break;
                case 2: 
                    Clasificacion =Tipos.Numero; 
                    break;
                case 8: 
                    Clasificacion =Tipos.FinSentencia; 
                    break;
                case 9: 
                    Clasificacion =Tipos.InicioBloque; 
                    break;
                case 10:
                    Clasificacion =Tipos.FinBloque; 
                    break;
                case 11:
                    Clasificacion =Tipos.OperadorTernario; 
                    break;
                case 12:
                case 14:
                    Clasificacion =Tipos.OperadorTermino; 
                    break;
                case 13:
                    Clasificacion =Tipos.IncrementoTermino; 
                    break;
                case 15:
                    Clasificacion =Tipos.Puntero; 
                    break;
                case 16:
                case 34: 
                    Clasificacion =Tipos.OperadorFactor; 
                    break;
                case 17: 
                    Clasificacion =Tipos.IncrementoFactor; 
                    break;
                case 18:
                case 20:
                case 29:
                case 32:
                case 33: 
                    Clasificacion =Tipos.Caracter; 
                    break;
                case 19:
                case 21: 
                    Clasificacion =Tipos.OperadorLogico; 
                    break;
                case 22:
                case 24:
                case 25:
                case 26: 
                    Clasificacion =Tipos.OperadorRelacional; 
                    break;
                case 23: 
                    Clasificacion =Tipos.Asignacion; 
                    break;
                case 27: 
                    Clasificacion =Tipos.Cadena; 
                    break;
            }
        }
        public void nexToken() {
            char c;
            string Buffer = "";
            int estado = 0;

            while (estado >= 0) {
                
                c = (char)archivo.Peek();
                estado = TRAND[estado,  columna(c)];
                clasificacion(estado);

                if (estado >= 0) {
                    archivo.Read();
                    col++;
                    if (c == '\n') {
                        linea++;
                        col = 0;
                    }
                    if (estado > 0) {
                        Buffer += c;
                    } else  {
                        Buffer = "";
                    }
                }
            }
            if (estado == E) {
                String mensaje;

                if (Clasificacion == Tipos.Numero) {
                    mensaje ="Lexico, Se espera un digito";
                } else if (Clasificacion == Tipos.Cadena) {
                    mensaje = "Lexico, Se esperaban comillas";
                } else if (Clasificacion == Tipos.Caracter) {
                    mensaje = "Lexico, Se esperaba una comilla";
                } else {
                    mensaje = "Lexico, Se esperaba cierre de comentario";
                }
                throw new Error(mensaje, log, linea);
            }
            
            Contenido = Buffer;

            if (Clasificacion == Tipos.Identificador) {
                switch(Contenido) {
                    case "char":
                    case "int":
                    case "float":
                        Clasificacion =(Tipos.TipoDato);
                        break;
                    case "if":
                    case "else":
                    case "do":
                    case "while":
                    case "for":
                        Clasificacion =(Tipos.PalabraReservada);
                        break;
                }
            }    
            if (!finArchivo()) {
                //log.WriteLine("{0}  °°°°  {1}",  getContenido(),  Clasificacion);
            }
        }
        public bool finArchivo() {
            return archivo.EndOfStream;
        }
        public void fechaHora() {
            DateTime fechaHoraActual = DateTime.Now;
        
            int year = fechaHoraActual.Year;
            int month = fechaHoraActual.Month;
            int day = fechaHoraActual.Day;

            int hour = fechaHoraActual.Hour; 
            int minute = fechaHoraActual.Minute;

            log.WriteLine("Fecha: {0}/{1}/{2}", day, month, year);
            log.WriteLine("Hora: {0}:{1}", hour, minute);
        }
    }
}
/*
    Expresion Regular: Metodo formal que através de una secuencia de caracteres 
    que define un PATRON de busqueda
    
        a) Reglas BNF
        b) Reglas BNF extendidas
        c) Operaciones aplicadas al lenguaje

    OAL

        1. Concatenación simple (·)
        2. Concatenación exponencial (^)
        3. Cerradura de Kleene (*)
        4. Cerradura positiva (+)
        5. Cerradura Epsilon (?)
        6. Operador OR (|)
        7. Parentesis()

        L = {A,  B,  C,  D,  E,  ...,  Z,  a,  b,  c,  d,  e,  ...,  z}
        D = {0,  1,  2,  3,  4,  5,  6,  7,  8,  9}

        1. L·D
            LD

        2. L^3 = LLL
           L^3D^2 = LLLDD

           D^5 = DDDDD
           =^2 = ==

        3. L* = Cero o más letras
           D* = Cero o más digítos

        4. L+ = Una o mas letras
           D+ = Uno o más digitos 

        5. L? = Cero o una letra (La letra es optativa-opcional)

        6. L | D = Letra o digito
           + | - = + ó - 

        7. (L D) L? = (Letra seguido de Digito y al final letra opcional)


    Producción Gramatical

        Clasificación del Token -> Expresión regular.

        Identificador -> L (L | D)* 

        Numero -> D+ (.D+)? (E(+ | -)? D+)?
        
        FinSentencia -> ;
        InicioBloque -> {
        FinBloque -> }
        OperadorTernario -> ?

        Puntero -> ->

        OperadorTermino -> + | -
        IncrementoTermino -> + (+ | =) | - (- | =)

        Termino + -> + (+ | =)?
        Termino - -> - (- | = | >)?

        OperadorFactor -> * | / | %
        IncrementoFactor -> *= | /= | %=

        Factor -> * | / | % (=)?
 
        OperadorLogico -> && | || | !

        NotOpRel -> ! (=)?
        
        Asignacion -> =

        AsigOpRel -> = (=)?

        OperadorRelacional -> > (=) ? | < (> | =)? | == | !=

        Cadena -> "c*"
        Caracter -> 'c' | #D* | lamda

    Automata: Modelo matematico que representa una expresion regular a travez de 
    un GRAFO,  para una maquina de estado finito que consiste en un conjunto de 
    estados bien definidos: 
        - Un estado inicial
        - Un alfabeto de entrada
        - Una funcion de transición
*/