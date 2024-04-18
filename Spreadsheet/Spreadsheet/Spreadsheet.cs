using SpreadsheetUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SS;

/// <summary>
/// Class used to create formatting for JSON serialization
/// creates a dictionary that holds a name of a cell as well as another dictionary that holds the key "StringForm" and string value
/// of the given cells value
///
/// then carries the verion of the spreadsheet
/// </summary>
public class allCells
{
    public Dictionary<string, Dictionary<string, string>>? Cells { get; set; }
    public string? Version { get; set; }

}
// Modified for PS5.
/// <summary>
/// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
/// spreadsheet consists of an infinite number of named cells.
/// 
/// A string is a cell name if and only if it consists of a letter or underscore followed by
/// zero or more letters, underscores, or digits, AND it satisfies the predicate IsValid.
/// For example, "_", "A1", and "BC89" are cell names so long as they satisfy IsValid.
/// On the other hand, "0A1", "1+1", and "" are not cell names, regardless of IsValid.
/// 
/// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
/// must be normalized with the Normalize method before it is used by or saved in 
/// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
/// the Formula "x3+a5" should be converted to "X3+A5" before use.
/// 
/// A spreadsheet contains a cell corresponding to every possible cell name.  
/// In addition to a name, each cell has a contents and a value.  The distinction is
/// important.
/// 
/// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
/// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
/// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
/// 
/// In a new spreadsheet, the contents of every cell is the empty string.
///  
/// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
/// (By analogy, the value of an Excel cell is what is displayed in that cell's position
/// in the grid.)
/// 
/// If a cell's contents is a string, its value is that string.
/// 
/// If a cell's contents is a double, its value is that double.
/// 
/// If a cell's contents is a Formula, its value is either a double or a FormulaError,
/// as reported by the Evaluate method of the Formula class.  The value of a Formula,
/// of course, can depend on the values of variables.  The value of a variable is the 
/// value of the spreadsheet cell it names (if that cell's value is a double) or 
/// is undefined (otherwise).
/// 
/// Spreadsheets are never allowed to contain a combination of Formulas that establish
/// a circular dependency.  A circular dependency exists when a cell depends on itself.
/// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
/// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
/// dependency.
/// </summary>
public class Spreadsheet : AbstractSpreadsheet
{

    private Dictionary<string, Cell> cells;
    private DependencyGraph graph;
    private Func<string, string> Normalize;
    private Func<string, bool> IsValid;
    private string version;

    public Spreadsheet() : this(s => true, s => s, "default")
    {
    }
    //empty constructor
    public Spreadsheet(Func<string, bool> IsValid, Func<string, string> Normalize, string Version) : base(Version)
    {
        graph = new DependencyGraph();
        cells = new Dictionary<string, Cell>();
        this.IsValid = IsValid;
        this.Normalize = Normalize;
        this.version = Version;
    }
    public Spreadsheet(string Path, Func<string, bool> IsValid, Func<string, string> Normalize, string Version) : base(Version)
    {
        graph = new DependencyGraph();
        cells = new Dictionary<string, Cell>();
        this.IsValid = IsValid;
        this.Normalize = Normalize;
        this.version = Version;

        bool cellIsNull = false;
        bool versionIsWrong = false;
 
        try
        {
            string jsonString = File.ReadAllText(Path);
            allCells cellFile = JsonSerializer.Deserialize<allCells>(jsonString)!;

            // check if the versions match
            if (cellFile.Version != version)
            {
                versionIsWrong = true;
                throw new SpreadsheetReadWriteException("Versions did not match");
            }

            // read through the cells and add them to the current spreadsheet
            if (cellFile.Cells is not null)
            {
                foreach (var i in cellFile.Cells)
                {
                    if (i.Value.TryGetValue("StringForm", out string? s))
                        SetContentsOfCell(i.Key, s);
                }
            }

            else
            {
                cellIsNull = true;
                throw new SpreadsheetReadWriteException("Issue reading file");
            }
            Changed = false;
        }
        catch
        {
            if(cellIsNull)
            {
                throw new SpreadsheetReadWriteException("No Cells");
            }
            else if (versionIsWrong)
            {
                throw new SpreadsheetReadWriteException("Versions did not match");
            }
            throw new SpreadsheetReadWriteException("Issue reading given file");
        }
    }


