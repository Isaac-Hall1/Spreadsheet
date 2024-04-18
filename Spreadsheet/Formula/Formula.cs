// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

// Author: Isaac Hall
// Date: 09/12/2023

using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private IEnumerable<string> formulaList;
    private bool formError;

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        IEnumerable<string> substrings = GetTokens(formula);
        if (substrings.Count() == 0)
        {
            throw new FormulaFormatException("empty formula");
        }
        // uses helper method to confirm whether or not the tokens are valid
        goodTokens(substrings, normalize, isValid);
        // normalizes tokens
        formulaList = goodTokens(substrings, normalize, isValid);

    }
    /// <summary>
    /// checks the tokens within the formula list and will throw if an invalid token is found
    /// </summary>
    /// <param name="s"> the formula list of tokens</param>
    /// <param name="normalize"> a given function </param>
    /// <param name="isValid"> another given function that returns a bool </param>
    /// <exception cref="FormulaFormatException"></exception>
    private List<string> goodTokens(IEnumerable<string> s, Func<string, string> normalize, Func<string, bool> isValid)
    {
        double dblVal;
        int leftParCount = 0;
        int rightParCount = 0;
        string lastToken = "";

        List<string> tempList = new List<string>();
        // if the first entry or the last entry are not what they should be throws an exception
        if (!s.ElementAt(0).Equals("(") && !Double.TryParse(s.ElementAt(0), out dblVal) && !isVar(normalize(s.ElementAt(0)), normalize, isValid))
        {
            throw new FormulaFormatException("did not start the formula with a number, left parenthesis, or a variable");
        }
        if (!s.ElementAt(s.Count() - 1).Equals(")") && !Double.TryParse(s.ElementAt(s.Count() - 1), out dblVal) && !isVar(normalize(s.ElementAt(s.Count() - 1)), normalize,isValid))
        {
            throw new FormulaFormatException("did not end the formula with a number, right parenthesis, or a variable");
        }
        // goes through the tokens
        foreach (var entry in s)
        {
            if ((isOpperator(lastToken) || lastToken.Equals("(")) &&
                !(Double.TryParse(entry, out dblVal) || isVar(entry, normalize,isValid) || entry.Equals("(")))
            {
                throw new FormulaFormatException("the entry following a left parenthesis or an opperator was not a number, variable or left parenthesis");
            }
            if ((Double.TryParse(lastToken, out dblVal) ||  lastToken.Equals(")") || isVar(lastToken, normalize, isValid)) &&
                !(isOpperator(entry) || entry.Equals(")")))
            {
                throw new FormulaFormatException("the entry following a variable, value, or right parenthesis was not an opperator or right parenthesis");
            }
            if (entry.Equals("(") || entry.Equals(")") || isOpperator(entry))
            {
                if (entry.Equals("("))
                {
                    leftParCount++;
                }
                else if (entry.Equals(")"))
                {
                    rightParCount++;
                }
                if(rightParCount > leftParCount)
                {
                    throw new FormulaFormatException("More right parenthesis than left parenthesis");
                }
                lastToken = entry;
                tempList.Add(entry);
                continue;
            }
            else if (Double.TryParse(entry,out dblVal))
            {
                lastToken = entry;
                tempList.Add(entry);
                continue;
            }
            else if(!isVar(entry, normalize, isValid))
            {
                throw new FormulaFormatException(entry + " was not not a valid variable");
            }
            tempList.Add(normalize(entry));
            lastToken = entry;
        }
        if(leftParCount > rightParCount)
        {
            throw new FormulaFormatException("more left parenthesis than right parenthesis");
        }
        return tempList;
    }
    /// <summary>
    /// takes in a string and determines whether or not its an opperator
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    private bool isOpperator(string entry)
    {
        if (entry.Equals("+") || entry.Equals("/") || entry.Equals("*") || entry.Equals("-"))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    ///  Determines if a string in tokens is a variable or not
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="normalize"></param>
    /// <param name="isValid"></param>
    /// <returns></returns>
    private bool isVar(string entry, Func<string, string> normalize, Func<string, bool> isValid)
    {
        bool boolVal = false;
        // if a token is a valid normalized string, tests to see if it follows the parameters of a variable
        if (isValid(normalize(entry)))
        {
            if (entry.Length > 0 && !Double.TryParse(entry.Substring(0, 1), out double tempVal))
            {
                foreach (var ch in entry)
                {
                    if (char.IsLetter(ch) || ch.Equals('_') || char.IsDigit(ch))
                    {
                        
                        boolVal = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return boolVal;
    }



    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<double> doubleStack = new Stack<double>();
        Stack<string> operatorStack = new Stack<string>();
        formError = false;
        // evaluates infix expressions
        foreach(var entry in formulaList)
        {
            if (formError == true)
                return new FormulaError("tried to divide by 0");
            if (entry.Equals("+") || entry.Equals("-"))
            {
                addSubtract(entry, doubleStack,operatorStack);
            }
            else if (entry.Equals("*") || entry.Equals("/"))
            {
                operatorStack.Push(entry);
            }
            else if (entry.Equals("("))
            {
                operatorStack.Push(entry);
            }
            else if (entry.Equals(")"))
            {
                rightParenthesis(doubleStack, operatorStack);
            }
            else
            {
                double val = checkVal(entry, lookup);
                if (formError == true)
                    return new FormulaError("lookup method threw an error");
                evaluateDouble(val, doubleStack, operatorStack);
            }
        }
        // if you tried to divide by 0 throws this error
        if (formError == true)
            return new FormulaError("tried to divide by 0");
        if (operatorStack.Count > 0)
        {
            addSubtract(operatorStack.Peek(),doubleStack, operatorStack);
        }

        return doubleStack.Pop();
    }
    /// <summary>
    /// adds the + or - opperators to the operatorStack and does necissary compuations based on the operators already within the operator stack
    /// </summary>
    /// <param name="s"></param>
    /// <param name="doubleStack"></param>
    /// <param name="operatorStack"></param>
    private void addSubtract(string s,Stack<double> doubleStack, Stack<string> operatorStack)
    {
        {
            // if there are no operators yet, adds the given opperator to the opperator stack
            if (!operatorStack.Contains("+") && !operatorStack.Contains("-"))
            {
                operatorStack.Push(s);
                return;
            }
            // subtracts two values of stack 
            else if (operatorStack.Peek().Equals("-"))
            {
                double x = doubleStack.Pop();
                double y = doubleStack.Pop();
                operatorStack.Pop();

                doubleStack.Push(y - x);
            }
            else if (operatorStack.Peek().Equals("+"))
            {
                double x = doubleStack.Pop();
                double y = doubleStack.Pop();
                operatorStack.Pop();

                doubleStack.Push(y + x);
            }
            // if the operatorStack's most recent value was not a + or - and it wasn't empty, add the operator.
            operatorStack.Push(s);
        }
    }
    /// <summary>
    /// evaluates when a number is passes into the double stack
    /// </summary>
    /// <param name="val"></param>
    /// <param name="doubleStack"></param>
    /// <param name="operatorStack"></param>
    /// <exception cref="ArgumentException"></exception>
    private void evaluateDouble(double val,Stack<double> doubleStack, Stack<string> operatorStack)
    {
        if (operatorStack.Count == 0)
        {
            doubleStack.Push(val);
            return;
        }
        //multiplies given val by next val in dblStack, then gets rid of operator
        else if (operatorStack.Peek().Equals("*"))
        {
            double x = doubleStack.Pop();
            operatorStack.Pop();
            doubleStack.Push(x * val);
        }
        //divides next val by given val in dblStack, then gets rid of operator
        else if (operatorStack.Peek().Equals("/"))
        {
            double x = doubleStack.Pop();
            operatorStack.Pop();
            if (val == 0)
            {
                formError = true;
                return;
            }
            doubleStack.Push(x / val);
        }
        // adds val if the operator stack did not have a * or /
        else
        {
            doubleStack.Push(val);
        }
    }
    /// <summary>
    /// Does necessary functions when the current entry value is a right parenthesis
    /// </summary>
    /// <param name="doubleStack"></param>
    /// <param name="operatorStack"></param>
    /// <exception cref="ArgumentException"></exception>
    private void rightParenthesis(Stack<double> doubleStack, Stack<string> operatorStack)
    {
        //uses add methods as well as pops "("
        if (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-"))
        {
            addSubtract(operatorStack.Peek(), doubleStack, operatorStack);
            // if the left parenthesis isnt where it should be and error is thrown.
            operatorStack.Pop();
            operatorStack.Pop();
            // check for to see if any multiplying or dividing needs to be done
            if (operatorStack.Count > 0 && (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
            {
                evaluateDouble(doubleStack.Pop(), doubleStack, operatorStack);
            }
        }
        // get rid of the ( if there are no other opperations to be done
        else
        {
            operatorStack.Pop();
            if (operatorStack.Count > 0 && (operatorStack.Peek().Equals("*") || operatorStack.Peek().Equals("/")))
            {
                evaluateDouble(doubleStack.Pop(), doubleStack, operatorStack);
            }
        }
    }
    /// <summary>
    /// if the entry is a normal value just return it, if its a variable, make sure the lookup method works then return the value
    /// </summary>
    /// <returns></returns>
    private double checkVal(string entry, Func<string, double> lookup)
    {
        // if its a double, return the value
        if(double.TryParse(entry, out double dblVal))
        {
            return dblVal;
        }
        // if its a variable, try to look it up and throw an exception if the lookup fails
        try
        {
            dblVal = lookup(entry);
        }
        catch
        {
            formError = true;
            return 0;
        }
        return dblVal;
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        List<string> variableList = new List<string>();
        foreach(var entry in formulaList)
        {
            if (Regex.IsMatch(entry, "[a-zA-Z_][0-9]*"))
            {
                if(!variableList.Contains(entry))
                    variableList.Add(entry);
            }
        }
        return variableList;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        string returnString = "";
        foreach (var entry in formulaList)
        {
            if(double.TryParse(entry, out double temp))
            {
                if (temp == (int)temp)
                {
                    temp = (int)temp;
                    returnString += temp.ToString();
                    continue;
                }
            }
            returnString += entry;
        }
        return returnString;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != this.GetType())
            return false;
        Formula? objectFormula = obj as Formula;
        if (this.formulaList.Count() != objectFormula?.formulaList.Count())
        {
            return false;
        }

        // checks through both formulas as strings to determine whether they're equal or not
        for (int i = 0; i < this.formulaList.Count(); i++)
        {
            if (this.formulaList.ElementAt(i) == objectFormula.formulaList.ElementAt(i))
            {
                continue;
            }

            else if (double.TryParse(this.formulaList.ElementAt(i), out double thisDoubleVal) && (double.TryParse(objectFormula.formulaList.ElementAt(i), out double objectDoubleVal)))
            {
                if (thisDoubleVal == objectDoubleVal)
                    continue;
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        if (f1.Equals(f2))
            return true;
        return false;
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        if (!f1.Equals(f2))
            return true;
        return false;
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        int hashCode = 7;
        foreach(var val in this.ToString())
        {
            hashCode = hashCode * 7 + (int)val;
        }
        return hashCode;
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

