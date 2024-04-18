namespace SS;

using System.Text.Json;
using SpreadsheetUtilities;

[TestClass]
public class SpreadsheetTests
{
    [TestMethod]
    public void emptySpreadSheet()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        Assert.AreEqual(new List<string>().Count, s.GetNamesOfAllNonemptyCells().Count());
        Assert.AreEqual("", s.GetCellContents("a2"));
    }

    [TestMethod]
    public void simpleSetReturnTest()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "=A1 * 3");
        s.SetContentsOfCell("C1", "=B1*A1");

        foreach(var i in s.SetContentsOfCell("A1", "3"))
        {
            Console.WriteLine(i);
        }

        
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void badNames()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.GetCellContents("1_a");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void badNames2()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("1a", "bruh");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void badNames3()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("a&1", "20.0");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void badNames4()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("a2_____@", "=a2 + 2");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void badNames5()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.GetCellContents("______%");
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void emptyFail()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.GetCellContents("");
    }
    [TestMethod]
    public void simpleSetCellContents()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        List<string> tempList = new List<string>();

        tempList.Add("a2");

        Assert.AreEqual(tempList[0],s.SetContentsOfCell("a2", "30")[0]);
        Assert.AreEqual(30.0, s.GetCellContents("a2"));
        

        s.SetContentsOfCell("a3", "bruh");
        Assert.AreEqual("bruh", s.GetCellContents("a3"));

        s.SetContentsOfCell("a3", "=a2 + 3");
        Assert.IsTrue(new Formula("a2 + 3").Equals(s.GetCellContents("a3")));
    }
    [TestMethod]
    public void simpleGetNamesOfNonEmptyCells()
    {
        AbstractSpreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("a2", "30");
        s.SetContentsOfCell("a3", "bruh");
        s.SetContentsOfCell("a4", "=a2 + 3");

        List<string> testList = new List<string>();
        testList.Add("a2");
        testList.Add("a3");
        testList.Add("a4");

        for(int i = 0; i < testList.Count; i++)
        {
            Assert.IsTrue(testList.ElementAt(i) == s.GetNamesOfAllNonemptyCells().ElementAt(i));
        }

    }
    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void circularException()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("a2", "=B1 + C1");
        s.SetContentsOfCell("B1", "=a2");
    }
    // b1 - (a2) c1 - (a2) a2 - (b1)
    // a2 - (b1, c1) b1 - (a2)

    [TestMethod]
    public void NoExceptionTest()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("a2", "=B1 + C1");
        s.SetContentsOfCell("a2", "30");
        s.SetContentsOfCell("B1", "=a2");
    }
    [TestMethod]
    public void complicatedSetCellContents()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        List<string> tempList = new List<string>();
        List<string> returnedList = new List<string>();

        s.SetContentsOfCell("a2", "30");
        Assert.AreEqual(s.GetCellContents("a2"), 30.0);
        s.SetContentsOfCell("a2", "40");
        Assert.AreEqual(s.GetCellContents("a2"), 40.0);


        s.SetContentsOfCell("a2", "=a1 + b2 + c2");
        s.SetContentsOfCell("a2", "=d1 + h2");
        s.SetContentsOfCell("b2", "=a2 + c2");
        s.SetContentsOfCell("a2", "bruhmoment");
        s.SetContentsOfCell("d1", "=a2");
        s.SetContentsOfCell("c2", "=l2 + g4");
        s.SetContentsOfCell("b2", "bruh");
        s.SetContentsOfCell("a1", "=b1 + 1");
        s.SetContentsOfCell("c2", "=a1 + 3");
        returnedList = (List<string>)s.SetContentsOfCell("b1", "beruh");

        tempList.Add("b1");
        tempList.Add("a1");
        tempList.Add("c2");

        for(int i = 0; i < returnedList.Count; i++)
        {
            Assert.IsTrue(returnedList[i].Equals(tempList[i]));
            Console.WriteLine(returnedList[i]);
        }

    }
    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void complicatedCircularException()
    {
        AbstractSpreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("b2", "=a2 + c2");
        s.SetContentsOfCell("a2", "bruhmoment");
        s.SetContentsOfCell("d1", "=a2");
        s.SetContentsOfCell("c2", "=l2 + g4");
        s.SetContentsOfCell("l2", "=b2");
    }

    [TestMethod]
    [ExpectedException(typeof(CircularException))]
    public void complicatedCircularException2()
    {
        AbstractSpreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("b2", "=a2 + c2");
        s.SetContentsOfCell("a2", "bruhmoment");
        s.SetContentsOfCell("d1", "=a2");
        s.SetContentsOfCell("c2", "=l2 + g4");
        s.SetContentsOfCell("l2", "=k2 + n3 + m2+ l3");
        s.SetContentsOfCell("l3", "=k3 + n4 + m1 + l4");
        s.SetContentsOfCell("l4", "=g4 + l3");
    }


    [TestMethod(), Timeout(2000)]
    [TestCategory("17b")]
    [ExpectedException(typeof(CircularException))]
    public void TestUndoCellsCircular()
    {
        Spreadsheet s = new Spreadsheet();
        try
        {
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }
        catch (CircularException e)
        {
            Assert.AreEqual("", s.GetCellContents("A2"));
            Assert.IsTrue(new HashSet<string> { "A1" }.SetEquals(s.GetNamesOfAllNonemptyCells()));
            throw e;
        }
    }



    //------------------- end of PS4 tests --------------------------

    [TestMethod]
    public void SaveTest()
    {
        AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToLower(), "1.1");
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("B1", "bruh");
        s.SetContentsOfCell("C1", "= 1 + A1");

        s.Save("Dude.txt");

        AbstractSpreadsheet s2 = new Spreadsheet("Dude.txt", s => true, s => s, "1.1");

        foreach(var i in s.GetNamesOfAllNonemptyCells())
        {
            Assert.AreEqual(s.GetCellContents(i), s2.GetCellContents(i));
        }
    }
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void nonExistentFile()
    {

        AbstractSpreadsheet s2 = new Spreadsheet("Non-existentFile.txt", s => true, s => s, "1.1");

    }
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void badSaveTest()
    {
        AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToLower(), "1.1");
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("B1", "bruh");
        s.SetContentsOfCell("C1", "= 1 + A1");

        s.Save("/some/nonsense/path.txt");
    }
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void differentVersions()
    {

        AbstractSpreadsheet s2 = new Spreadsheet("Bruh.txt", s => true, s => s, "1.0");

    }
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void BadFile()
    {

        AbstractSpreadsheet s2 = new Spreadsheet("BadFile.txt", s => true, s => s, "1.1");

    }
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void BadFile2()
    {

        AbstractSpreadsheet s2 = new Spreadsheet("BadFile2.txt", s => true, s => s, "1.0");

    }
    [TestMethod]
    public void formulaError()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("C1", "= A1 / 3");
        Assert.AreEqual(new FormulaError("lookup method threw an error"), s.GetCellValue("C1"));

        s.SetContentsOfCell("D1", "=10 + 5");
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("C1", "= A1 / 0");
        Assert.AreEqual(new FormulaError("tried to divide by 0"), s.GetCellValue("C1"));

        s.SetContentsOfCell("A1", "=5 + 10");
        s.SetContentsOfCell("B1", "Bruh");
        s.SetContentsOfCell("D1", "=A1 + B1");
        s.SetContentsOfCell("C1", "=A1 / 3");
        Assert.AreEqual(new FormulaError("lookup method threw an error"), s.GetCellValue("D1"));
        Assert.AreEqual(5.0, s.GetCellValue("C1"));
    }
    [TestMethod]
    public void getCellValue()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "10.0");
        s.SetContentsOfCell("C1", "=A1 / 5");

        s.SetContentsOfCell("B1", "=C1 + 2");
        Assert.AreEqual(4.0,s.GetCellValue("B1"));

        s.SetContentsOfCell("A1", "20.0");
        Assert.AreEqual(6.0, s.GetCellValue("B1"));

        s.SetContentsOfCell("D2", "5");
        s.SetContentsOfCell("D3", "6");

        s.SetContentsOfCell("D1", "=D2 + D3");
        s.SetContentsOfCell("D4", "=D1 + C1");

        Assert.AreEqual(15.0, s.GetCellValue("D4"));
    }
    [TestMethod]
    public void setContentOfCellTest()
    {
        AbstractSpreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("A1", "10.0");
        s.SetContentsOfCell("C1", "=A1 / 5");
        s.SetContentsOfCell("B1", "=C1 + 2");
        Assert.AreEqual(4.0, s.GetCellValue("B1"));

        s.SetContentsOfCell("C1", "25");
        Assert.AreEqual(27.0, s.GetCellValue("B1"));
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidNameException))]
    public void getCellValueWithInvalidName()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "10.0");
        s.SetContentsOfCell("C1", "=A1 / 5");

        s.GetCellValue("1a");

    }
    [TestMethod, Timeout(2000)]
    public void stressTest()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        Random rand = new Random();

        for(int i = 0; i < 10000; i++)
        {
            s.SetContentsOfCell("A" + (i + 1).ToString(), "5");
        }

        for(int i = 0; i < 2000; i++)
        {
            s.SetContentsOfCell("A" + rand.Next(i, 2000).ToString(), "=A" + rand.Next(2001, 10000).ToString() + "+ 5" + "+ A" + rand.Next(2001, 10000).ToString());
        }

        s.GetCellValue("A11");
        s.GetCellValue("A111");
        s.GetCellValue("A1111");

        s.Save("test.txt");

        Spreadsheet s2 = new Spreadsheet("test.txt", s => true, s => s, "default");

    }
    // Verifies cells and their values, which must alternate.
    public void VV(AbstractSpreadsheet sheet, params object[] constraints)
    {
        for (int i = 0; i < constraints.Length; i += 2)
        {
            if (constraints[i + 1] is double)
            {
                Assert.AreEqual((double)constraints[i + 1], (double)sheet.GetCellValue((string)constraints[i]), 1e-9);
            }
            else
            {
                Assert.AreEqual(constraints[i + 1], sheet.GetCellValue((string)constraints[i]));
            }
        }
    }


    // For setting a spreadsheet cell.
    public IEnumerable<string> Set(AbstractSpreadsheet sheet, string name, string contents)
    {
        List<string> result = new List<string>(sheet.SetContentsOfCell(name, contents));
        return result;
    }

    // Tests IsValid
    [TestMethod, Timeout(2000)]
    [TestCategory("1")]
    public void IsValidTest1()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "x");
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("2")]
    [ExpectedException(typeof(InvalidNameException))]
    public void IsValidTest2()
    {
        AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
        ss.SetContentsOfCell("A1", "x");
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("3")]
    public void IsValidTest3()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "= A1 + C1");
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("4")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void IsValidTest4()
    {
        AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
        ss.SetContentsOfCell("B1", "= A1 + C1");
    }

    // Tests Normalize
    [TestMethod, Timeout(2000)]
    [TestCategory("5")]
    public void NormalizeTest1()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "hello");
        Assert.AreEqual("", s.GetCellContents("b1"));
    }

    [TestMethod]
    [TestCategory("6")]
    public void NormalizeTest2()
    {
        AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
        ss.SetContentsOfCell("B1", "hello");
        Assert.AreEqual("hello", ss.GetCellContents("b1"));
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("7")]
    public void NormalizeTest3()
    {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("a1", "5");
        s.SetContentsOfCell("A1", "6");
        s.SetContentsOfCell("B1", "= a1");
        Assert.AreEqual(5.0, (double)s.GetCellValue("B1"), 1e-9);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("8")]
    public void NormalizeTest4()
    {
        AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
        ss.SetContentsOfCell("a1", "5");
        ss.SetContentsOfCell("A1", "6");
        ss.SetContentsOfCell("B1", "= a1");
        Assert.AreEqual(6.0, (double)ss.GetCellValue("B1"), 1e-9);
    }

    // Simple tests
    [TestMethod, Timeout(2000)]
    [TestCategory("9")]
    public void EmptySheet()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        VV(ss, "A1", "");
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("10")]
    public void OneString()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        OneString(ss);
    }

    public void OneString(AbstractSpreadsheet ss)
    {
        Set(ss, "B1", "hello");
        VV(ss, "B1", "hello");
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("11")]
    public void OneNumber()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        OneNumber(ss);
    }

    public void OneNumber(AbstractSpreadsheet ss)
    {
        Set(ss, "C1", "17.5");
        VV(ss, "C1", 17.5);
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("12")]
    public void OneFormula()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        OneFormula(ss);
    }

    public void OneFormula(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.1");
        Set(ss, "B1", "5.2");
        Set(ss, "C1", "= A1+B1");
        VV(ss, "A1", 4.1, "B1", 5.2, "C1", 9.3);
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("13")]
    public void ChangedAfterModify()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        Assert.IsFalse(ss.Changed);
        ss.SetContentsOfCell( "C1", "17.5");
        Assert.IsTrue(ss.Changed);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("13b")]
    public void UnChangedAfterSave()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        Set(ss, "C1", "17.5");
        ss.Save("changed.txt");
        Assert.IsFalse(ss.Changed);
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("14")]
    public void DivisionByZero1()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        DivisionByZero1(ss);
    }

    public void DivisionByZero1(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.1");
        Set(ss, "B1", "0.0");
        Set(ss, "C1", "= A1 / B1");
        Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("15")]
    public void DivisionByZero2()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        DivisionByZero2(ss);
    }

    public void DivisionByZero2(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "5.0");
        Set(ss, "A3", "= A1 / 0.0");
        Assert.IsInstanceOfType(ss.GetCellValue("A3"), typeof(FormulaError));
    }



    [TestMethod, Timeout(2000)]
    [TestCategory("16")]
    public void EmptyArgument()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        EmptyArgument(ss);
    }

    public void EmptyArgument(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.1");
        Set(ss, "C1", "= A1 + B1");
        Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("17")]
    public void StringArgument()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        StringArgument(ss);
    }

    public void StringArgument(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.1");
        Set(ss, "B1", "hello");
        Set(ss, "C1", "= A1 + B1");
        Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("18")]
    public void ErrorArgument()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        ErrorArgument(ss);
    }

    public void ErrorArgument(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.1");
        Set(ss, "B1", "");
        Set(ss, "C1", "= A1 + B1");
        Set(ss, "D1", "= C1");
        Assert.IsInstanceOfType(ss.GetCellValue("D1"), typeof(FormulaError));
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("19")]
    public void NumberFormula1()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        NumberFormula1(ss);
    }

    public void NumberFormula1(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.1");
        Set(ss, "C1", "= A1 + 4.2");
        VV(ss, "C1", 8.3);
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("20")]
    public void NumberFormula2()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        NumberFormula2(ss);
    }

    public void NumberFormula2(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "= 4.6");
        VV(ss, "A1", 4.6);
    }


    // Repeats the simple tests all together
    [TestMethod, Timeout(2000)]
    [TestCategory("21")]
    public void RepeatSimpleTests()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        Set(ss, "A1", "17.32");
        Set(ss, "B1", "This is a test");
        Set(ss, "C1", "= A1+B1");
        OneString(ss);
        OneNumber(ss);
        OneFormula(ss);
        DivisionByZero1(ss);
        DivisionByZero2(ss);
        StringArgument(ss);
        ErrorArgument(ss);
        NumberFormula1(ss);
        NumberFormula2(ss);
    }

    // Four kinds of formulas
    [TestMethod, Timeout(2000)]
    [TestCategory("22")]
    public void Formulas()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        Formulas(ss);
    }

    public void Formulas(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "4.4");
        Set(ss, "B1", "2.2");
        Set(ss, "C1", "= A1 + B1");
        Set(ss, "D1", "= A1 - B1");
        Set(ss, "E1", "= A1 * B1");
        Set(ss, "F1", "= A1 / B1");
        VV(ss, "C1", 6.6, "D1", 2.2, "E1", 4.4 * 2.2, "F1", 2.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("23")]
    public void Formulasa()
    {
        Formulas();
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("24")]
    public void Formulasb()
    {
        Formulas();
    }


    // Are multiple spreadsheets supported?
    [TestMethod, Timeout(2000)]
    [TestCategory("25")]
    public void Multiple()
    {
        AbstractSpreadsheet s1 = new Spreadsheet();
        AbstractSpreadsheet s2 = new Spreadsheet();
        Set(s1, "X1", "hello");
        Set(s2, "X1", "goodbye");
        VV(s1, "X1", "hello");
        VV(s2, "X1", "goodbye");
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("26")]
    public void Multiplea()
    {
        Multiple();
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("27")]
    public void Multipleb()
    {
        Multiple();
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("28")]
    public void Multiplec()
    {
        Multiple();
    }

    // Reading/writing spreadsheets
    [TestMethod, Timeout(2000)]
    [TestCategory("29")]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void SaveTest1()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        ss.Save(Path.GetFullPath("/missing/save.txt"));
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("30")]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void SaveTest2()
    {
        AbstractSpreadsheet ss = new Spreadsheet(Path.GetFullPath("/missing/save.txt"), s => true, s => s, "");
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("31")]
    public void SaveTest3()
    {
        AbstractSpreadsheet s1 = new Spreadsheet();
        Set(s1, "A1", "hello");
        s1.Save("save1.txt");
        s1 = new Spreadsheet("save1.txt", s => true, s => s, "default");
        Assert.AreEqual("hello", s1.GetCellContents("A1"));
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("32")]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void SaveTest4()
    {
        using (StreamWriter writer = new StreamWriter("save2.txt"))
        {
            writer.WriteLine("This");
            writer.WriteLine("is");
            writer.WriteLine("a");
            writer.WriteLine("test!");
        }
        AbstractSpreadsheet ss = new Spreadsheet("save2.txt", s => true, s => s, "");
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("33")]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void SaveTest5()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        ss.Save("save3.txt");
        ss = new Spreadsheet("save3.txt", s => true, s => s, "version");
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("35")]
    public void SaveTest7()
    {
        var sheet = new
        {
            Cells = new
            {
                A1 = new { StringForm = "hello" },
                A2 = new { StringForm = "5.0" },
                A3 = new { StringForm = "4.0" },
                A4 = new { StringForm = "= A2 + A3" }
            },
            Version = ""
        };

        File.WriteAllText("save5.txt", JsonSerializer.Serialize(sheet));


        AbstractSpreadsheet ss = new Spreadsheet("save5.txt", s => true, s => s, "");
        VV(ss, "A1", "hello", "A2", 5.0, "A3", 4.0, "A4", 9.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("36")]
    public void SaveTest8()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        Set(ss, "A1", "hello");
        Set(ss, "A2", "5.0");
        Set(ss, "A3", "4.0");
        Set(ss, "A4", "= A2 + A3");
        ss.Save("save6.txt");

        string fileContents = File.ReadAllText("save6.txt");
        JsonDocument o = JsonDocument.Parse(fileContents);
        Assert.AreEqual("default", o.RootElement.GetProperty("Version").ToString());
        Assert.AreEqual("hello", o.RootElement.GetProperty("Cells").GetProperty("A1").GetProperty("StringForm").ToString());
        Assert.AreEqual(5.0, double.Parse(o.RootElement.GetProperty("Cells").GetProperty("A2").GetProperty("StringForm").ToString()), 1e-9);
        Assert.AreEqual(4.0, double.Parse(o.RootElement.GetProperty("Cells").GetProperty("A3").GetProperty("StringForm").ToString()), 1e-9);
        Assert.AreEqual("=A2+A3", o.RootElement.GetProperty("Cells").GetProperty("A4").GetProperty("StringForm").ToString().Replace(" ", ""));
    }


    // Fun with formulas
    [TestMethod]
    [TestCategory("37")]
    public void Formula1()
    {
        Formula1(new Spreadsheet());
    }
    public void Formula1(AbstractSpreadsheet ss)
    {
        Set(ss, "a1", "= a2 + a3");
        Set(ss, "a2", "= b1 + b2");
        Assert.IsInstanceOfType(ss.GetCellValue("a1"), typeof(FormulaError));
        Assert.IsInstanceOfType(ss.GetCellValue("a2"), typeof(FormulaError));
        Set(ss, "a3", "5.0");
        Set(ss, "b1", "2.0");
        Set(ss, "b2", "3.0");
        VV(ss, "a1", 10.0, "a2", 5.0);
        Set(ss, "b2", "4.0");
        VV(ss, "a1", 11.0, "a2", 6.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("38")]
    public void Formula2()
    {
        Formula2(new Spreadsheet());
    }
    public void Formula2(AbstractSpreadsheet ss)
    {
        Set(ss, "a1", "= a2 + a3");
        Set(ss, "a2", "= a3");
        Set(ss, "a3", "6.0");
        VV(ss, "a1", 12.0, "a2", 6.0, "a3", 6.0);
        Set(ss, "a3", "5.0");
        VV(ss, "a1", 10.0, "a2", 5.0, "a3", 5.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("39")]
    public void Formula3()
    {
        Formula3(new Spreadsheet());
    }
    public void Formula3(AbstractSpreadsheet ss)
    {
        Set(ss, "a1", "= a3 + a5");
        Set(ss, "a2", "= a5 + a4");
        Set(ss, "a3", "= a5");
        Set(ss, "a4", "= a5");
        Set(ss, "a5", "9.0");
        VV(ss, "a1", 18.0);
        VV(ss, "a2", 18.0);
        Set(ss, "a5", "8.0");
        VV(ss, "a1", 16.0);
        VV(ss, "a2", 16.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("40")]
    public void Formula4()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        Formula1(ss);
        Formula2(ss);
        Formula3(ss);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("41")]
    public void Formula4a()
    {
        Formula4();
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("42")]
    public void MediumSheet()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        MediumSheet(ss);
    }

    public void MediumSheet(AbstractSpreadsheet ss)
    {
        Set(ss, "A1", "1.0");
        Set(ss, "A2", "2.0");
        Set(ss, "A3", "3.0");
        Set(ss, "A4", "4.0");
        Set(ss, "B1", "= A1 + A2");
        Set(ss, "B2", "= A3 * A4");
        Set(ss, "C1", "= B1 + B2");
        VV(ss, "A1", 1.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 3.0, "B2", 12.0, "C1", 15.0);
        Set(ss, "A1", "2.0");
        VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 4.0, "B2", 12.0, "C1", 16.0);
        Set(ss, "B1", "= A1 / A2");
        VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("43")]
    public void MediumSheeta()
    {
        MediumSheet();
    }


    [TestMethod, Timeout(2000)]
    [TestCategory("44")]
    public void MediumSave()
    {
        AbstractSpreadsheet ss = new Spreadsheet();
        MediumSheet(ss);
        ss.Save("save7.txt");
        ss = new Spreadsheet("save7.txt", s => true, s => s, "default");
        VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
    }

    [TestMethod, Timeout(2000)]
    [TestCategory("45")]
    public void MediumSavea()
    {
        MediumSave();
    }


    // A long chained formula. Solutions that re-evaluate 
    // cells on every request, rather than after a cell changes,
    // will timeout on this test.
    // This test is repeated to increase its scoring weight
    [TestMethod, Timeout(6000)]
    [TestCategory("46")]
    public void LongFormulaTest()
    {
        object result = "";
        LongFormulaHelper(out result);
        Assert.AreEqual("ok", result);
    }

    [TestMethod, Timeout(6000)]
    [TestCategory("47")]
    public void LongFormulaTest2()
    {
        object result = "";
        LongFormulaHelper(out result);
        Assert.AreEqual("ok", result);
    }

    [TestMethod, Timeout(6000)]
    [TestCategory("48")]
    public void LongFormulaTest3()
    {
        object result = "";
        LongFormulaHelper(out result);
        Assert.AreEqual("ok", result);
    }

    [TestMethod, Timeout(6000)]
    [TestCategory("49")]
    public void LongFormulaTest4()
    {
        object result = "";
        LongFormulaHelper(out result);
        Assert.AreEqual("ok", result);
    }

    [TestMethod, Timeout(6000)]
    [TestCategory("50")]
    public void LongFormulaTest5()
    {
        object result = "";
        LongFormulaHelper(out result);
        Assert.AreEqual("ok", result);
    }

    public void LongFormulaHelper(out object result)
    {
        try
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("sum1", "= a1 + a2");
            int i;
            int depth = 100;
            for (i = 1; i <= depth * 2; i += 2)
            {
                s.SetContentsOfCell("a" + i, "= a" + (i + 2) + " + a" + (i + 3));
                s.SetContentsOfCell("a" + (i + 1), "= a" + (i + 2) + "+ a" + (i + 3));
            }
            s.SetContentsOfCell("a" + i, "1");
            s.SetContentsOfCell("a" + (i + 1), "1");
            Assert.AreEqual(Math.Pow(2, depth + 1), (double)s.GetCellValue("sum1"), 1.0);
            s.SetContentsOfCell("a" + i, "0");
            Assert.AreEqual(Math.Pow(2, depth), (double)s.GetCellValue("sum1"), 1.0);
            s.SetContentsOfCell("a" + (i + 1), "0");
            Assert.AreEqual(0.0, (double)s.GetCellValue("sum1"), 0.1);
            result = "ok";
        }
        catch (Exception e)
        {
            result = e;
        }
    }

}