    /// <summary>
    ///  helper method that makes sure a given name is valid or not
    /// </summary>
    /// <param name="name"></param>
    private void addCell(string name, Func<string, bool> IsValid, Func<string, string> Normalize)
    {
        // check if name is allowed
        bool validName = false;
        // makes sure it isn't an empty string and that it does not start with a number
        if (name.Length > 0 && !Double.TryParse(name.Substring(0, 1), out double tempVal))
        {
            foreach (var ch in name)
            {
                if (char.IsLetter(ch) || ch.Equals('_') || char.IsDigit(ch))
                {
                    validName = true;
                }
                else
                {
                    validName = false;
                    break;
                }
            }
        }
        // if its a valid name and cells does not already contain it, it passes.
        if (validName && !cells.ContainsKey(name) && IsValid(name))
        {
            cells.Add(name, new Cell(""));
        }

    }


    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, returns the contents (as opposed to the value) of the named cell.
    /// The return value should be either a string, a double, or a Formula.
    /// 
    /// </summary>
    /// <param name="name"> the name related to the cell</param>
    /// <exception cref="InvalidNameException"> thrown if the cell name is invalid</exception>
    public override object GetCellContents(string name)
    {
        // if the name is valid in cells return its related cell value
        addCell(name, IsValid, Normalize);
        name = Normalize(name);
        if (cells.TryGetValue(name, out Cell? newCellVal))
            return newCellVal.Content;
        throw new InvalidNameException();

    }


    /// <summary>
    /// Enumerates the names of all the non-empty cells in the spreadsheet.
    /// </summary>
    public override IEnumerable<string> GetNamesOfAllNonemptyCells()
    {
        // grab all of the cell's names and add them to a list of names
        List<string> names = new List<string>();
        foreach (var entry in cells.Keys)
        {
            if (cells.TryGetValue(entry, out Cell? sizeCheck))
            {
                if (!sizeCheck.Content.Equals(""))
                    names.Add(entry);
            }
        }
        return names;
    }

    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, the contents of the named cell becomes number.  The method returns a
    /// list consisting of name plus the names of all other cells whose value depends, 
    /// directly or indirectly, on the named cell.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    /// <param name="name"> the cell name that is being changed </param>
    /// <param name="number"> the new number that is being added</param>
    protected override IList<string> SetCellContents(string name, double number)
    {
        //change the value of a cell to the given double and return a list of the items that are dependent on it
        addCell(name, IsValid, Normalize);
        if (cells.TryGetValue(name, out Cell? newCellVal))
        {
            newCellVal.Content = number;
            newCellVal.Val = number;
            IList<string> returnList = new List<string>();
            foreach (var entry in GetCellsToRecalculate(name))
                returnList.Add(entry);
            // cell no longer depends on anything so replace its dependents
            graph.ReplaceDependees(name, new HashSet<string>());
            return returnList;
        }
        throw new InvalidNameException();

    }

    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, the contents of the named cell becomes text.  The method returns a
    /// list consisting of name plus the names of all other cells whose value depends, 
    /// directly or indirectly, on the named cell.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    /// <param name="name"> the cell name that is being changed </param>
    /// <param name="text"> the new text that is being added</param>
    protected override IList<string> SetCellContents(string name, string text)
    {
        //change the value of a cell to the given string and return a list of the items that are dependent on it
        addCell(name, IsValid, Normalize);

        if (cells.TryGetValue(name, out Cell? newCellVal))
        {
            newCellVal.Content = text;
            newCellVal.Val = text;
            IList<string> returnList = new List<string>();
            foreach (var entry in GetCellsToRecalculate(name))
                returnList.Add(entry);
            // cell no longer depends on anything so replace its dependents
            graph.ReplaceDependees(name, new HashSet<string>());
            return returnList;
        }
        throw new InvalidNameException();
    }

    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
    /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
    /// 
    /// Otherwise, the contents of the named cell becomes formula.  The method returns a
    /// list consisting of name plus the names of all other cells whose value depends,
    /// directly or indirectly, on the named cell.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    /// <param name="name"> the cell name that is being changed </param>
    /// <param name="formula"> the new formula that is being added</param>
    protected override IList<string> SetCellContents(string name, Formula formula)
    {
        HashSet<string> temp = (HashSet<string>)graph.GetDependees(name);
        //change the value of a cell to the given Formula and return a list of the items that are dependent on it
        addCell(name, IsValid, Normalize);
        if (cells.TryGetValue(name, out Cell? newCellVal))
        {

            graph.ReplaceDependees(name, new HashSet<string>());
            // save formula incase of error
            object tempFormula = newCellVal.Content;
            newCellVal.Content = formula;

            // add dependencies depending on formula given
            foreach (var variable in formula.GetVariables())
            {
                graph.AddDependency(variable, name);
            }
            IList<string> returnList = new List<string>();
            try
            {
                foreach (var entry in GetCellsToRecalculate(name))
                    returnList.Add(entry);
            }
            catch
            {
                // if theres a circular error, return the cell back to its original state
                newCellVal.Content = tempFormula;
                graph.ReplaceDependees(name, temp);
                throw new CircularException();
            }
            return returnList;
        }
        throw new InvalidNameException();
    }

