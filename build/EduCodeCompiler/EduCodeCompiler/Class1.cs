using System;
using System.IO;

namespace EduCodeCompiler
{
    public static class Compiler {

        private static class Interpreter {

        }

        private static class Debug {

            private const string outFile = "DEBUG.txt";

            private static void Log(string message, string toString, string stackTrace) {

            }

            internal static void ParseExecption(Exception exc) {
                string message = exc.Message;
                string toString = exc.ToString();
                string stackTrace = exc.StackTrace;
            }

            private static void WriteToDebugFile(string message) {
                File.OpenWrite(outFile);
                File.WriteAllText(outFile, message);
            }

        }
        
        public static void Compile(string fileOfUncompiled, string directoryToOutput) {
            try {
                string text = File.ReadAllText(fileOfUncompiled);

            }
            catch(IOException inputError) {
                Debug.ParseExecption(inputError);
            }
        }
    }
}
