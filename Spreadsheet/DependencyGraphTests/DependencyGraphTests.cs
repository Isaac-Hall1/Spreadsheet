﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace DevelopmentTests;

/// <summary>
///This is a test class for DependencyGraphTest and is intended
///to contain all DependencyGraphTest Unit Tests (once completed by the student)
///</summary>
[TestClass()]
public class DependencyGraphTest
{

    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyTest()
    {
        DependencyGraph t = new DependencyGraph();
        Assert.AreEqual(0, t.NumDependencies);
    }


    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyRemoveTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(1, t.NumDependencies);
        t.RemoveDependency("x", "y");
        Assert.AreEqual(0, t.NumDependencies);
    }


    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void EmptyEnumeratorTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
        Assert.IsTrue(e1.MoveNext());
        Assert.AreEqual("x", e1.Current);
        //IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
        //Assert.IsTrue(e2.MoveNext());
        //Assert.AreEqual("y", e2.Current);
        //t.RemoveDependency("x", "y");
        //Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
        //Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
    }


    /// <summary>
    ///Replace on an empty DG shouldn't fail
    ///</summary>
    [TestMethod()]
    public void SimpleReplaceTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(t.NumDependencies, 1);
        t.RemoveDependency("x", "y");
        t.ReplaceDependents("x", new HashSet<string>());
        t.ReplaceDependees("y", new HashSet<string>());
    }



    ///<summary>
    ///It should be possibe to have more than one DG at a time.
    ///</summary>
    [TestMethod()]
    public void StaticTest()
    {
        DependencyGraph t1 = new DependencyGraph();
        DependencyGraph t2 = new DependencyGraph();
        t1.AddDependency("x", "y");
        Assert.AreEqual(1, t1.NumDependencies);
        Assert.AreEqual(0, t2.NumDependencies);
    }




    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void SizeTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");
        Assert.AreEqual(4, t.NumDependencies);
    }


    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void EnumeratorTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        // This is one of several ways of testing whether your IEnumerable
        // contains the right values. This does not require any particular
        // ordering of the elements returned.
        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }


    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void ReplaceThenEnumerate()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "b");
        t.AddDependency("a", "z");
        t.ReplaceDependents("b", new HashSet<string>());
        t.AddDependency("y", "b");
        t.ReplaceDependents("a", new HashSet<string>() { "c" });
        t.AddDependency("w", "d");
        t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
        t.ReplaceDependees("d", new HashSet<string>() { "b" });

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());


    }



}