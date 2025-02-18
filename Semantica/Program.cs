using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Semantica {
    class Program : Token{
        static void Main(string[] args) {
            try {
                using Lenguaje l = new("Main.cpp");
                l.Programa();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}