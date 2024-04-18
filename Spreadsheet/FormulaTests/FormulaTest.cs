using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTester;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void createSimpleFormulas()
    {
        Formula f = new Formula("x+y");
        Formula f2 = new Formula("_1 + y2");
        Formula f3 = new Formula("X1 + y2");
        Formula f4 = new Formula("x2 + y22__");
        Formula f5 = new Formula("x2 +       y7 /        7");
        Formula f6 = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
    }
    [TestMethod]
    public void createComplicatedFormulas()
    {
        Formula f = new Formula("x+y", s => s.ToUpper(), s => true);
        Formula f2 = new Formula("_1 + y2", s => s.ToUpper(), s => true);
        Formula f3 = new Formula("X1 + y2");
        Formula f4 = new Formula("x2 + y22__");
        Formula f5 = new Formula("x2 +       y7 /        7");
        Formula f6 = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
        Formula f7 = new Formula("x2+_y", s => s.ToUpper(), s => Regex.IsMatch(s, "[a-zA-Z_][0-9]*"));
    }
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void incorrectConstructorCall()
    {
        Formula f = new Formula("2x + y");
        Formula f2 = new Formula("2 2 + y      +    x");
        Formula f3 = new Formula("");
        Formula f4 = new Formula("+ 2 + x3)");
        Formula f5 = new Formula("x2 + x3 +");
        Formula f6 = new Formula("(x2 + y3))");
        Formula f7 = new Formula("(((x2 + y3))");
        Formula f8 = new Formula("(+ y3)");
        Formula f9 = new Formula(")+ y3)");
        Formula f10 = new Formula(")+ y3)");
        Formula f11 = new Formula("x+y", s => s.ToUpper(), s => false);
        Formula f12 = new Formula("x+y", s => s.ToUpper(), s => Regex.IsMatch(s, "[a-zA-Z_][0-9]"));
    }
    [TestMethod]
    public void evaluateFormula()
    {
        Assert.AreEqual(20.0, new Formula("x2 * 10", s => s, s => true).Evaluate(s => 2));
        Assert.AreEqual(9.0, new Formula("x2 + 7", s => s, s => true).Evaluate(s => 2));
        Assert.AreEqual(9.0, new Formula("7 + x2 / (7 - 3)", s => s, s => true).Evaluate(s => 8));
        Assert.AreEqual(9.0, new Formula("7 + x2 / ((7 - 3))", s => s, s => true).Evaluate(s => 8));
        Assert.AreEqual(9.0, new Formula("7 + (x2+2) / (5 - 0)", s => s, s => true).Evaluate(s => 8));
        Assert.AreEqual(9.0, new Formula("(7) + (x2+2) / (5) ", s => s, s => true).Evaluate(s => 8));
        Assert.AreEqual(12.0, new Formula("((((x1+x2)+x3)+x4)+x5)+x6", s => s, s => true).Evaluate(s => 2));
    }
    [TestMethod]
    public void evaluateFormulaDivideby0()
    {
        Assert.AreEqual(new FormulaError("tried to divide by 0"), new Formula("x2 / 0", s => s, s => true).Evaluate(s => 10));
        Assert.AreEqual(new FormulaError("lookup method threw an error"), new Formula("x2 / 1", s => s, s => true).Evaluate(s => { throw new ArgumentException(); }));
    }
    [TestMethod]
    public void getVariables()
    {
        Assert.AreEqual("x2", new Formula("x2 + y2 * y4").GetVariables().ElementAt(0));
        Assert.AreEqual("y2", new Formula("x2 + y2 * y4").GetVariables().ElementAt(1));
        Assert.AreEqual("y4", new Formula("x2 + y2 * y4").GetVariables().ElementAt(2));
        Assert.AreEqual("x5", new Formula("((((x1+x2)+x3)+x4)+x5)+x6").GetVariables().ElementAt(4));
        Assert.AreEqual("x2", new Formula("x2 + X2 * y4").GetVariables().ElementAt(0));
        Assert.AreEqual("X2", new Formula("x2 + X2 * y4").GetVariables().ElementAt(1));
    }
    [TestMethod]
    public void toString()
    {
        Formula f = new Formula("x+y");
        Formula f2 = new Formula("_1 + y2");
        Formula f3 = new Formula("X1 + y2");
        Formula f4 = new Formula("x2 + y22__");
        Formula f5 = new Formula("x2 +       y7 /        7");
        Formula f6 = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
        Formula f7 = new Formula("(x+y)");

        Assert.AreEqual("x+y", f.ToString());
        Assert.AreEqual("X+Y", new Formula("x+y", s => s.ToUpper(), s => true).ToString());
        Assert.AreEqual("_1+y2", f2.ToString());
        Assert.AreEqual("y1*3-8/2+4*(8-9*2)/14*x7", f6.ToString());


        Assert.AreNotEqual("x+y", f7.ToString());
        Assert.AreNotEqual("X+y", f.ToString());
    }
    [TestMethod]
    public void equalsTest()
    {
        Formula f = new Formula("x+y");
        Formula f2 = new Formula("_1 + y2");
        Formula f3 = new Formula("X1 + y2");
        Formula f4 = new Formula("x2 + y22__");
        Formula f5 = new Formula("x2 +       y7 /        7");
        Formula f6 = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
        Formula f7 = new Formula("x+y + 9.000");

        Assert.IsTrue(f.Equals(new Formula("x+y")));
        Assert.IsTrue(f.Equals(new Formula("x+  y")));
        Assert.IsFalse(f.Equals(new Formula("x+Y")));
        Assert.IsTrue(f7.Equals(new Formula("x + y + 9")));
        Assert.IsFalse(f7.Equals(new Formula("x + y + 8")));
        Assert.IsFalse(f7.Equals(new Formula("x + y + 8", s => s.ToUpper(), s => true)));
        Assert.IsTrue(f2.Equals(new Formula("_1 + y2         ")));
        Assert.IsTrue(new Formula("9.000 + x").Equals(new Formula("9 + x")));
    }
    [TestMethod]
    public void equalOverrides()
    {
        Formula f = new Formula("x+y");
        Formula f1 = new Formula("x+y");

        Assert.IsTrue(f == f1);
        Assert.IsFalse(f != f1);

        Assert.IsFalse(f == new Formula("x+ x2"));

        Assert.IsTrue(f != new Formula("x+ x2"));
    }
    [TestMethod]
    public void getHashcodes()
    {
        Formula f = new Formula("x+y");
        Formula f1 = new Formula("x+y");
        Formula f2 = new Formula("_1 + y2");
        Formula f3 = new Formula("X1 + y2");
        Formula f4 = new Formula("x2 + y22__");
        Formula f5 = new Formula("x2 +       y7 /        7");
        Formula f6 = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
        Formula f7 = new Formula("x+y + 9.000");

        Assert.IsTrue(f.GetHashCode() == f1.GetHashCode());
        Assert.IsTrue(f.GetHashCode() == new Formula("x +       y").GetHashCode());
        Assert.IsFalse(f.GetHashCode() == new Formula("(x+y)").GetHashCode());


        Assert.IsFalse(f.GetHashCode() == new Formula("x+y", s => s.ToUpper(), s => true).GetHashCode());
        Assert.IsTrue(f6.GetHashCode() == new Formula("y1*3-8/2+4*(8-9*2)/14*x7", s => s, s => true).GetHashCode());
    }

    