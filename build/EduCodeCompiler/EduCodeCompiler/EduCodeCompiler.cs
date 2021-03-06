﻿using System;
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
        private static List<Variable> variables = new List<Variable>();
        private static List<Variable> loopVariables = new List<Variable>();
        private static bool isLooping = false;

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
                if (value is double || value is string) this.value = value;
                
                Debug.WriteToDebugFile($"Reassigned variable {name} to {value}");
            }

            public string Name {
                get { return name; }
            }

            public bool Is(Variable other) {
                return other.name == name;
            }

            public override bool Equals(object other) {
                Variable o = other as Variable;
                return o.ToString() == ToString();//Will have to create equals method for groups
            }

            

            public override string ToString() {
                //for now
                return name;
            }

            internal object Value { get { return value; } }
            //TODO: CHECK IF I DID EVERYTHING I NEEDED TO DO
        }

        private static void StoreLoopVariable(Variable var) {
            if (loopVariables == null) loopVariables = new List<Variable>();
            foreach (Variable vr in loopVariables) {
                if (var.Name == vr.Name) {
                    throw new IOException("Cannot instantiate 2 variables with the same name");
                }
            }
            loopVariables.Add(var);
            StoreVariable(var);
        }

        private static bool LoopVariablesContain(Variable var) {
            foreach(Variable vr in loopVariables) {
                if (vr.Name == var.Name) return true;
            }
            return false;
        }

        private static void ClearVarListFromLoopVars() {
            List<string> toRemove = new List<string>();
            foreach(Variable var in loopVariables) {
                toRemove.Add(var.Name);
            }
            foreach(string name in toRemove) {
                Variable removeVar = new Variable(name, Variable.VariableType.Number, 0);
                variables.Remove(removeVar);
            }
            loopVariables = new List<Variable>();
        }

        private static void StoreVariable(Variable var) {
            if (variables == null) variables = new List<Variable>();
            foreach (Variable vr in variables) {
                if (var.Name == vr.Name) {
                    if (isLooping && LoopVariablesContain(var)) {

                    }
                    //uh oh, cant instantiate 2 variables of the same name!
                    else throw new IOException("Cannot instantiate 2 variables with the same name"); //todo make this actually decent
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

        private static void ChangeLoopVarValue(string name, object value) {
            foreach (Variable var in variables) {
                if (var.Name == name) {
                    var.Reassign(value);
                    break;
                }
                
                }
            foreach (Variable var in loopVariables) {
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
                    case "square":
                        index = SquareVar(parts, index);
                        break;
                    case "cube":
                        index = CubeVar(parts, index);
                        break;
                    case "ln":
                        index =LnVar(parts, index);
                        break;
                    case "log":
                        index =Log10Var(parts, index);
                        break;
                    case "loop":
                        index = Loop(parts, index);
                        break;
                    case "if":
                        index = Conditional(parts, index);
                        break;
                    case ":":
                        index++;
                        break;
                    default:
                        if (VarExists(part)) {
                            index = RunThroughVarReassignment(parts, index);
                            //for now this is the only other case
                        }
                        
                        break;
                }
            }
            if (!isLooping) Debug.WriteToDebugFile("Successful parsing.");

        }

        

        //These methods return the next unparsed index
        private static int Conditional(string[] parts, int index) {
            index++;
            string varName = parts[index];
            index++;
            if (!VarExists(varName)) {
                throw new IOException($"No variable of name {varName}");
            }
            else {
                if (parts[index] != "equals") {
                    throw new IOException($"Expected equals, recieved {parts[index]}");
                }
                else {
                    Variable var = null;
                    foreach (Variable vr in variables) {
                        if (vr.Name == varName) {
                            var = vr;
                            break;
                        }
                    }
                    index++;
                    bool val;
                    if (var.Value.ToString() == parts[index]) { // for now this only works for numbers
                        val = true;
                    }
                    else {
                        val = false;
                    }
                    index++;
                    if (parts[index++] == "then" && parts[index++] == "do") {
                        if (parts[index] != ":") {
                            throw new IOException($"Expected : recieved {parts[index]}");
                        }
                        else {
                            index++;
                            List<string> cmdParts = new List<string>();
                            while (parts[index] != ":") {
                                cmdParts.Add(parts[index]);
                                index++;
                            }
                            if (val) BeginParse(cmdParts.ToArray());

                            else {
                                while(parts[index] != ":") {
                                    index++;
                                }
                                
                            }


                            
                        }
                    }
                    else {
                        throw new IOException("Expected then do after conditional");
                    }

                }
                return index;   
            }
        }


        private static int Loop(string[] parts, int index) {
            index++;
            string times = parts[index];
            int amt;
            if (!int.TryParse(times, out amt)) {
                throw new IOException($"Unrecognized token: {parts[index]}");
            }
            else {
                index++;
                if (parts[index] != ":") {
                    throw new IOException($"Expected \":\", recieved {parts[index]}");
                }
                index++;
                List<string> cmdParts = new List<string>();
                while(parts[index] != ":") {
                    cmdParts.Add(parts[index]);
                    index++;
                }
                isLooping = true;
                for(int i = 0; i < amt; i++) {
                    BeginParse(cmdParts.ToArray());
                    ClearVarListFromLoopVars();
                }
            }
            isLooping = false;
            
            return index;
        }
        
        private static int LnVar(string[] parts, int index) {
            index++;
            bool foundVar = false;
            for (int i = 0; i < variables.ToArray().Length; i++) {
                if (variables.ToArray()[i].Name == parts[index]) {
                    if (!(variables[i].Value is double) && !(variables[i].Value is int)) {
                        throw new IOException("Cannot natural log this type of variable");
                    }
                    variables[i].Reassign(Math.Log((double)variables[i].Value));
                    foundVar = true;
                    break;
                }
            }

            if (!foundVar) {
                throw new IOException($"No variable matched called {parts[index]}");
                //exception
            }
            else index++;
            return index;
        }

        private static int Log10Var(string[] parts, int index) {
            index++;
            bool foundVar = false;
            for (int i = 0; i < variables.ToArray().Length; i++) {
                if (variables.ToArray()[i].Name == parts[index]) {
                    if (!(variables[i].Value is double) && !(variables[i].Value is int)) {
                        throw new IOException("Cannot natural log this type of variable");
                    }
                    variables[i].Reassign(Math.Log10((double)variables[i].Value));
                    foundVar = true;
                    break;
                }
            }

            if (!foundVar) {
                throw new IOException($"No variable matched called {parts[index]}");
                //exception
            }
            else index++;
            return index;
        }

        private static int SquareVar(string[] parts, int index) {
            index++;
            bool foundVar = false;
            for(int i = 0; i < variables.ToArray().Length; i++) {
                if (variables.ToArray()[i].Name == parts[index]) {
                    if (!(variables[i].Value is double) && !(variables[i].Value is int)) {
                        throw new IOException("Cannot square this type of variable");
                    }
                    variables[i].Reassign(Math.Pow((double)variables[i].Value,2));
                    foundVar = true;
                    break;
                }
            }
            
            if (!foundVar) {
                throw new IOException($"No variable matched called {parts[index]}");
                //exception
            }
            else index++;
            return index;
        }

        private static int CubeVar(string[] parts, int index) {
            index++;
            bool foundVar = false;
            for (int i = 0; i < variables.ToArray().Length; i++) {
                if (variables.ToArray()[i].Name == parts[index]) {
                    if (!(variables[i].Value is double) && !(variables[i].Value is int)) {
                        throw new IOException("Cannot cube this type of variable");
                    }
                    variables[i].Reassign(Math.Pow((double)variables[i].Value, 3));
                    foundVar = true;
                    break;
                }
            }

            if (!foundVar) {
                throw new IOException($"No variable matched called {parts[index]}");
                //exception
            }
            else index++;
            return index;
        }

        //variable x = 10;
        private static int RunThroughVarAssignment(string[] parts, int index) {
            index++;
            int tryparse = 0;
            if (int.TryParse(parts[index], out tryparse)) {
                //This raises an error, the variable shouldn't be named a number
            }
            else if (parts[index] == "=") {
                //This raises an error, there is no variable name
                throw new IOException("Cannot name a variable = or have no variable name");
            }
            else {
                string varName = parts[index];
                switch(varName) {
                    case "e":
                        throw new IOException("Cannot assign a variable with name e (reserved name for the constant)");
                    case "ln":
                    case "log":
                        throw new IOException("Cannot assign a variable with name log/ln (reserved for the functions)");
                    case "square":
                        throw new IOException("Cannot assign a variable with name square (reserved for the function)");
                    case "cube":
                        throw new IOException("Cannot assign a variable with name cube (reserved for the function");
                }
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
                            index++;
                            valIsStr += " ";
                            while (!parts[index].Contains("\"")) {
                                valIsStr += parts[index] + " ";
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
                        if (!isLooping) StoreVariable(var);
                        else StoreLoopVariable(var);
                    }
                    else if (double.TryParse(parts[index], out valIfDouble)) {
                        //double assignment
                        Variable var = new Variable(varName, Variable.VariableType.Number, valIfDouble);
                        if (!isLooping) StoreVariable(var);
                        else StoreLoopVariable(var);
                    }
                    else if (int.TryParse(parts[index], out valIfInt)) {
                        //int assignment
                        //technically this should never happen as double is more inclusive than int
                        Variable var = new Variable(varName, Variable.VariableType.Number, valIfInt);
                        if (!isLooping) StoreVariable(var);
                        else StoreLoopVariable(var);
                    }
                    else if (parts[index].Substring(0, 1) == "[") {

                        //group assignment
                        //StoreVariable(var);
                    }
                    else if (parts[index].Contains("+") || parts[index].Contains("*") || parts[index].Contains("/") || parts[index].Contains("-")) {
                        //groupmodification
                        index = GroupModificationsNumbers(parts,index);
                        double.TryParse(parts[index], out valIfDouble);
                        Variable var = new Variable(varName, Variable.VariableType.Number, valIfDouble);
                        if (!isLooping) StoreVariable(var);
                        else StoreLoopVariable(var);
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
                            index++;
                            valIsStr += " ";
                            while (!parts[index].Contains("\"")) {
                                valIsStr += parts[index] + " ";
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


                            //Reassignment
                            for (int i = 0; i < vars.Length; i++) {
                                if (vars[i].Name == name) {
                                    vars[i].Reassign(valIsStr);
                                    break;

                                }
                            }
                        }
                    } //TODO TEST THIS

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
                else if (parts[index].Contains("+") || parts[index].Contains("*") || parts[index].Contains("/") || parts[index].Contains("-")) {
                    //groupmodification
                    index = GroupModificationsNumbers(parts, index);
                    double.TryParse(parts[index], out valIfDouble);
                    for (int i = 0; i < vars.Length; i++) {
                        if (vars[i].Name == name) {
                            vars[i].Reassign(valIfDouble);
                        }
                    }
                }
                index++;
            }
            return index;
            //TODO THIS

        }

        //print 2, print 2.5, print [2,3,4,5], print ["2","a","c","e"]
        private static int RunThroughPrint(string[] parts, int index) {
            if (variables == null) variables = new List<Variable>();
            index++;
            if (VarExists(parts[index])) {
                //printing variable
                //go to gen and executor
                Variable v = null; //this should never print out null
                foreach(Variable var in variables) {
                    if (var.Name == parts[index]) {
                        v = var;
                        break;
                    }
                }
                Executor.PrintVar(v);
            }
            else {
                double valIsNum;
                if (double.TryParse(parts[index], out valIsNum)) {
                    //go to gen and executor
                    Executor.Print($"{valIsNum}");
                }

                else if (parts[index].Contains("+") || parts[index].Contains("*") || parts[index].Contains("/") || parts[index].Contains("-")) {
                    //groupmodification
                    GroupModificationsNumbers(parts, index);
                    double.TryParse(parts[index], out valIsNum);
                    Executor.Print($"{valIsNum}");
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
                    Executor.Print(valIsStr);
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
        private static int GroupModificationsNumbers(string[] parts, int index) {
            //im gonna have this only apply to numbers because REDACTED anyone who does this with strings for now
            //instead of returning a tuple or some stuff like that ill just change parts[index] to the value
            //and the plusses will have to be together because im a meanie like that (actually its because if they had spaces that messes EVERYTHING UP [maybe ill include it later but I DONT HAVE TIME])
            string part = parts[index];
            double total = 0;
            string num = "";
            int anchor = 0;
            int startIndex;
            for(int i = 0; i < parts[index].Length; i++) {
                
                if (parts[index].Substring(anchor, i - anchor).Contains("+")) {
                    startIndex = i;
                    double val;
                    if (!double.TryParse(parts[index].Substring(anchor, i - anchor - 1), out val) && !VarExists(parts[index].Substring(anchor, i - anchor - 1))) {
                        
                        //exception
                    }
                    else {
                        if (VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                            for(int j = 0; j < variables.ToArray().Length; j++) {
                                if (variables[j].Name == parts[index].Substring(anchor, i - anchor - 1)) {
                                    if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                        throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                    }
                                    val = (double)variables[j].Value;
                                }
                                
                            }
                            
                        }
                        //look for next number
                        anchor = i;
                        total += val;
                        int len = 0;
                        while(!parts[index].Substring(anchor, len).Contains("+") && startIndex + len < parts[index].Length) {
                            len++;
                        }
                        int anchor2 = i - 1 + len;
                        i = len;
                        int len2 = anchor2 - anchor;
                        string oPart;
                        bool end;
                        if (startIndex + len == parts[index].Length) {
                            oPart = parts[index].Substring(startIndex);
                            end = true;
                        }else {
                            oPart = parts[index].Substring(anchor, len2);
                            end = false;
                        }
                            
                        double val2;
                        if (!double.TryParse(oPart, out val2) && !VarExists(oPart)) {

                            //exception
                        }
                        else {
                            if (VarExists(oPart)) {

                                for (int j = 0; j < variables.ToArray().Length; j++) {
                                    if (variables[j].Name == oPart) {
                                        if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                            throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                        }
                                        val2 = (double)variables[j].Value;
                                    }

                                }

                            }
                            total += val2;
                            if (!end) parts[index] = total + parts[index].Substring(anchor2);
                            else parts[index] = "" + total;
                        }
                        
                        total = 0;
                        i = 0;
                        anchor = 0;
                        continue ;
                    }
                }
                else if (parts[index].Substring(anchor, i - anchor).Contains("-") && parts[index].Substring(0,1) != "-") {
                    startIndex = i;
                    double val;
                    if (!double.TryParse(parts[index].Substring(anchor, i - anchor - 1), out val) && !VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                        //exception

                    }
                    else {
                        if (VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                            for (int j = 0; j < variables.ToArray().Length; j++) {
                                if (variables[j].Name == parts[index].Substring(anchor, i - anchor - 1)) {
                                    if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                        throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                    }
                                    val = (double)variables[j].Value;
                                }

                            }

                        }
                        anchor = i;
                        total += val;
                        int len = 0;
                        while (!parts[index].Substring(anchor, len).Contains("-") && startIndex + len < parts[index].Length) {
                            len++;
                        }
                        int anchor2 = i - 1 + len;                        i = len;
                        int len2 = anchor2 - anchor;
                        string oPart;
                        bool end;
                        if (startIndex + len == parts[index].Length) {
                            oPart = parts[index].Substring(startIndex);
                            end = true;
                        }
                        else {
                            oPart = parts[index].Substring(anchor, len2);
                            end = false;
                        }

                        double val2;
                        if (!double.TryParse(oPart, out val2) && !VarExists(oPart)) {

                            //exception
                        }
                        else {
                            if (VarExists(oPart)) {

                                for (int j = 0; j < variables.ToArray().Length; j++) {
                                    if (variables[j].Name == oPart) {
                                        if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                            throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                        }
                                        val2 = (double)variables[j].Value;
                                    }

                                }

                            }
                            total -= val2;
                            if (!end) parts[index] = total + parts[index].Substring(anchor2);
                            else parts[index] = "" + total;
                        }

                        total = 0;
                        i = 0;
                        anchor = 0;
                        continue;
                    }
                }
                else if (parts[index].Substring(anchor, i - anchor).Contains("*")) {
                    startIndex = i;
                    double val;
                    if (!double.TryParse(parts[index].Substring(anchor, i - anchor - 1), out val) && !VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                        //exception

                    }
                    else {
                        if (VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                            for (int j = 0; j < variables.ToArray().Length; j++) {
                                if (variables[j].Name == parts[index].Substring(anchor, i - anchor - 1)) {
                                    if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                        throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                    }
                                    val = (double)variables[j].Value;
                                }

                            }

                        }
                        anchor = i;
                        total += val;
                        int len = 0;

                        while (!parts[index].Substring(anchor, len).Contains("*") && startIndex + len < parts[index].Length) {

                            len++;
                        }
                        int anchor2 = i - 1 + len;
                        i = len;
                        int len2 = anchor2 - anchor;
                        string oPart;
                        bool end;
                        if (startIndex + len == parts[index].Length) {
                            oPart = parts[index].Substring(startIndex);
                            end = true;
                        }
                        else {
                            oPart = parts[index].Substring(anchor, len2);
                            end = false;
                        }

                        double val2;
                        if (!double.TryParse(oPart, out val2) && !VarExists(oPart)) {

                            //exception
                        }
                        else {
                            if (VarExists(oPart)) {

                                for (int j = 0; j < variables.ToArray().Length; j++) {
                                    if (variables[j].Name == oPart) {
                                        if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                            throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                        }
                                        val2 = (double)variables[j].Value;
                                    }

                                }

                            }

                            total *= val2;

                            if (!end) parts[index] = total + parts[index].Substring(anchor2);
                            else parts[index] = "" + total;
                        }

                        total = 0;
                        i = 0;
                        anchor = 0;
                        continue;
                    }
                }

                else if (parts[index].Substring(anchor, i - anchor).Contains("/")) {
                    startIndex = i;
                    double val;
                    if (!double.TryParse(parts[index].Substring(anchor, i - anchor - 1), out val) && !VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                        //exception
                    }
                    else {
                        if (VarExists(parts[index].Substring(anchor, i - anchor - 1))) {

                            for (int j = 0; j < variables.ToArray().Length; j++) {
                                if (variables[j].Name == parts[index].Substring(anchor, i - anchor - 1)) {
                                    if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                        throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                    }
                                    val = (double)variables[j].Value;
                                }

                            }

                        }
                        anchor = i;
                        total += val;
                        int len = 0;
                        while (!parts[index].Substring(anchor, len).Contains("/") && startIndex + len < parts[index].Length) {
                            len++;
                        }
                        int anchor2 = i - 1 + len;
                        i = len;
                        int len2 = anchor2 - anchor;
                        string oPart;
                        bool end;
                        if (startIndex + len == parts[index].Length) {
                            oPart = parts[index].Substring(startIndex);
                            end = true;
                        }
                        else {
                            oPart = parts[index].Substring(anchor, len2);
                            end = false;
                        }

                        double val2;
                        if (!double.TryParse(oPart, out val2) && !VarExists(oPart)) {

                            //exception
                        }
                        else {
                            if (VarExists(oPart)) {

                                for (int j = 0; j < variables.ToArray().Length; j++) {
                                    if (variables[j].Name == oPart) {
                                        if (!(variables[j].Value is double) && !(variables[j].Value is int)) {
                                            throw new IOException("Cannot use this type of variable for adding/subtracting/multiplying/dividing");
                                        }
                                        val2 = (double)variables[j].Value;
                                    }

                                }

                            }
                            total /= val2;
                            if (!end) parts[index] = total + parts[index].Substring(anchor2);
                            else parts[index] = "" + total;
                        }

                        total = 0;
                        i = 0;
                        anchor = 0;
                        continue;
                    }
                }
               
                
            }
            //parts[index] = num;
            //parts[index] = $"{total}";
            return index;
            //this is going to be tricky
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
            internal static string fileDir = "GENERATED.cs"; //default
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

            internal static void WriteVarAssign(Variable var) {
                WriteToGenFile($"var ${var.Name} = {var.Value};");
            }
            
            internal static void WriteVarReassign(Variable var, object newValue) {
                WriteToGenFile($"{var.Name} = {newValue}");
            }

            internal static void WritePrint(string contents) {
                WriteToGenFile($"Console.WriteLine(\"{contents}\");");
            }

            public static void EndMain() {
                totalFileText += "}";
                File.WriteAllText(fileDir, totalFileText);
                Debug.WriteToDebugFile("Main function closed");
            }
        }

        //Executes a flow based on Parsing
        private static class Executor {
            internal static void PrintVar(Variable var) {
                Console.WriteLine(var.Value);
            }

            internal static void Print(string str) {
                Console.WriteLine(str);
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
                WriteToDebugFile(message);
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
                Generator.fileDir = directoryToOutput;
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
