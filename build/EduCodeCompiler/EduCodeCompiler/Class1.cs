using System;
using System.IO;
using System.Collections.Generic;


//Parsing: the source text is converted to an abstract syntax tree(AST).
//Resolution of references to other modules(C postpones this step till linking).
//Semantic validation: weeding out syntactically correct statements that make no sense, e.g.unreachable code or duplicate declarations.
//Equivalent transformations and high-level optimization: the AST is transformed to represent a more efficient computation with the same semantics.This includes e.g.early calculation of common subexpressions and constant expressions, eliminating excessive local assignments, etc.
//Code generation: the AST is transformed into linear low-level code, with jumps, register allocation and the like.Some function calls can be inlined at this stage, some loops unrolled, etc.
//Peephole optimization: the low-level code is scanned for simple local inefficiencies which are eliminated.


namespace EduCodeCompiler
{
    public static class Compiler {
        private static List<Variable> variables;


        private class Variable {
            public enum VariableType {
                String,
                Number,
                NumberGroup,
                StringGroup
            }

            private readonly VariableType type;
            private readonly string name;
            private object value;

            public Variable(string name, VariableType type, object value) {
                this.name = name;
                this.type = type;
                this.value = value;

                switch (type) {
                    case VariableType.String:
                        break;
                    case VariableType.Number:
                        break;
                    case VariableType.NumberGroup:
                        break;
                    case VariableType.StringGroup:
                        break;
                }
            }

            public void Reassign(object value) {
                //Check if types match
                //Group values will have to debug write differently
                Debug.WriteToDebugFile($"Reassigned variable {name} to {value}");
            }

            public string Name {
                get { return name; }
            }

            public bool Is(Variable other) {
                return other.name == name;
            }

            public override bool Equals(object other) {
                return value.Equals(other); //Will have to create equals method for groups
            }


            //TODO: CHECK IF I DID EVERYTHING I NEEDED TO DO
        }

        private static void StoreVariable(Variable var) {
            if (variables == null) variables = new List<Variable>();
            variables.Add(var);
            Debug.WriteToDebugFile($"Stored variable: {var.Name}");
        }



        private static void BeginParse(string[] parts) {
            int index = 0;
            while (index < parts.Length) {
                string part = parts[index];
                switch(part) {
                    case "variable":
                        index = RunThroughVarAssignment(parts, index);
                        break;
                    case ";":
                        //should we require semicolons? would be required after assignment, or print
                        break;

                    case "print":
                        index = RunThroughPrint(parts, index);
                        break;
                    default:
                        if (VarExists(part)) {
                            index = RunThroughVarReassignment(parts, index);
                            //for now this is the only other case
                        }
                        break;
                }
            }
        }

        //Returns the next unparsed index

        //variable x = 10;
        private static int RunThroughVarAssignment(string[] parts, int index) {
            return index;
        }

        //x = 20;
        private static int RunThroughVarReassignment(string[] parts, int index) {
            return index;
        }

        //Print x, Print 7, Print 900.
        private static int RunThroughPrint(string[] parts, int index) {
            return index;
        }

        //IE: 7+7+7+7+7
        private static int GroupModifications(string parts, int index) {
            return index;
        }

        private static bool VarExists(string varName) {
            foreach(Variable vari in variables) {
                if (vari.Name == varName) {
                    return true;
                }
            }
            return false;
        }

            //Generates the horribly made C# code
            private static class Generator {
               
            }

            //Executes a flow based on Parsing
            private static class Executor {

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

            internal static void WriteToDebugFile(string message) {
                File.OpenWrite(outFile);
                File.WriteAllText(outFile, message);
            }

        }
        
        public static void Compile(string fileOfUncompiled, string directoryToOutput, bool isExecuting) {
            try {
                string text = File.ReadAllText(fileOfUncompiled);
                string[] textInParts = text.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                BeginParse(textInParts);
            }
            catch(IOException inputError) {
                Debug.ParseExecption(inputError);
            }
        }
    }
}
