using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

/*
    - REQUERIMIENTOS -
        ----------------------------------------------------------------------------
        [ 1) Concatenaciones                                                       ]
        ----------------------------------------------------------------------------
        [ 2) Inicializar una variable desde la declaración                         ]
        ----------------------------------------------------------------------------
        [ 3) Evaluar las expresiones                                               ]
        ----------------------------------------------------------------------------
        [ 4) Levantar excepcion si en el ReadLine no ingresan numeros (asignacion) ]
        ----------------------------------------------------------------------------
        [ 5) Modificar la variable con el restro de operadores                     ]
        [    (Incremento de factor y termino) (asignación)                         ]
        ----------------------------------------------------------------------------
         6) Implementar el else                                                    
        ----------------------------------------------------------------------------
*/

/*
    1. Concatenaciones (LISTO)
    2. Inicializar una variable desde la declaración (LISTO)
    3. Evaluar las expresiones matemáticas (LISTO)
    4. Levantar si en el Console.ReadLine() no ingresan números (LISTO)
    5. Modificar la variable con el resto de operadores (Incremento de factor y termino) (LISTO)
    6. Hacer que funcione el else
*/

namespace Sintaxis_1 {
    public class Lenguaje : Sintaxis {
        Stack<float> s;
        List<Variable> l;
        public Lenguaje() : base() {
            s = new Stack<float>();
            l = new List<Variable>();
        }
        public Lenguaje(string name) : base(name) {
            s = new Stack<float>();
            l = new List<Variable>();
        }

        private void DysplayStack() {
            Console.WriteLine("Contenido del Stack");
            foreach (float elemento in s) {
                Console.WriteLine(elemento);
            }
        }

        private void DysplayList() {
            log.WriteLine("Lista de variables");
            foreach (Variable elemento in l) {
                log.WriteLine("{0}  {1}  {2}", elemento.getTipoDato(), elemento.getNombre(), elemento.getValor());
            }
        }

        // ? Cerradura epsilon
        //Programa  -> Librerias? Variables? Main
        public void Programa() {
            if (getContenido() == "using") {
                Librerias();
            }

            if (getClasificacion() == Tipos.TipoDato) {
                Variables();
            }
            Main();
            DysplayList();
        }

        //Librerias -> using ListaLibrerias; Librerias?
        private void Librerias() {
            match("using");
            ListaLibrerias();
            match(";");

            if (getContenido() == "using") {
                Librerias();
            }
        }

        //Variables -> tipo_dato Lista_identificadores; Variables?
        private void Variables() {
            Variable.TipoDato t = Variable.TipoDato.Char;

            switch (getContenido()) {
                case "int":   t = Variable.TipoDato.Int; break;
                case "float": t = Variable.TipoDato.Float; break;
            }
            
            match(Tipos.TipoDato);
            ListaIdentificadores(t);
            match(";");

            if (getClasificacion() == Tipos.TipoDato) {
                Variables();
            }
        }

        //ListaLibrerias -> identificador (.ListaLibrerias)?
        private void ListaLibrerias() {
            match(Tipos.Identificador);

            if (getContenido() == ".") {
                match(".");
                ListaLibrerias();
            }
        }

        //ListaIdentificadores -> identificador (= Expresion)? (, ListaIdentificadores)?
        private void ListaIdentificadores(Variable.TipoDato t) {
            float r = 0;
            String nombreVariable = "";
            Variable? v = l.Find(Variable => Variable.getNombre() == getContenido());

            if (v == null) {
                nombreVariable = getContenido();
                match(Tipos.Identificador);


                if (getContenido() == "=") {
                    match("=");

                    if (getContenido() == "Console") {
                        match("Console");
                        match(".");

                        if (getContenido() == "Read") {
                            match("Read");
                            match("(");
                            match(")");
                            r = Console.Read();
                        } else {
                            match("ReadLine");
                            match("(");
                            match(")");
                        
                        if (float.TryParse(Console.ReadLine(),out float numero)) {
                            r = numero;
                        } else {
                            throw new Error("Se esperaba un numero", line);
                        }
                    }
                    } else {
                        Expresion();
                        r = s.Pop();
                    }
                }

                l.Add(new Variable(t, nombreVariable, r));

                if (getContenido() == ",") {
                    match(",");
                    ListaIdentificadores(t);
                }
            } else {
                 throw new Error("Sintaxis: La variable " + getContenido() + " ya existe", line);
            }
        }

        //BloqueInstrucciones -> { listaIntrucciones? }
        private void BloqueInstrucciones(bool ejecuta) {
            match("{");
            if (!(getContenido() == "}")) {
                ListaInstrucciones(ejecuta);
            } else {
                match("}");
            }   
        }

