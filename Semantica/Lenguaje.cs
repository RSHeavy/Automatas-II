using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

/*
    - REQUERIMIENTOS -
        ------------------------------------------------------------------------------
         1)
        ------------------------------------------------------------------------------
         2) 
        ------------------------------------------------------------------------------
         3)                                                
        ------------------------------------------------------------------------------
         4) 
        ------------------------------------------------------------------------------
         5) 
        ------------------------------------------------------------------------------
         6) 
        ------------------------------------------------------------------------------
*/

namespace Semantica {
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
            if (Contenido == "using") {
                Librerias();
            }

            if (Clasificacion == Tipos.TipoDato) {
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

            if (Contenido == "using") {
                Librerias();
            }
        }

        //Variables -> tipo_dato Lista_identificadores; Variables?
        private void Variables() {
            Variable.TipoDato t = Variable.TipoDato.Char;

            switch (Contenido) {
                case "int":   t = Variable.TipoDato.Int; break;
                case "float": t = Variable.TipoDato.Float; break;
            }
            
            match(Tipos.TipoDato);
            ListaIdentificadores(t);
            match(";");

            if (Clasificacion == Tipos.TipoDato) {
                Variables();
            }
        }

        //ListaLibrerias -> identificador (.ListaLibrerias)?
        private void ListaLibrerias() {
            match(Tipos.Identificador);

            if (Contenido == ".") {
                match(".");
                ListaLibrerias();
            }
        }

        //ListaIdentificadores -> identificador (= Expresion)? (, ListaIdentificadores)?
        private void ListaIdentificadores(Variable.TipoDato t) {

            if (l.Find(variable => variable.getNombre() == Contenido) != null) {
                throw new Error($"La variable {Contenido} ya existe", linea, col);
            }

            Variable v = new Variable(t,Contenido);
            l.Add(v);

            match(Tipos.Identificador);
            if (Contenido == "=") {
                match("=");
                if (Contenido == "Console") {
                    match("Console");
                    match(".");
                    if (Contenido == "Read") {
                        match("Read");
                        int r = Console.Read();
                        v.setValor(r); // Asignamos el último valor leído a la última variable detectada
                    } else {
                        match("ReadLine");
                        string? r = Console.ReadLine();
                        if (float.TryParse(r, out float valor)) {
                            v.setValor(valor);
                        } else {
                            throw new Error("Sintaxis. No se ingresó un número ", linea, col);
                        }
                    }
                    match("(");
                    match(")");
                } else {
                    // Como no se ingresó un número desde el Console, entonces viene de una expresión matemática
                    Expresion();
                    float resultado = s.Pop();
                    l.Last().setValor(resultado);
                }
            }
            if (Contenido == ",") {
                match(",");
                ListaIdentificadores(t);
            }
        }

        //BloqueInstrucciones -> { listaIntrucciones? }
        private void BloqueInstrucciones(bool ejecuta) {
            match("{");
            if (!(Contenido == "}")) {
                ListaInstrucciones(ejecuta);
            } else {
                match("}");
            }   
        }

        //ListaInstrucciones -> Instruccion ListaInstrucciones?
        private void ListaInstrucciones(bool ejecuta) {
            Instruccion(ejecuta);

            if (Contenido != "}") {
                ListaInstrucciones(ejecuta);
            } else {
                match("}");
            }
        }

        //Instruccion -> console | If | While | do | For | Variables | Asignación
        private void Instruccion(bool ejecuta) {            
            if (Contenido == "Console") {
                console(ejecuta);
            } else if (Contenido == "if") {
                If(ejecuta);
            } else if (Contenido == "while") {
                While();
            } else if (Contenido == "do") {
                Do();
            } else if (Contenido == "for") {
                For();
            } else if (Clasificacion == Tipos.TipoDato) {
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
            Variable? v = l.Find(Variable => Variable.getNombre() == Contenido);

            if (v == null) {
                 throw new Error("Sintaxis: La variable " + Contenido + " no esta definida", linea, col);
            }
            //Console.Write(Contenido + " = ");
            
            match(Tipos.Identificador);

            if (Contenido == "=") {
                match("=");
                if (Contenido == "Console") {
                    ListaIdentificadores(v.getTipoDato());
                } else {
                    Expresion();
                    nuevoValor = s.Pop();
                }
            } else {
                switch (Contenido) {
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
            String operador = Contenido;
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

            match(")");

            if (Contenido == "{") {
                BloqueInstrucciones(ejecuta);
            } else {
                Instruccion(ejecuta);
            }

            if (Contenido == "else") {
                bool ejecutarElse = ejecuta2 && !ejecuta;
                match("else");
                if (Contenido == "{") {
                    BloqueInstrucciones(ejecutarElse);
                } else {
                    Instruccion(ejecutarElse);
                }
            }
        }
        
        //While -> while(Condicion) bloqueInstrucciones | instruccion
        private void While() {
            match("while");
            match("(");
            Condicion();
            match(")");

            if (Contenido == "{") {
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

            if (Contenido == "{") {
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

            if (Contenido == "{") {
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
            
            if (Contenido == "Write") {
                tipo = true;
                match("Write");
            } else {
                match("WriteLine");
            } 
           
            match("(");

            if (!tipo && Contenido == ")") {
                match(")");
                match(";");
                Console.WriteLine();
            } else {
                texto = Contenido.Trim('"');

                if (Clasificacion == Tipos.Cadena) {
                    match(Tipos.Cadena);
                } else {
                    match(Tipos.Identificador);
                }

                if (Contenido == "+") {
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
            if (Clasificacion == Tipos.OperadorTermino) {
                String operador = Contenido;
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
            if (Clasificacion == Tipos.OperadorFactor) {
                String operador = Contenido;
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
            if (Clasificacion == Tipos.Numero) {
                s.Push(float.Parse(Contenido));
                //Console.Write(Contenido + " ");
                match(Tipos.Numero);
            } else if (Clasificacion == Tipos.Identificador) {
                Variable? v = l.Find(Variable => Variable.getNombre() == Contenido);
                if (v == null) {
                throw new Error("Sintaxis: La variable " + Contenido + " no existe", linea, col);
                }

                s.Push(v.getValor());
                //Console.Write(Contenido + " ");
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

            texto += Contenido.Trim('"');

            if (Clasificacion == Tipos.Cadena) {
                match(Tipos.Cadena);
            } else {
                match(Tipos.Identificador);
            }
            
            if (Contenido == "+") {
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