    // ADDED FOR PS5
    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, if content parses as a double, the contents of the named
    /// cell becomes that double.
    /// 
    /// Otherwise, if content begins with the character '=', an attempt is made
    /// to parse the remainder of content into a Formula f using the Formula
    /// constructor.  There are then three possibilities:
    /// 
    ///   (1) If the remainder of content cannot be parsed into a Formula, a 
    ///       SpreadsheetUtilities.FormulaFormatException is thrown.
    ///       
    ///   (2) Otherwise, if changing the contents of the named cell to be f
    ///       would cause a circular dependency, a CircularException is thrown,
    ///       and no change is made to the spreadsheet.
    ///       
    ///   (3) Otherwise, the contents of the named cell becomes f.
    /// 
    /// Otherwise, the contents of the named cell becomes content.
    /// 
    /// If an exception is not thrown, the method returns a list consisting of
    /// name plus the names of all other cells whose value depends, directly
    /// or indirectly, on the named cell. The order of the list should be any
    /// order such that if cells are re-evaluated in that order, their dependencies 
    /// are satisfied by the time they are evaluated.
    /// 
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    /// list {A1, B1, C1} is returned.
    /// </summary>
    /// <param name="content"> the content of the new cell</param>
    /// <param name="name"> Name of the cell thats being set</param>
    public override IList<string> SetContentsOfCell(string name, string content)
    {
        IList<string> temp = new List<string>();
        name = Normalize(name);
        // if its a double, use setCellContents's double verison
        if (double.TryParse(content, out double outVal))
        {
            Changed = true;
            temp = SetCellContents(name, outVal);
        }

        //if its a double, use setCellContents's formula verison and add the dependents values to a list
        else if (content.Length > 0 && content.Substring(0, 1) == "=")
        {
            Formula f = new Formula(content[1..], Normalize, IsValid);
            temp = SetCellContents(name, f);
        }

        // if its a string use setCellContent's string version
        else
        {
            Changed = true;
            temp = SetCellContents(name, content);
        }

        //use the list thats saved if it's a formula to find and set the values of each formula
        foreach (string i in temp)
        {
            if (cells[i].Content is Formula f)
            {
                cells[i].Val = f.Evaluate(Lookup);
            }
        }
        Changed = true;
        return temp;
    }