        //ListaInstrucciones -> Instruccion ListaInstrucciones?
        private void ListaInstrucciones(bool ejecuta) {
            Instruccion(ejecuta);

            if (getContenido() != "}") {
                ListaInstrucciones(ejecuta);
            } else {
                match("}");
            }
        }

        //Instruccion -> console | If | While | do | For | Variables | Asignación
        private void Instruccion(bool ejecuta) {            
            if (getContenido() == "Console") {
                console(ejecuta);
            } else if (getContenido() == "if") {
                If(ejecuta);
            } else if (getContenido() == "while") {
                While();
            } else if (getContenido() == "do") {
                Do();
            } else if (getContenido() == "for") {
                For();
            } else if (getClasificacion() == Tipos.TipoDato) {
                Variables();
            } else {
                Asignacion();
                match(";");
            }
        }
        
        //Asignacion -> id = Expresion | id++ | id-- | id  IncTermino expresion |                                           
                      //id IncrementoFactor Expresion | id = console.Read() | id = console.ReadLine()
        private void Asignacion() {
            float nuevoValor = 0;
            Variable? v = l.Find(Variable => Variable.getNombre() == getContenido());

            if (v == null) {
                 throw new Error("Sintaxis: La variable " + getContenido() + " no esta definida", line);
            }
            //Console.Write(getContenido() + " = ");
            
            match(Tipos.Identificador);

            if (getContenido() == "=") {
                match("=");
                if (getContenido() == "Console") {
                     match("Console");
                     match(".");

                    if (getContenido() == "Read") {
                        match("Read");
                        match("(");
                        match(")");
                        Console.Read();
                    } else {
                        match("ReadLine");
                        match("(");
                        match(")");
                        
                        if (float.TryParse(Console.ReadLine(),out float numero)) {
                            nuevoValor = numero;
                        } else {
                            throw new Error("Se esperaba un numero", line);
                        }
                    }
                } else {
                    Expresion();
                    nuevoValor = s.Pop();
                }
            } else {
                switch (getContenido()) {
                    case "++":
                        match(Tipos.IncrementoTermino);
                        nuevoValor = v.getValor() + 1; 
                        break;
                    case "--": 
                        match(Tipos.IncrementoTermino);
                        nuevoValor = v.getValor() - 1; 
                        break;
                    case "+=":
                        match(Tipos.IncrementoTermino);
                        Expresion();
                        nuevoValor = v.getValor() + s.Pop();
                        break;
                    case "-=": 
                        match(Tipos.IncrementoTermino);
                        Expresion();
                        nuevoValor = v.getValor() - s.Pop();
                        break;
                    case "*=":
                        match(Tipos.IncrementoFactor);
                        Expresion();
                        nuevoValor = v.getValor() * s.Pop();
                        break;
                    case "/=":
                        match(Tipos.IncrementoFactor);
                        Expresion();
                        nuevoValor = v.getValor() / s.Pop();
                        break;
                    case "%=": 
                        match(Tipos.IncrementoFactor);
                        Expresion();
                        nuevoValor = v.getValor() / s.Pop();
                        break;
                }
            }
            v.setValor(nuevoValor);
            //DysplayStack();
        }

        //Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion() {
            Expresion();
            float v1 = s.Pop();
            String operador = getContenido();
            match(Tipos.OperadorRelacional);
            Expresion();
            float v2 = s.Pop();

            switch (operador) {
                case ">": return v1 > v2;
                case ">=": return v1 >= v2;
                case "<": return v1 < v2;
                case "<=": return v1 <= v2;
                case "==": return v1 == v2;
                default: return v1 != v2;
            }
        }

        //If -> if (Condicion) bloqueInstrucciones | instruccion
             //(else bloqueInstrucciones | instruccion)?
        private void If(bool ejecuta2) {
            match("if");
            match("(");
            bool ejecuta = Condicion() && ejecuta2;
            Console.WriteLine(ejecuta);
            match(")");

            if (getContenido() == "{") {
                BloqueInstrucciones(ejecuta);
            } else {
                Instruccion(ejecuta);
            }

            if (getContenido() == "else") {
                match("else");
                if (getContenido() == "{") {
                    BloqueInstrucciones(!ejecuta);
                } else {
                    Instruccion(!ejecuta);
                }
            }
        }
        
        //While -> while(Condicion) bloqueInstrucciones | instruccion
        private void While() {
            match("while");
            match("(");
            Condicion();
            match(")");

            if (getContenido() == "{") {
                BloqueInstrucciones(true);
            } else {
                Instruccion(true);
            }
        }
        
