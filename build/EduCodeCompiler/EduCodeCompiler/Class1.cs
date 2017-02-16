using System;
using System.IO;


//Parsing: the source text is converted to an abstract syntax tree(AST).
//Resolution of references to other modules(C postpones this step till linking).
//Semantic validation: weeding out syntactically correct statements that make no sense, e.g.unreachable code or duplicate declarations.
//Equivalent transformations and high-level optimization: the AST is transformed to represent a more efficient computation with the same semantics.This includes e.g.early calculation of common subexpressions and constant expressions, eliminating excessive local assignments, etc.
//Code generation: the AST is transformed into linear low-level code, with jumps, register allocation and the like.Some function calls can be inlined at this stage, some loops unrolled, etc.
//Peephole optimization: the low-level code is scanned for simple local inefficiencies which are eliminated.


namespace EduCodeCompiler
{
    public static class Compiler {

        private static class Interpreter {

            
            private static class Parser
            {
                private static Lexer.Token[] array;
                private static uint counter = 0;


                public static void Parse(Lexer.Token[] ary/*some array*/) {
                    //something
                    array = ary;
                    ParseNext();
                }

                private static void ParseNext() {

                }
            }

            private static class Lexer
            {
                public class Token {
                    private TokenType type;
                    private object value;

                    public Token(TokenType Type, object Value) {
                        type = Type;
                        value = Value;
                    }
                }

                public enum TokenType {
                    String, //"hello";
                    Variable, //"variable x = 2;
                    Number, //1, 1.5;
                    Group, //[5,4,3,2,1];
                    Semicolon, //;
                    ParseEnder, //STRICT_END
                    Assignment, //=
                    Comparison, //<=>
                    Modifier, //*, +, -, /
                    Grouper, //[
                    GroupEnd, //]
                    StringStart, //"
                    StringEnd, //,"
                      
                }
            }

            //Generates the horribly made C# code
            private static class Generator {
               
            }

            //Executes a flow based on Token Parsing
            private static class Executor {

            }
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
        
        public static void Compile(string fileOfUncompiled, string directoryToOutput, bool isExecuting) {
            try {
                string text = File.ReadAllText(fileOfUncompiled);

            }
            catch(IOException inputError) {
                Debug.ParseExecption(inputError);
            }
        }
    }
}
