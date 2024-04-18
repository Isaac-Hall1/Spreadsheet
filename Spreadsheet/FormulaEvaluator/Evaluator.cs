//
// Author: Isaac Hall
//
// Date: 9/4/2023
// 
// This class solves infix problems as well as looks for possible variables that have a numerical value.
//
using System;
using System.Text.RegularExpressions;
namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);
        /// <summary>
        ///  Parses through given formula and returns a result once evaluated
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns> integer </returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {

            exp = String.Concat(exp.Where(c => !Char.IsWhiteSpace(c)));
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            Stack<int> integerStack = new Stack<int>();
            Stack<string> operatorStack = new Stack<string>();

            for (int i = 0; i < substrings.Length; i++)
            {
                //adds "+" to opp Stack, making sure to add two int vals if
                // a + is at the top of the opp Stack
                if (substrings[i].Equals("+"))
                {
                    addSubtract(substrings[i], integerStack, operatorStack);
                }
                //adds "-" to opp Stack, making sure to add two int vals if
                // a - is at the top of the opp Stack
                else if (substrings[i].Equals("-"))
                {
                    addSubtract(substrings[i], integerStack, operatorStack);
                }
                //adds "*" to opp Stack
                else if (substrings[i].Equals("*"))
                {
                    operatorStack.Push("*");
                }
                //adds "/" to opp stack
                else if (substrings[i].Equals("/"))
                {
                    operatorStack.Push("/");
                }
                // adds "(" to opp stack
                else if (substrings[i].Equals("("))
                {
                    operatorStack.Push("(");
                }
                // calls func for adding ")"
                else if (substrings[i].Equals(")"))
                {
                    rightParenthesis(integerStack, operatorStack);
                }
                else
                {
                    // there was an edge case where after a ")" the regex statement added a whitespace
                    // this if statement protects from unecissary errors
                    if(substrings[i].Length == 0)
                    {

                    }
                    else
                    {
                        int value = checkVal(substrings[i], variableEvaluator);
                        evaluateInteger(value, integerStack,operatorStack);
                    }
                }
            }
            // If the operator stack is not empty after the sequence has finished, add or subtract the final values
            if(operatorStack.Count > 0 && (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
            {
                addSubtract(operatorStack.Peek(), integerStack, operatorStack);
                // If there is more than 1 number left over throw an error
                if(integerStack.Count > 1)
                {
                    throw new ArgumentException();
                }
            }
            // if there is an operator left in the operator stack that is not a + or - throw an error.
            else if (operatorStack.Count > 0 && (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
            {
                throw new ArgumentException();
            }
            // if there is no integer in the integer stack throw an error
            else if (integerStack.Count == 0)
            {
                throw new ArgumentException();
            }
            return integerStack.Pop();
        }
        /// <summary>
        /// does necissary functions for adding or subtracting values
        /// </summary>
        /// <param name="s"> operator that is being added </param>
        /// <exception cref="ArgumentException">< if the integerStack is empty of only 1 value, an error is thrown /exception>
        private static void addSubtract(string s, Stack<int> integerStack, Stack<string> operatorStack)
        {
            // if there are no operators yet, adds the given opperator to the opperator stack
            if (!operatorStack.Contains("+") && !operatorStack.Contains("-"))
            {
                operatorStack.Push(s);
                return;
            }
            // integerStack doesn't have 2 values in it
            else if(integerStack.Count <= 1)
            {
                throw new ArgumentException();
            }
            // subtracts two values of stack 
            else if (operatorStack.Peek().Equals("-"))
            {
                int x = integerStack.Pop();
                int y = integerStack.Pop();
                operatorStack.Pop();

                integerStack.Push(y - x);
            }
            else if (operatorStack.Peek().Equals("+"))
            {
                int x = integerStack.Pop();
                int y = integerStack.Pop();
                operatorStack.Pop();

                integerStack.Push(y + x);
            }
            operatorStack.Push(s);
        }
        /// <summary>
        /// Evaluates when an integer is added into the integerStack
        /// </summary>
        /// <param name="val"> the integer being added </param>
        /// <exception cref="ArgumentException"> if the integer is 0 it cannot be a dividend </exception>
        private static void evaluateInteger(int val, Stack<int> integerStack, Stack<string> operatorStack)
        {
            // if the operator on top of the stack isn't one above it pushes the current value to the int stack
            if (operatorStack.Count == 0)
            {
                integerStack.Push(val);
                return;
            }
            //multiplies given val by next val in intStack, then gets rid of operator
            if (operatorStack.Peek().Equals("*"))
            {
                int x = integerStack.Pop();
                operatorStack.Pop();
                integerStack.Push(x * val);
            }
            else if (operatorStack.Peek().Equals("/"))
            {
                int x = integerStack.Pop();
                operatorStack.Pop();
                if(val == 0)
                {
                    throw new ArgumentException();
                }
                integerStack.Push(x / val);
            }
            else
            {
                integerStack.Push(val);
            }
        }
        /// <summary>
        ///  Computes contents within parenthesis
        /// </summary>
        /// <param name="integerStack"></param>
        /// <param name="operatorStack"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void rightParenthesis(Stack<int> integerStack, Stack<string> operatorStack)
        {
            // if the integer stack or opperator stack dont have enough values within it an error is thrown
            if(integerStack.Count < 1 || operatorStack.Count <= 0)
            {
                throw new ArgumentException();
            }
            //uses add methods as well as pops "("
            if (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-"))
            {
                addSubtract(operatorStack.Peek(), integerStack, operatorStack);
                // if the left parenthesis isnt where it should be and error is thrown.
                operatorStack.Pop();
                if (operatorStack.Count == 0 ||!operatorStack.Peek().Equals("("))
                    throw new ArgumentException();
                operatorStack.Pop();
                // check for to see if any multiplying or dividing needs to be done
                if (operatorStack.Count > 0 && (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
                {
                    evaluateInteger(integerStack.Pop(), integerStack,operatorStack);
                }
            }
            // throw an error if ( was not removed and there are other opperators in the operator stack
            else if(operatorStack.Count > 0 && !operatorStack.Peek().Equals("("))
            {
                throw new ArgumentException();
            }
            // get rid of the ( if there are no other opperations to be done
            else
            {
                operatorStack.Pop();
            }
        }
        /// <summary>
        /// Helper method for getting the correct value for when an integer, or table #, is present
        /// within the infix string
        /// </summary>
        /// <param name="val"> the integer or variable that is going to be transformed into a value from a string </param>
        /// <param name="varLookup"> allows for delegate method to be used </param>
        /// <returns> the integer from the string given or variable given </returns>
        /// <exception cref="ArgumentException"> if an invalid token is given an error is thrown </exception>
        private static int checkVal(string val, Lookup varLookup)
        {
            // checks if the current string is a digit and if it is it returns it
            if (Char.IsDigit(val, 0))
            {
                int x;
                int.TryParse(val, out x);
                return x;
            }
            // checks if the current string is a variable that needs to be looked up
            else if (Char.IsDigit(val, val.Length - 1))
            {
                int v = varLookup(val);
                if (v > 0 || v <= 0)
                {
                    return v;
                }
                else
                {
                    throw new ArgumentException("delegate was not able to get a number");
                }
            }
            // if neither of these are the case the given string is an invalid token
            else
            {
                throw new ArgumentException("Not a valid token");
            }
        }
    }

}

