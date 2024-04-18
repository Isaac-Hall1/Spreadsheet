// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)
// Author: Isaac Hall
// Date: 9/5/2023


namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
/// </summary>
///
public class DependencyGraph 
{
    Dictionary<string,HashSet<string>> dependents;
    Dictionary<string, HashSet<string>> dependees;


    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph()
    {
        dependents = new Dictionary<string, HashSet<string>>();
        dependees = new Dictionary<string, HashSet<string>>();
    }




    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies
    {
        get
        {
            // go through my dependents map and add the size of each hashSet to a count value
            // because that represents the amount of pairs.
            int pairs = 0;
            foreach(KeyValuePair<string,HashSet<string>> entry in dependents)
            {
               pairs += entry.Value.Count;
            }
            return pairs;
        }
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    public int NumDependees(string s)
    {
        // check the size of the set within the dependencyGraph dict to see the amount of entries that are dependent
        // on a given item
        HashSet<string>? set;
        if(dependees.TryGetValue(s, out set))
        {
            return set.Count;
        }
        return 0;
            
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        // check to see if theres more than one item in each maps set of dependents
        HashSet<string>? set;
        if (dependents.TryGetValue(s, out set))
        {
            if(set.Count > 0)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        // check to see if theres more than one item in each maps set of dependees
        HashSet<string>? set;
        if (dependees.TryGetValue(s, out set))
        {
            if (set.Count > 0)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        // return the set from given map entry of dependents
        HashSet<string>? set;
        if (dependents.TryGetValue(s, out set))
        {
            return set;
        }
        return new HashSet<string>();
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {
        // return the set from the given map entry of dependees 
        HashSet<string>? set;
        if (dependees.TryGetValue(s, out set))
        {
            return set;
        }
        return new HashSet<string>();
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t)
    {
        HashSet<string>? dependentsSet;
        HashSet<string>? dependeesSet;

        // if theres an instance of s within the dependents map add 
        // t to the set of the map value
        if (dependents.TryGetValue(s, out dependentsSet))
        {
            dependentsSet.Add(t);
            // if theres an instance of t within dependees add s to that set 
            if (dependees.TryGetValue(t, out dependeesSet))
            {
                dependeesSet.Add(s);
                return;
            }
            // if there isn't an instance of t within dependees create a new one and add s to it
            dependees.Add(t, new HashSet<string> { s});
            return;
        }

        else
        {
            // if there isn't an instance of s within dependents create a new one and add to to its set
            dependents.Add(s, new HashSet<string> { t});

            // if theres already an instance of t within dependees just add s to the set
            if (dependees.TryGetValue(t, out dependeesSet))
            {
                dependeesSet.Add(s);
                return;
            }
            // if there isn't an instance of t, create it and add s to it
            dependees.Add(t, new HashSet<string> { s});
        }

    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t)
    {
        HashSet<string>? dependentsSet;
        HashSet<string>? dependeesSet;

        // if s exists in dependents and it has the dependent value t, get rid of it
        // and remove s from the instance of t wihtin dependees
        if (dependents.TryGetValue(s, out dependentsSet) && dependentsSet.Contains(t))
        {
            dependentsSet.Remove(t);
            if(dependees.TryGetValue(t, out dependeesSet))
                dependeesSet.Remove(s);
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {

        // if s is not within dependents there's nothing to replace so return
        if(!dependents.TryGetValue(s, out HashSet<string>? unNeededSet))
        {
            dependents.Add(s, (HashSet<string>)newDependents);

            // if one of the replaced values in the new set doesnt exist within dependees
            // add such value to dependees
            foreach (var entry in newDependents)
            {
                if (!dependees.ContainsKey(entry))
                {
                    dependees.Add(entry, new HashSet<string> { s });
                }
                dependees.TryGetValue(entry, out HashSet<string>? tempAdd);
                tempAdd?.Add(s);
            }
            return;
        }
        if (dependents.TryGetValue(s, out HashSet<string>? originalSet))
        {
            dependents.Remove(s);
            dependents.Add(s, (HashSet<string>)newDependents);

            // if one of the replaced values in the new set doesnt exist within dependees
            // add such value to dependees
            foreach (var entry in newDependents)
            {
                if (!dependees.ContainsKey(entry))
                {
                    dependees.Add(entry, new HashSet<string> { s });
                }
                dependees.TryGetValue(entry, out HashSet<string>? tempAdd);
                tempAdd?.Add(s);
            }

            // removes s from a dependees entry's hashset if the entry is not within the new given set
            foreach (var entry in originalSet)
            {
                if (dependees.ContainsKey(entry))
                {
                    if (!newDependents.Contains(entry))
                    {
                        dependees.TryGetValue(entry, out HashSet<string>? temp);
                        temp?.Remove(s);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {

        // if s is not within dependents there's nothing to replace so return
        if (!dependees.TryGetValue(s, out HashSet<string>? unNeededSet))
        {
            dependees.Add(s, (HashSet<string>)newDependees);
            foreach (var entry in newDependees)
            {
                if (!dependents.ContainsKey(entry))
                {
                    dependents.Add(entry, new HashSet<string> { s });
                }
                dependents.TryGetValue(entry, out HashSet<string>? tempAdd);
                tempAdd?.Add(s);
            }
            return;
        }
        if (dependees.TryGetValue(s, out HashSet<string>? originalSet))
        {
            dependees.Remove(s);
            dependees.Add(s, (HashSet<string>)newDependees);

            // if one of the replaced values in the new set doesnt exist within dependees
            // add such value to dependees
            foreach (var entry in newDependees)
            {
                if (!dependents.ContainsKey(entry))
                {
                    dependents.Add(entry, new HashSet<string> { s });
                }
                dependents.TryGetValue(entry, out HashSet<string>? tempAdd);
                tempAdd?.Add(s);
            }
            // removes s from a dependees entry's hashset if the entry is not within the new given set
            foreach (var entry in originalSet)
            {
                if (dependents.ContainsKey(entry))
                {
                    if (!newDependees.Contains(entry))
                    {
                        dependents.TryGetValue(entry, out HashSet<string>? newSet);
                        newSet?.Remove(s);
                    }
                }
            }
        }
    }
}