    // ADDED FOR PS5
    /// <summary>
    /// If name is invalid, throws an InvalidNameException.
    /// 
    /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
    /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
    /// </summary>
    /// <param name="name"> Cell thats value is being accessed </param>
    public override object GetCellValue(string name)
    {
        // make sure name is valid
        addCell(name, IsValid, Normalize);
        name = Normalize(name);
        if (cells.TryGetValue(name, out Cell? newCellVal))
        {
            return newCellVal.Val;
        }
        throw new InvalidNameException();
    }
    /// <summary>
    /// Simple lookup method
    /// </summary>
    /// <param name="name"> name of cell that is being loo</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private double Lookup(string name)
    {
        if (GetCellValue(name) is double d)
        {
            return d;
        }
        else
        {
            throw new ArgumentException("Invalid lookup");
        }
    }

    /// <summary>
    /// Returns an enumeration, without duplicates, of the names of all cells whose
    /// values depend directly on the value of the named cell.  In other words, returns
    /// an enumeration, without duplicates, of the names of all cells that contain
    /// formulas containing name.
    /// 
    /// For example, suppose that
    /// A1 contains 3
    /// B1 contains the formula A1 * A1
    /// C1 contains the formula B1 + A1
    /// D1 contains the formula B1 - C1
    /// The direct dependents of A1 are B1 and C1
    /// </summary>
    /// <param name="name"> the cell you are trying to get what its dependent on</param>
    protected override IEnumerable<string> GetDirectDependents(string name)
    {
        return graph.GetDependents(name);
    }

    // ADDED FOR PS5
    /// <summary>
    /// Writes the contents of this spreadsheet to the named file using a JSON format.
    /// The JSON object should have the following fields:
    /// "Version" - the version of the spreadsheet software (a string)
    /// "Cells" - a data structure containing 0 or more cell entries
    ///           Each cell entry has a field (or key) named after the cell itself 
    ///           The value of that field is another object representing the cell's contents
    ///               The contents object has a single field called "StringForm",
    ///               representing the string form of the cell's contents
    ///               - If the contents is a string, the value of StringForm is that string
    ///               - If the contents is a double d, the value of StringForm is d.ToString()
    ///               - If the contents is a Formula f, the value of StringForm is "=" + f.ToString()
    /// 
    /// For example, if this spreadsheet has a version of "default" 
    /// and contains a cell "A1" with contents being the double 5.0 
    /// and a cell "B3" with contents being the Formula("A1+2"), 
    /// a JSON string produced by this method would be:
    /// 
    /// {
    ///   "Cells": {
    ///     "A1": {
    ///       "StringForm": "5"
    ///     },
    ///     "B3": {
    ///       "StringForm": "=A1+2"
    ///     }
    ///   },
    ///   "Version": "default"
    /// }
    /// 
    /// If there are any problems opening, writing, or closing the file, the method should throw a
    /// SpreadsheetReadWriteException with an explanatory message.
    /// </summary>
    public override void Save(string filename)
    {
        Dictionary<string, Dictionary<string, string>> tempCells = new Dictionary<string, Dictionary<string, string>>();
        string version = Version;
        try
        {
            foreach (string i in GetNamesOfAllNonemptyCells())
            {
                // add values of cells to a new dictionary which will represent the cells in a serialized version of the spreadsheet
                Dictionary<string, string> StringForm = new Dictionary<string, string>();
                if (cells[i].Content is Formula f)
                {
                    StringForm.Add("StringForm", "=" + f.ToString());
                }
                else
                {
                    StringForm.Add("StringForm", "" + cells[i].Content);
                }
                tempCells.Add(i, StringForm);
            }
            var cellHolder = new allCells
            {
                Cells = tempCells,
                Version = version
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(cellHolder, options);
            Changed = false;
            File.WriteAllText(filename, jsonString);
        }
        // if any errors happen while serializing throw this error
        catch
        {
            throw new SpreadsheetReadWriteException("There was a problem writing the file");
        }
    }
}

