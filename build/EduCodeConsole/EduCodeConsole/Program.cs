using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduCodeCompiler;

namespace EduCodeConsole {
    class Program {
        static void Main(string[] args) {
            Compiler.Compile("UNCOMPILED.edc", "OUTFILE.cs", false);
            Console.ReadLine();
        }
    }
}
