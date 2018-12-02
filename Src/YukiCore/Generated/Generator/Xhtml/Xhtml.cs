//==========================================================================
//
//  Notice:      This file is automatically generated.
//               Please don't modify this file.
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Boolean = System.Boolean;
using String = System.String;
using Type = System.Type;
using Int = System.Int32;
using Real = System.Double;
using Byte = System.Byte;
using UInt8 = System.Byte;
using UInt16 = System.UInt16;
using UInt32 = System.UInt32;
using UInt64 = System.UInt64;
using Int8 = System.SByte;
using Int16 = System.Int16;
using Int32 = System.Int32;
using Int64 = System.Int64;
using Float32 = System.Single;
using Float64 = System.Double;

namespace Yuki.ObjectSchema.Xhtml
{
    partial class Templates
    {
        private IEnumerable<String> Begin()
        {
            yield return "";
        }
        private IEnumerable<String> Combine(IEnumerable<String> Left, String Right)
        {
            foreach (var vLeft in Left)
            {
                yield return vLeft + Right;
            }
        }
        private IEnumerable<String> Combine(IEnumerable<String> Left, Object Right)
        {
            foreach (var vLeft in Left)
            {
                yield return vLeft + Convert.ToString(Right, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        private IEnumerable<String> Combine(IEnumerable<String> Left, IEnumerable<String> Right)
        {
            foreach (var vLeft in Left)
            {
                foreach (var vRight in Right)
                {
                    yield return vLeft + vRight;
                }
            }
        }
        private IEnumerable<String> Combine<T>(IEnumerable<String> Left, IEnumerable<T> Right)
        {
            foreach (var vLeft in Left)
            {
                foreach (var vRight in Right)
                {
                    yield return vLeft + Convert.ToString(vRight, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }
        public IEnumerable<String> Ref(String Name, String Ref, String Description)
        {
            if (Description == "")
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "<a href=\""), GetEscaped(Ref)), "\">"), GetEscaped(Name)), "</a>"))
                {
                    yield return _Line;
                }
            }
            else
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "<a href=\""), GetEscaped(Ref)), "\" title=\""), GetEscaped(Description)), "\">"), GetEscaped(Name)), "</a>"))
                {
                    yield return _Line;
                }
            }
        }
        public IEnumerable<String> BarRef(String Name, String Ref, String Description)
        {
            if (Description == "")
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "<a href=\""), GetEscaped(Ref)), "\" target=\"contentFrame\">"), GetEscaped(Name)), "</a>"))
                {
                    yield return _Line;
                }
            }
            else
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "<a href=\""), GetEscaped(Ref)), "\" title=\""), GetEscaped(Description)), "\" target=\"contentFrame\">"), GetEscaped(Name)), "</a>"))
                {
                    yield return _Line;
                }
            }
        }
        public IEnumerable<String> Type(TypeDef t)
        {
            var Name = t.VersionedName();
            var MetaType = GetMetaType(t);
            var GenericParameters = t.GenericParameters();
            var Description = t.Description();
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "<h3 id=\""), GetEscaped(Name)), "\">"), GetEscaped(MetaType)), " "), GetEscaped(Name)), "</h3>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "<pre>"), GetEscaped(Description)), "</pre>"))
            {
                yield return _Line;
            }
            if (GenericParameters.Count > 0)
            {
                yield return "<table>";
                foreach (var gp in GenericParameters)
                {
                    foreach (var _Line in Combine(Begin(), Variable("'" + gp.Name, gp.Type, gp.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
                yield return "</table>";
            }
            if (t.OnPrimitive)
            {
            }
            else if (t.OnAlias)
            {
                var TypeSpec = GetTypeString(t.Alias.Type, true);
                foreach (var _Line in Combine(Combine(Combine(Begin(), "<pre>类型："), TypeSpec), "</pre>"))
                {
                    yield return _Line;
                }
            }
            else if (t.OnRecord)
            {
                yield return "<table>";
                foreach (var _Line in Combine(Combine(Begin(), "    "), Variables(t.Record.Fields)))
                {
                    yield return _Line;
                }
                yield return "</table>";
            }
            else if (t.OnTaggedUnion)
            {
                yield return "<table>";
                foreach (var _Line in Combine(Combine(Begin(), "    "), Variables(t.TaggedUnion.Alternatives)))
                {
                    yield return _Line;
                }
                yield return "</table>";
            }
            else if (t.OnEnum)
            {
                yield return "<table>";
                foreach (var _Line in Combine(Combine(Begin(), "    "), Literals(t.Enum.Literals)))
                {
                    yield return _Line;
                }
                yield return "</table>";
            }
            else if (t.OnClientCommand)
            {
                yield return "<div><pre>参数</pre></div>";
                yield return "<table>";
                foreach (var _Line in Combine(Combine(Begin(), "    "), Variables(t.ClientCommand.OutParameters)))
                {
                    yield return _Line;
                }
                yield return "</table>";
                yield return "<div><pre>返回值</pre></div>";
                yield return "<table>";
                foreach (var _Line in Combine(Combine(Begin(), "    "), Variables(t.ClientCommand.InParameters)))
                {
                    yield return _Line;
                }
                yield return "</table>";
            }
            else if (t.OnServerCommand)
            {
                yield return "<table>";
                foreach (var _Line in Combine(Combine(Begin(), "    "), Variables(t.ServerCommand.OutParameters)))
                {
                    yield return _Line;
                }
                yield return "</table>";
            }
            else
            {
                throw new InvalidOperationException();
            }
            yield return "<pre></pre>";
        }
        public IEnumerable<String> Variable(String Name, TypeSpec Type, String Description)
        {
            yield return "<tr>";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td>"), GetEscaped(Name)), "</td>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td>"), GetTypeString(Type, true)), "</td>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td><pre>"), GetEscaped(Description)), "</pre></td>"))
            {
                yield return _Line;
            }
            yield return "</tr>";
        }
        public IEnumerable<String> Variables(List<VariableDef> l)
        {
            if (l.Count > 0)
            {
                foreach (var v in l)
                {
                    foreach (var _Line in Combine(Begin(), Variable(v.Name, v.Type, v.Description)))
                    {
                        yield return _Line;
                    }
                }
            }
            else
            {
                yield return "<tr>";
                yield return "    <td><pre>        </pre></td>";
                yield return "    <td><pre>        </pre></td>";
                yield return "    <td><pre>        </pre></td>";
                yield return "</tr>";
            }
        }
        public IEnumerable<String> Literal(String Name, Int64 Value, String Description)
        {
            yield return "<tr>";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td>"), GetEscaped(Name)), "</td>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td>"), Value), "</td>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td><pre>"), GetEscaped(Description)), "</pre></td>"))
            {
                yield return _Line;
            }
            yield return "</tr>";
        }
        public IEnumerable<String> Literals(List<LiteralDef> l)
        {
            if (l.Count > 0)
            {
                foreach (var v in l)
                {
                    foreach (var _Line in Combine(Begin(), Literal(v.Name, v.Value, v.Description)))
                    {
                        yield return _Line;
                    }
                }
            }
            else
            {
                yield return "<tr>";
                yield return "    <td><pre>        </pre></td>";
                yield return "    <td><pre>        </pre></td>";
                yield return "    <td><pre>        </pre></td>";
                yield return "</tr>";
            }
        }
        public IEnumerable<String> Brief(String FilePath, IEnumerable<String> Types)
        {
            foreach (var _Line in Combine(Combine(Combine(Begin(), "<h3>"), GetEscaped(FilePath)), "</h3>"))
            {
                yield return _Line;
            }
            yield return "<table>";
            foreach (var _Line in Combine(Combine(Begin(), "    "), Types))
            {
                yield return _Line;
            }
            yield return "</table>";
            yield return "<pre></pre>";
        }
        public IEnumerable<String> TypeBrief(TypeDef t)
        {
            var ts = TypeSpec.CreateTypeRef(new TypeRef { Name = t.Name(), Version = t.Version() });
            var MetaType = GetMetaType(t);
            var Description = t.Description();
            yield return "<tr>";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td>"), GetTypeString(ts, false)), "</td>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td>"), GetEscaped(MetaType)), "</td>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <td><pre>"), GetEscaped(Description)), "</pre></td>"))
            {
                yield return _Line;
            }
            yield return "</tr>";
        }
        public IEnumerable<String> BarBrief(String FilePath, IEnumerable<String> Types)
        {
            foreach (var _Line in Combine(Combine(Combine(Begin(), "<h3>"), GetEscaped(FilePath)), "</h3>"))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Begin(), Types))
            {
                yield return _Line;
            }
        }
        public IEnumerable<String> BarTypeBrief(TypeDef t)
        {
            var ts = TypeSpec.CreateTypeRef(new TypeRef { Name = t.Name(), Version = t.Version() });
            foreach (var _Line in Combine(Combine(Combine(Begin(), "<p>"), GetTypeString(ts, true, true)), "</p>"))
            {
                yield return _Line;
            }
        }
        public IEnumerable<String> PageContent(String Name, String Title, String CopyrightText, IEnumerable<String> Content, Boolean UseBackToMain)
        {
            yield return "<head>";
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "    <title>"), GetEscaped(Name)), "-"), GetEscaped(Title)), "</title>"))
            {
                yield return _Line;
            }
            yield return "    <link rel=\"stylesheet\" href=\"style.css\" />";
            yield return "</head>";
            yield return "<body>";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <h2>"), GetEscaped(Title)), "</h2>"))
            {
                yield return _Line;
            }
            if (UseBackToMain)
            {
                yield return "    " + "<a href=\"main.html\">返回首页</a>";
            }
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <h1>"), GetEscaped(Name)), "</h1>"))
            {
                yield return _Line;
            }
            yield return "    <pre></pre>";
            foreach (var _Line in Combine(Combine(Begin(), "    "), Content))
            {
                yield return _Line;
            }
            yield return "    <pre></pre>";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <pre>"), GetEscaped(CopyrightText)), "</pre>"))
            {
                yield return _Line;
            }
            yield return "</body>";
        }
        public IEnumerable<String> BarPageContent(String Name, String Title, IEnumerable<String> Content)
        {
            yield return "<head>";
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "    <title>"), GetEscaped(Name)), "-"), GetEscaped(Title)), "</title>"))
            {
                yield return _Line;
            }
            yield return "    <link rel=\"stylesheet\" href=\"style.css\" />";
            yield return "</head>";
            yield return "<body>";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    <h2>"), GetEscaped(Title)), "</h2>"))
            {
                yield return _Line;
            }
            yield return "    <a href=\"main.html\" target=\"contentFrame\">所有</a>";
            foreach (var _Line in Combine(Combine(Begin(), "    "), Content))
            {
                yield return _Line;
            }
            yield return "</body>";
        }
        public IEnumerable<String> IndexPageContent(String Name, String Title)
        {
            yield return "<head>";
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "    <title>"), GetEscaped(Name)), "-"), GetEscaped(Title)), "</title>"))
            {
                yield return _Line;
            }
            yield return "    <link rel=\"stylesheet\" href=\"style.css\" />";
            yield return "</head>";
            yield return "<frameset cols=\"20%,80%\" title=\"\">";
            yield return "    <frame src=\"bar.html\" name=\"barFrame\" />";
            yield return "    <frame src=\"main.html\" name=\"contentFrame\" />";
            yield return "</frameset>";
        }
        public IEnumerable<String> PageWrapper(IEnumerable<String> Content)
        {
            yield return "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            yield return "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML Basic 1.1//EN\"";
            yield return "    \"http://www.w3.org/TR/xhtml-basic/xhtml-basic11.dtd\">";
            yield return "<!--";
            yield return "==========================================================================";
            yield return "";
            yield return "    Notice:      This file is automatically generated.";
            yield return "                Please don't modify this file.";
            yield return "";
            yield return "==========================================================================";
            yield return "-->";
            yield return "<html xmlns=\"http://www.w3.org/1999/xhtml\">";
            foreach (var _Line in Combine(Combine(Begin(), "    "), Content))
            {
                yield return _Line;
            }
            yield return "</html>";
            yield return "";
        }
        public IEnumerable<String> Css()
        {
            yield return "h1";
            yield return "{";
            yield return "    font-family: 宋体;";
            yield return "    font-size: x-large;";
            yield return "}";
            yield return "";
            yield return "h2";
            yield return "{";
            yield return "    font-family: 宋体;";
            yield return "    font-size: large;";
            yield return "}";
            yield return "";
            yield return "h2";
            yield return "{";
            yield return "    font-family: 宋体;";
            yield return "    font-size: medium;";
            yield return "}";
            yield return "";
            yield return "table, td";
            yield return "{";
            yield return "    border-color: Gray;";
            yield return "    border-style: solid;";
            yield return "}";
            yield return "";
            yield return "table";
            yield return "{";
            yield return "    border-width: 0 0 1pt 1pt;";
            yield return "    border-spacing: 0;";
            yield return "    border-collapse: collapse;";
            yield return "}";
            yield return "";
            yield return "td";
            yield return "{";
            yield return "    border-width: 1pt 1pt 0 0;";
            yield return "    margin: 0;";
            yield return "    padding: 5pt 5pt 5pt 5pt;";
            yield return "}";
            yield return "";
            yield return "a";
            yield return "{";
            yield return "    text-decoration: none;";
            yield return "}";
            yield return "";
            yield return "td, pre";
            yield return "{";
            yield return "    font-size: medium;";
            yield return "}";
            yield return "";
            yield return "pre";
            yield return "{";
            yield return "    margin: 0;";
            yield return "    padding: 5pt 0pt 5pt 0pt;";
            yield return "}";
            yield return "";
        }
    }
}
