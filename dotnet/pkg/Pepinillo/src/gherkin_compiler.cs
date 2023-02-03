// datagrove gherkin compiler
namespace Datagrove.Pep;
using System.Text.RegularExpressions;
using Gherkin;
using System.Reflection;
using System.Text;
using Gherkin.Ast;


using Compiled = System.Action<string, string, string>;
using System.CodeDom.Compiler;


// each step is part of a class that will get initialized
// [Scope("gherkin_tag")]
// potentially add gherkin tags based on folder hierarchy
// naming base on first tag is bad idea? at best its asi specific.

// asi tests use table, so we need to give them a table.


// these are things we need to invoke at compile time to transform the string into a bool or an int or something. 
public class TransformArg
{

    Regex regex;
    MethodInfo m;
    Object obj;
    public TransformArg(String s, Object o, MethodInfo m)
    {
        this.regex = new Regex(s);
        this.m = m;
        this.obj = o;
    }
    public bool tryTransform(string s, Type t, out string r)
    {
        // if the string matches, then we need to pass it to the method then ToString() it's result.
        r = "";
        try
        {
            if (regex.IsMatch(s))
            {
                // m.ReturnType==t && 
                var o = m.Invoke(obj, new object[] { s });
                if (o is bool ob)
                {
                    r = ob ? "true" : "false";
                }
                else
                    r = o?.ToString() ?? "";
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }
}
public class Namer
{
    Dictionary<string, int> mname = new Dictionary<string, int>();

    public string name(string sname)
    {
        int n = 0;
        if (mname.TryGetValue(sname, out n))
        {
            n++;
        }
        mname[sname] = n;
        if (n != 0)
            sname += $"{n}";
        return sname;
    }
}
public class GherkinCompiler
{
    public static void compile(Compiled compiled, string file, StepSet ss, Namer nm, BuildGherkin bg, GherkinDocumentation docm)
    {
        var parser = new Parser();
        try
        {
            var doc = parser.Parse(file);
            var x = new StringBuilder();
            // Creates a TextWriter to use as the base output writer.
            var baseTextWriter = new System.IO.StringWriter();
            var methods = new IndentedTextWriter(baseTextWriter, "    ");
            methods.Indent = 1;

            var bgb = new System.IO.StringWriter();
            var bgw = new IndentedTextWriter(bgb, "    ");
            bgw.Indent++;

            var tags = String.Join("", doc.Feature.Tags.Select((e) => $"[TestCategory(\"{e.Name.Substring(1)}\")]\n"));

            var name = symbol(doc.Feature.Name);
            name = nm.name(name);
            docm.addFeature(file, name);
            var comments = doc.Comments;
            var children = doc.Feature.Children;
            var nmr = new Namer();
            bool isBackground = false;
            // each step has a class and we need to collect them to add them as members to the class.
            var stepClass = new HashSet<string>();
            var appendStep = (Step step, string text, IndentedTextWriter tw) =>
            {
                var cstep = ss.compile(step, text, (string w) => compiled(file, "", w));
                if (cstep != null)
                    docm.addStep(cstep);
                if (cstep == null)
                {
                    var error = $"\n## Not found\n{text}\n";
                    compiled(file, "", error);

                    // for debugging
                    var cstep2 = ss.compile(step, text, (string w) => compiled(file, "", w));
                }
                else
                {
                    stepClass.Add(cstep.step.classType.FullName ?? cstep.step.classType.Name);
                    // backgrounds don't initialize, they only use the members.
                    tw.WriteLine("step." + cstep.csharp);
                    //methods.WriteLine($"shot(\"{Uri.EscapeDataString(text.Replace(' ', '_'))}\");");
                }
            };
            foreach (var ch in doc.Feature.Children)
            {

                if (ch is Rule rule)
                {
                    // rules are a way to group methods and give them common tags
                    // not used in asi.
                }
                else if (ch is Background background)
                {
                    isBackground = true;
                    // we need to gather the steps, treat as a subroutine or macro expansion? if it's a subroutine, how do we share the context? It's class wide anyway.
                    // there are no examples in background steps.            
                    foreach (var st in background.Steps)
                    {
                        appendStep(st, st.Text, bgw);
                    }
                }
                else if (ch is Scenario scenario)
                {
                    bool ignore = false;
                    foreach (var tg in scenario.Tags)
                    {
                        if (tg.Name == "@Ignore" || tg.Name == "@Ignored")
                            ignore = true;
                        else
                            methods.WriteLine($"[TestCategory(\"{tg.Name.Substring(1)}\")]");
                    }
                    // we can't generate ignore methods, because they typically don't have steps.
                    if (ignore) continue;

                    var sname = scenario.Name;
                    // maybe here we should allow a @name:GoodName?
                    sname = symbol(sname);
                    sname = nmr.name(sname);
                    if (sname == name)
                    {
                        // can't use the class name/Feature name
                        sname += "_";
                    }

                    // collect the classes needed for this test.
                    var classes = new Dictionary<string, string>();

                    docm.addScenario(sname);

                    // !! we need to initialize the classes here
                    // we need to initialize any that are used in the background as well
                    // we need to call background if it exists.

                    var exn = 0;
                    // we should have a better way of doing this, most things don't need macro expansion.
                    var oneExample = (Dictionary<string, string> ed) =>
                    {
                        var suffix = exn == 0 ? "" : $"__{exn}";
                        methods.WriteLine($"[TestMethod()]");
                        methods.WriteLine($"public void {sname}{suffix}()");
                        methods.WriteLine("{");
                        methods.WriteLine("using (var actor = new AsiTest(TestContext!,\"actor\")){");
                        methods.Indent++;
                        methods.WriteLine("var step = new Steps(actor);");
                        foreach (var st in scenario.Steps)
                        {
                            var tx = st.Text;
                            foreach (var e in ed)
                            {
                                tx = tx.Replace("<" + e.Key + ">", e.Value);
                            }
                            appendStep(st, tx, methods);
                        }

                        methods.WriteLine("}");
                        methods.Indent--;
                        methods.WriteLine("}");
                        exn++;
                    };

                    // we can have multiple Examples? how does that work?
                    var ed = new Dictionary<string, string>();
                    int cnt = scenario.Examples.Count();
                    if (cnt == 0)
                    {
                        oneExample(ed);
                    }
                    else if (cnt > 0)
                    {
                        if (cnt > 1)
                        {
                            Console.Write("");
                        }
                        var tbl = scenario.Examples.First()!;
                        string[] header = tbl.TableHeader.Cells.Select(e => e.Value).ToArray();
                        var d = new List<string>();
                        foreach (var k in tbl.TableBody)
                        {
                            ed.Clear();
                            int ik = 0;
                            foreach (var cl in k.Cells)
                            {
                                ed.Add(header[ik++], cl.Value);
                            }
                            oneExample(ed);
                        }

                    }


                }
            }



            var member = "";
            var featureConstructor = "";
            foreach (var cn in stepClass)
            {
                // substitute short names
                var nm3 = ss.usingNamespace[cn];
                var nmx = nm3.Substring(0, 1).ToLower() + nm3.Substring(1);
                member += $"        internal {cn} {nmx};\n";
                if ("inventoryReceiptsStepDef" != nmx)
                {
                    featureConstructor += $"            {nmx} = new {cn}(actor.driver,actor.context);\n";
                }
                else
                {
                    featureConstructor += $"            {nmx} = new {cn}(actor.driver,actor.context,actor.scenario);\n";
                }
            }

            var stepDef = @$"
    public class Steps {{
{member}
        public Steps(AsiTest actor)
        {{
{featureConstructor}
            var step=this;
{bgb.ToString()}
        }}
    }}
";

            var debugInfo = "";
            var category = $"[TestCategory(\"{bg.tag}\")]";
            var result = $@"
// {file}
[TestClass()]
{category}
{tags}public class {name} : BaseTest
{{
    {stepDef}
    {baseTextWriter.ToString()}
    {debugInfo}
}}
";
            compiled(file, result, "");
        }
        catch (Exception e)
        {
            compiled(file, "", e.ToString());
        }
    }


    public StepSet ss;

    public GherkinCompiler(string[] stepSpace, Assembly? prog = null)
    {

        ss = new StepSet(prog ?? System.Reflection.Assembly.GetExecutingAssembly(), stepSpace);
    }

    // we should also give this a set of assemblies
    // compiled(path,result,error)
    public GherkinDocumentation compile(Compiled compiled, string[] files, BuildGherkin bg)
    {
        var doc = new GherkinDocumentation(bg, files);
        var nmr = new Namer();
        for (var j = 0; j < files.Count(); j++)
        {
            compile(compiled, files[j], ss, nmr, bg, doc);
        }
        return doc;
    }

    public string[] findFeatures(string[] dir)
    {
        var r = new List<string>();
        foreach (var o in dir)
        {
            r = r.Concat(Directory.GetFiles(o,
                            "*.feature",
                            SearchOption.AllDirectories)).ToList();
        }
        return r.ToArray();
    }


    static public string symbol(string x)
    {
        var sb = new StringBuilder();
        foreach (var c in x)
        {
            if (c >= 48 && c <= 57 || c >= 65 && c <= 90 || c >= 97 && c <= 122)
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        return sb.ToString();
    }

}

// Gherkin doesn't natively understand namespaces, so we need to add some additional information about what types are useful for a given feature folder.
public class BuildGherkin
{
    public Assembly? assembly = null;
    public string[] stepSpace = new string[] { };
    public string[] featureFolder = new string[] { };
    public string outputFile = "";


    public string tag = "";

    public void build()
    {

        var sb = new StringBuilder();
        var log = new StringBuilder();
        var errors = new HashSet<string>();
        var compiled = (string f, string r, string e) =>
        {
            if (e != "" && !errors.Contains(e))
            {
                log.Append(e);
                errors.Add(e);
            }
            sb.Append(r);
        };

        var c = new GherkinCompiler(stepSpace, assembly);
        var fl = c.findFeatures(featureFolder);
        var doc = c.compile(compiled, fl, this);

        // not needed because we use fully qualified
        // + c.ss.usingText() 

        File.WriteAllText(outputFile, $@"// File generated by Datagrove Gherkin Compiler, If you edit this file, rename it and/or delete the .feature file that generates it.

namespace As1.{tag};
#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
" + sb.ToString());

        // here we add a bunch of debug information the markdown loag.
        log.Append(c.ss.text());
        File.WriteAllText(outputFile + ".md", log.ToString());

        File.WriteAllText(outputFile + ".json", doc.json());
    }

}



// public static void buildJson(Type t, FeatureData f)
// {

// }
// public static void findSteps(Assembly prog, string f)
// {
//     var a = new StepSet(prog);

//     File.WriteAllText(f, a.text());

// }

/* 
                  // the scenario name can be replaced by a tag.
                int i = 0;
                foreach (var tg in scenario.Tags)
                {
                    if (i == 0)
                    {
                        sname = tg.Name.Substring(1);
                    }
                    else
                    {
                        methods.Append($"[TestCategory(\"{tg.Name.Substring(1)}\")]\n");
                    }
                    i++;
                }
*/
// when do we need this? (st = Step)
// var type = st.KeywordType;
// switch (type)
// {
//     case StepKeywordType.Context: break;
//     case StepKeywordType.Action: break;
//     case StepKeywordType.Outcome: break;
//     case StepKeywordType.Conjunction: break;
// }
// we need to match all the steps

// should we just add the usings for all the steps?