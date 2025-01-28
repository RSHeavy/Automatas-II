using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sintaxis_1 {
    class Program {
        static void Main(string[] args) {
            try {
                using Lenguaje l = new("Prueba.cpp");
                    /*while (!l.finArchivo()) {
                        l.nexToken();
                    }*/
                    l.Programa();
            } catch (Exception e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}