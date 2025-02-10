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
        [ 1) Implementar set y get para la clase Token                               ]
        ------------------------------------------------------------------------------
        [ 2) Implementar parámetros por default en el constructor del archivo Léxico ]
        ------------------------------------------------------------------------------
        [ 3) Implementar línea y columna en los errores Semanticos                   ]
        ------------------------------------------------------------------------------
        [ 4) Implementar maximoTipo en la asignación (cuando se haga v.setValor(r))  ]
        ------------------------------------------------------------------------------
        [ 5) Aplicar el casteo en el Stack                                           ]
        ------------------------------------------------------------------------------
         6) 
        ------------------------------------------------------------------------------
*/

namespace Semantica {
    public class Lenguaje : Sintaxis {
        Stack<float> s;
        List<Variable> l;
        Variable.TipoDato maximoTipo;
        public Lenguaje() : base() {
            s = new Stack<float>();
            l = new List<Variable>();
            maximoTipo = Variable.TipoDato.Char;
        }
        public Lenguaje(string name) : base(name) {
            s = new Stack<float>();
            l = new List<Variable>();
            maximoTipo = Variable.TipoDato.Char;
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
                        v.setValor(r, maximoTipo, linea, col); // Asignamos el último valor leído a la última variable detectada
                    } else {
                        match("ReadLine");
                        string? r = Console.ReadLine();
                        if (float.TryParse(r, out float valor)) {
                            v.setValor(valor, maximoTipo, linea, col);
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
                    
                    l.Last().setValor(resultado, maximoTipo, linea, col);   
                    
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
            //Cada vez que haya una asignacion para reiniciar el maximo tipo
            maximoTipo = Variable.TipoDato.Char;
            float nuevoValor = 0;
            Variable? v = l.Find(Variable => Variable.getNombre() == Contenido);

            if (v == null) {
                throw new Error("Sintaxis: La variable " + Contenido + " no esta definida", linea, col);
            }
            
            match(Tipos.Identificador);

            if (Contenido == "=") {
                match("=");
                if (Contenido == "Console") {
                    ListaIdentificadores(v.getTipoDato());
                } else {
                    //Console.WriteLine("Antes: " + maximoTipo);
                    Expresion();
                    //Console.WriteLine("Despues: " + maximoTipo);
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
            //Console.WriteLine("Maximo tipo: " + maximoTipo);
    
            v.setValor(nuevoValor, maximoTipo, linea, col);
            
            //DysplayStack();
        }

        //Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion() {
            maximoTipo = Variable.TipoDato.Char;
            Expresion();
            float v1 = s.Pop();

            String operador = Contenido;
            match(Tipos.OperadorRelacional);

            maximoTipo = Variable.TipoDato.Char;
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
                if (Clasificacion == Tipos.Cadena) {
                    texto = Contenido.Trim('"');
                    match(Tipos.Cadena);
                } else {
                    Variable? v = l.Find(Variable => Variable.getNombre() == Contenido);
                    if (v == null) {
                        throw new Error("Sintaxis: La variable " + Contenido + " no esta definida", linea, col);
                    } else {
                        texto = v.getValor().ToString();
                        match(Tipos.Identificador);
                    }
                }

                if (Contenido == "+") {
                    //Console.WriteLine("Paso por concatenaciones");
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

                if (maximoTipo < Variable.valorToTipoDato(float.Parse(Contenido))) {
                    maximoTipo = Variable.valorToTipoDato(float.Parse(Contenido));
                }

                s.Push(float.Parse(Contenido));
                //Console.Write(Contenido + " ");
                match(Tipos.Numero);
            } else if (Clasificacion == Tipos.Identificador) {
                Variable? v = l.Find(Variable => Variable.getNombre() == Contenido);
                if (v == null) {
                    throw new Error("Sintaxis: La variable " + Contenido + " no esta definida", linea, col);
                }
                
                if (maximoTipo < v.getTipoDato()) {
                    maximoTipo = v.getTipoDato();
                }

                s.Push(v.getValor());
                //Console.Write(Contenido + " ");
                match(Tipos.Identificador);
            } else {
                match("(");

                Variable.TipoDato tipoCasteo = Variable.TipoDato.Char;
                bool huboCasteo = false;

                if (Clasificacion == Tipos.TipoDato) {

                    switch(Contenido){
                        case "int": tipoCasteo = Variable.TipoDato.Int; break;
                        case "float": tipoCasteo = Variable.TipoDato.Float; break;
                    }
                    match(Tipos.TipoDato);
                    match(")");
                    match("(");
                    huboCasteo = true;
                }
                Expresion();
                if (huboCasteo) {
                    float resultado;
                    maximoTipo = tipoCasteo;

                    resultado = s.Pop();

                    switch(tipoCasteo) {
                        case Variable.TipoDato.Char:
                            resultado = resultado %  256;
                        
                        break;
                        case Variable.TipoDato.Int: 
                            resultado = resultado % 65536;
                        break;
                    }
                    s.Push(resultado);
                }
                match(")");
            }
        }

        //Concatenaciones -> Identificador | cadena (+ Concatenaciones)?                
                                          // sin comillas
        private void Concatenaciones(String texto, bool tipo, bool ejecuta) {
            match("+");

            if (Clasificacion == Tipos.Cadena) {
                texto += Contenido.Trim('"');
                match(Tipos.Cadena);
            } else {
                Variable? v = l.Find(Variable => Variable.getNombre() == Contenido);
                    if (v == null) {
                        throw new Error("Sintaxis: La variable " + Contenido + " no esta definida", linea, col);
                    } else {
                        texto += v.getValor().ToString();
                        match(Tipos.Identificador);
                    }
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
/*
    public Lexico (String nombreArchivo = " prueba.cpp", String log = "pueba.log") {

    }
*/