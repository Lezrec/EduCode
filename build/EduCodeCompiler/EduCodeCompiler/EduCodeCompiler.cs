using System;
using System.IO;
using System.Collections.Generic;


//Parsing: the source text is converted to an abstract syntax tree(AST).
//Resolution of references to other modules(C postpones this step till linking).
//Semantic validation: weeding out syntactically correct statements that make no sense, e.g.unreachable code or duplicate declarations.
//Equivalent transformations and high-level optimization: the AST is transformed to represent a more efficient computation with the same semantics.This includes e.g.early calculation of common subexpressions and constant expressions, eliminating excessive local assignments, etc.
//Code generation: the AST is transformed into linear low-level code, with jumps, register allocation and the like.Some function calls can be inlined at this stage, some loops unrolled, etc.
//Peephole optimization: the low-level code is scanned for simple local inefficiencies which are eliminated.


namespace EduCodeCompiler {
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
            foreach (Variable vr in variables) {
                if (var.Name == vr.Name) {
                    //uh oh, cant instantiate 2 variables of the same name!
                    throw new Exception(); //todo make this actually decent
                }

            }
            variables.Add(var);
            Debug.WriteToDebugFile($"Stored variable: {var.Name}");
            //TODO check for no repeat variables
        }

        private static void ChangeVarValue(string name, object value) {
            foreach (Variable var in variables) {
                if (var.Name == name) {
                    var.Reassign(value);
                    break;
                }
            }
        }



        private static void BeginParse(string[] parts) {
            int index = 0;
            while (index < parts.Length) {
                string part = parts[index];
                switch (part) {
                    case "variable":
                        index = RunThroughVarAssignment(parts, index);
                        break;
                    case ";":
                        //should we require semicolons? would be required after assignment, or print
                        //if not just keep this break case
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
            Debug.WriteToDebugFile("Successful parsing.");

        }

        //These methods return the next unparsed index

        //variable x = 10;
        private static int RunThroughVarAssignment(string[] parts, int index) {
            index++;
            int tryparse = 0;
            if (int.TryParse(parts[index], out tryparse)) {
                //This raises an error, the variable shouldn't be named a number
            }
            else if (parts[index] == "=") {
                //This raises an error, there is no variable name
            }
            else {
                string varName = parts[index];
                index++;
                if (parts[index] != "=") {
                    //This raises an error, there is no equals sign
                }
                else {
                    index++;
                    double valIfDouble;
                    int valIfInt;
                    string valIfString;

                    if (parts[index].Substring(0, 1) == "\"") {
                        string valIsStr = "";
                        int len = parts[index].Length;
                        if (parts[index].Substring(1, len - 1).Contains("\"")) {
                            //1 word/number/thing

                            int endVal = parts[index].Substring(1, len - 1).IndexOf("\"");
                            valIsStr = parts[index].Substring(1, endVal);
                        }
                        else {
                            //has spaces or no ending (error)
                            valIsStr += parts[index].Substring(1, len - 1);
                            while (!parts[index].Contains("\"")) {
                                valIsStr += " " + parts[index];
                                index++;
                                //will throw an error if no ending
                            }
                            if (parts[index] == "\"") {

                            }
                            else if (parts[index].Substring(parts[index].Length - 1, 1) != "\"") {
                                //this raises an exception
                            }
                            else {
                                int length = parts[index].Length;

                                valIsStr += parts[index].Substring(0, length - 1);
                            }

                            //goto gen and executor
                        }


                        //string check
                       
                        
                        Variable var = new Variable(varName, Variable.VariableType.String, valIsStr);
                        StoreVariable(var);
                    }
                    else if (double.TryParse(parts[index], out valIfDouble)) {
                        //double assignment
                        Variable var = new Variable(varName, Variable.VariableType.Number, valIfDouble);
                        StoreVariable(var);
                    }
                    else if (int.TryParse(parts[index], out valIfInt)) {
                        //int assignment
                        Variable var = new Variable(varName, Variable.VariableType.Number, valIfInt);
                        StoreVariable(var);
                    }
                    else if (parts[index].Substring(0, 1) == "[") {

                        //group assignment
                        //StoreVariable(var);
                    }
                    index++;
                }
            }

            return index;
        }

        //x = 20;
        private static int RunThroughVarReassignment(string[] parts, int index) {
            //case already checks if name exists
            string name = parts[index];
            index++;
            if (parts[index] != "=") {
                //error
            }
            else {
                index++;
                double valIfDouble;
                int valIfInt;
                string valIfString;
                Variable[] vars = variables.ToArray();

                if (parts[index].Substring(0, 1) == "\"") {
                    //string check
                    //TODO make illegal symbols
                    int len = parts[index].Length;
                    int endVal = parts[index].Substring(1, len - 1).IndexOf("\"");
                    valIfString = parts[index].Substring(1, endVal);
                    //Reassignment
                    for (int i = 0; i < vars.Length; i++) {
                        if (vars[i].Name == name) {
                            vars[i].Reassign(valIfString);
                        }
                    }

                }
                else if (double.TryParse(parts[index], out valIfDouble)) {
                    //double assignment
                    for (int i = 0; i < vars.Length; i++) {
                        if (vars[i].Name == name) {
                            vars[i].Reassign(valIfDouble);
                        }
                    }
                }
                else if (int.TryParse(parts[index], out valIfInt)) {
                    //int assignment
                    for (int i = 0; i < vars.Length; i++) {
                        if (vars[i].Name == name) {
                            vars[i].Reassign(valIfInt);
                        }
                    }
                }
                else if (parts[index].Substring(0, 1) == "[") {
                    //TODO
                    //group assignment
                    //variables.Add(var);
                }
                index++;
            }
            return index;
            //TODO THIS

        }

        //print 2, print 2.5, print [2,3,4,5], print ["2","a","c","e"]
        private static int RunThroughPrint(string[] parts, int index) {
            index++;
            if (VarExists(parts[index])) {
                //printing variable
                //go to gen and executor
            }
            else {
                double valIsNum;
                if (double.TryParse(parts[index], out valIsNum)) {
                    //go to gen and executor
                }
                else if (parts[index].Substring(0,1) == "\"") {

                    string valIsStr = "";
                    int len = parts[index].Length;
                    if (parts[index].Substring(1, len - 1).Contains("\"")) {
                        //1 word/number/thing
                        
                        int endVal = parts[index].Substring(1, len - 1).IndexOf("\"");
                        valIsStr = parts[index].Substring(1, endVal);
                    }
                    else {
                        //has spaces or no ending (error)
                        valIsStr += parts[index].Substring(1, len - 1);
                        while(!parts[index].Contains("\"")) {
                            valIsStr += " " + parts[index];
                            index++;
                            //will throw an error if no ending
                        }
                        if (parts[index] == "\"") {
                        
                        }
                        else if (parts[index].Substring(parts[index].Length-1, 1) != "\"") {
                            //this raises an exception
                        }
                        else {
                            int length = parts[index].Length;
                            
                            valIsStr += parts[index].Substring(0, length - 1);
                        }

                        //goto gen and executor
                    }
                }
                else {
                    //groups andor exception
                }
            }
            index++;
            return index;
            //should be easy enough
            //TODO THIS
        }

        //IE: 7+7+7+7+7
        private static int GroupModifications(string parts, int index) {
            return index;
            //f*** me this is going to be tricky
            //TODO THIS
        }

        private static bool VarExists(string varName) {
            foreach (Variable vari in variables) {
                if (vari.Name == varName) {
                    return true;
                }
            }
            return false;
        }

        //Generates the horribly made C# code
        private static class Generator {
            private static string totalFileText = "";
            private static string L = Environment.NewLine;
            private const string fileDir = "GENERATED.cs";
            public static void BeginMain() {
                if (!File.Exists(fileDir)) {
                    File.Create(fileDir);
                    File.OpenWrite(fileDir);
                    Debug.WriteToDebugFile("Gen file didn't exist, it does now!");
                }
                totalFileText += "using System;" + L + L + "public static void Main(string[] args) {" + L;
                Debug.WriteToDebugFile("Main function opened");

            }

            internal static void WriteToGenFile(string text) {
                totalFileText += text + L;
                Debug.WriteToDebugFile($"{text} was written to genfile");
            }

            public static void EndMain() {
                totalFileText += "}"; //LUL
                Debug.WriteToDebugFile("Main function closed");
            }
        }

        //Executes a flow based on Parsing
        private static class Executor {
            internal static void Print(object value) {
                Console.WriteLine(value);
            }
        }




        private static class Debug {

            private const string outFile = "DEBUG.txt";

            private static void Log(string message, string toString, string stackTrace) {
                //TODO THIS
            }

            internal static void ParseExecption(Exception exc) {
                string message = exc.Message;
                string toString = exc.ToString();
                string stackTrace = exc.StackTrace;
                //TODO THIS
            }

            internal static void Clear() {
                if (!File.Exists(outFile)) File.Create(outFile);
                else {
                    File.WriteAllText(outFile, "");
                }
                File.WriteAllText(outFile, "--DEBUG--");
            }

            internal static void WriteToDebugFile(string message) {
                if (!File.Exists(outFile)) File.Create(outFile);
                string txt = File.ReadAllText(outFile);
                //File.OpenWrite(outFile);
                string output = txt + Environment.NewLine + message;
                File.WriteAllText(outFile, output);
            }

        }

        public static void Compile(string fileOfUncompiled, string directoryToOutput, bool isExecuting /*This means "Am I executing or not?"*/) {
            try {
                Debug.Clear();
                if (!File.Exists(fileOfUncompiled)) { Debug.WriteToDebugFile("The specified .edc file does not exist"); throw new IOException("Specified .edc file does not exist."); /*throw new Exception("No file");*/ } //TODO MAKE THIS EXCEPTION BETTER
                else if (!File.Exists(directoryToOutput)) {
                    Debug.WriteToDebugFile("The specified .cs file does not exist.");
                    throw new IOException("Specified .cs file does not exist.");
                    /*throw new Exception("No file");*/
                }
                string text = File.ReadAllText(fileOfUncompiled);
                string[] textInParts = text.Split(new string[] { " ", "\n", ";", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                BeginParse(textInParts);
                //TODO OUTPUT TO OUTDIR (right now I have GENERATED.CS as outdir)
            }
            catch (IOException inputError) {
                Debug.ParseExecption(inputError);
                Console.WriteLine("Error compiling.\nCheck the debug file for more.");
            }
        }
    }
}