        //Do -> do 
                //bloqueInstrucciones | intruccion 
                //while(Condicion);
        private void Do() {
            match("do");

            if (getContenido() == "{") {
                BloqueInstrucciones(true);
            } else {
                Instruccion(true);
            }

            match("while");
            match("(");
            Condicion();
            match(")");
            match(";");
        }
        
        //For -> for(Asignacion; Condicion; Asignacion) 
               //BloqueInstrucciones | Intruccion
        private void For() {
            match("for");
            match("(");
            Asignacion();
            match(";");
            Condicion();
            match(";");
            Asignacion();
            match(")");

            if (getContenido() == "{") {
                BloqueInstrucciones(true);
            } else {
                Instruccion(true);
            }
        }

        //console -> console.(WriteLine|Write) (cadena concatenaciones?);
        private void console(bool ejecuta) {
            bool tipo = false;
            String texto;

            match("Console");
            match(".");
            
            if (getContenido() == "Write") {
                tipo = true;
                match("Write");
            } else {
                match("WriteLine");
            } 
           
            match("(");

            if (!tipo && getContenido() == ")") {
                match(")");
                match(";");
                Console.WriteLine();
            } else {
                texto = getContenido().Trim('"');

                if (getClasificacion() == Tipos.Cadena) {
                    match(Tipos.Cadena);
                } else {
                    match(Tipos.Identificador);
                }

                if (getContenido() == "+") {
                    Concatenaciones(texto, tipo, ejecuta);
                } else {
                    match(")");
                    match(";");
                    if (ejecuta) {
                        if (tipo) {
                            Console.Write(texto);
                        } else {
                            Console.WriteLine(texto);
                        }
                    }
                }
            }
        }

        //Main      -> static void Main(string[] args) BloqueInstrucciones 
        private void Main() {
            match("static");
            match("void");
            match("Main");
            match("(");
            match("string");
            match("[");
            match("]");
            match("args");
            match(")");
            BloqueInstrucciones(true);
        }

        //Expresion -> Termino MasTermino
        private void Expresion() {
            Termino();
            MasTermino();
        }
        
        //MasTermino -> (OperadorTermino Termino)?
        private void MasTermino() {
            if (getClasificacion() == Tipos.OperadorTermino) {
                String operador = getContenido();
                match(Tipos.OperadorTermino);
                Termino();
                //Console.Write(operador + " ");

                float n1 = s.Pop();
                float n2 = s.Pop();

                switch (operador) {
                    case "+": s.Push(n2 + n1); break;
                    case "-": s.Push(n2 - n1); break;
                }
            }
        }
        
        //Termino -> Factor PorFactor
        private void Termino() {
            Factor();
            PorFactor();
        }
        
        //PorFactor -> (OperadorFactor Factor)?
        private void PorFactor() {
            if (getClasificacion() == Tipos.OperadorFactor) {
                String operador = getContenido();
                match(Tipos.OperadorFactor);
                Factor();
                //Console.Write(operador + " ");

                float n1 = s.Pop();
                float n2 = s.Pop();

                switch (operador) {
                    case "*": s.Push(n2 * n1); break;
                    case "/": s.Push(n2 / n1); break;
                    case "%": s.Push(n2 % n1); break;
                }
            }
        }
        
        //Factor -> numero | identificador | (Expresion)
        private void Factor() {
            if (getClasificacion() == Tipos.Numero) {
                s.Push(float.Parse(getContenido()));
                //Console.Write(getContenido() + " ");
                match(Tipos.Numero);
            } else if (getClasificacion() == Tipos.Identificador) {
                Variable? v = l.Find(Variable => Variable.getNombre() == getContenido());
                if (v == null) {
                 throw new Error("Sintaxis: La variable " + getContenido() + " no existe", log, line);
                }

                s.Push(v.getValor());
                //Console.Write(getContenido() + " ");
                match(Tipos.Identificador);
            } else {
                match("(");
                Expresion();
                match(")");
            }
        }

        //Concatenaciones -> Identificador | cadena (+ Concatenaciones)?                
                                          // sin comillas
        private void Concatenaciones(String texto, bool tipo, bool ejecuta) {
            match("+");

            texto += getContenido().Trim('"');

            if (getClasificacion() == Tipos.Cadena) {
                match(Tipos.Cadena);
            } else {
                match(Tipos.Identificador);
            }
            
            if (getContenido() == "+") {
                Concatenaciones(texto, tipo, ejecuta);
            } else {
                match(")");
                match(";");
                if (ejecuta) {
                    if (tipo) {
                        Console.Write(texto);
                    } else {
                        Console.WriteLine(texto);
                    }
                }
            }
        }
    }
}