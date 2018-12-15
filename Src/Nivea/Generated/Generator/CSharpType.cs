//==========================================================================
//
//  Notice:      This file is automatically generated.
//               Please don't modify this file.
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Nivea.Template.Semantics;
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

namespace Nivea.Generator.CSharpType
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
        private IEnumerable<String> GetEscapedIdentifier(IEnumerable<String> IdentifierValues)
        {
            foreach (var Identifier in IdentifierValues)
            {
                yield return GetEscapedIdentifier(Identifier);
            }
        }
        public IEnumerable<String> SingleLineXmlComment(String Description)
        {
            foreach (var _Line in Combine(Combine(Combine(Begin(), "/// <summary>"), Description), "</summary>"))
            {
                yield return _Line;
            }
        }
        public IEnumerable<String> MultiLineXmlComment(List<String> Description)
        {
            yield return "/// <summary>";
            foreach (var _Line in Combine(Combine(Begin(), "/// "), Description))
            {
                yield return _Line;
            }
            yield return "/// </summary>";
        }
        public IEnumerable<String> Primitive(String Name, String PlatformName)
        {
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "using "), GetEscapedIdentifier(Name)), " = "), PlatformName), ";"))
            {
                yield return _Line;
            }
        }
        public IEnumerable<String> Primitive_Unit()
        {
            yield return "public class AliasAttribute : Attribute {}";
            yield return "public class RecordAttribute : Attribute {}";
            yield return "public class TaggedUnionAttribute : Attribute {}";
            yield return "public class TagAttribute : Attribute {}";
            yield return "public class TupleAttribute : Attribute {}";
            yield return "";
            yield return "[Record]";
            yield return "public struct Unit {}";
        }
        public IEnumerable<String> Primitive_Optional()
        {
            yield return "public enum OptionalTag";
            yield return "{";
            yield return "    NotHasValue = 0,";
            yield return "    HasValue = 1";
            yield return "}";
            yield return "[TaggedUnion]";
            yield return "public struct Optional<T>";
            yield return "{";
            yield return "    [Tag] public OptionalTag _Tag;";
            yield return "";
            yield return "    public Unit NotHasValue;";
            yield return "    public T HasValue;";
            yield return "";
            yield return "    public static Optional<T> CreateNotHasValue() { return new Optional<T> { _Tag = OptionalTag.NotHasValue, NotHasValue = new Unit() }; }";
            yield return "    public static Optional<T> CreateHasValue(T Value) { return new Optional<T> { _Tag = OptionalTag.HasValue, HasValue = Value }; }";
            yield return "";
            yield return "    public Boolean OnNotHasValue { get { return _Tag == OptionalTag.NotHasValue; } }";
            yield return "    public Boolean OnHasValue { get { return _Tag == OptionalTag.HasValue; } }";
            yield return "";
            yield return "    public static Optional<T> Empty { get { return CreateNotHasValue(); } }";
            yield return "    public static implicit operator Optional<T>(T v)";
            yield return "    {";
            yield return "        if (v == null)";
            yield return "        {";
            yield return "            return CreateNotHasValue();";
            yield return "        }";
            yield return "        else";
            yield return "        {";
            yield return "            return CreateHasValue(v);";
            yield return "        }";
            yield return "    }";
            yield return "    public static explicit operator T(Optional<T> v)";
            yield return "    {";
            yield return "        if (v.OnNotHasValue)";
            yield return "        {";
            yield return "            throw new InvalidOperationException();";
            yield return "        }";
            yield return "        return v.HasValue;";
            yield return "    }";
            yield return "    public static Boolean operator ==(Optional<T> Left, Optional<T> Right)";
            yield return "    {";
            yield return "        return Equals(Left, Right);";
            yield return "    }";
            yield return "    public static Boolean operator !=(Optional<T> Left, Optional<T> Right)";
            yield return "    {";
            yield return "        return !Equals(Left, Right);";
            yield return "    }";
            yield return "    public static Boolean operator ==(Optional<T>? Left, Optional<T>? Right)";
            yield return "    {";
            yield return "        return Equals(Left, Right);";
            yield return "    }";
            yield return "    public static Boolean operator !=(Optional<T>? Left, Optional<T>? Right)";
            yield return "    {";
            yield return "        return !Equals(Left, Right);";
            yield return "    }";
            yield return "    public override Boolean Equals(Object obj)";
            yield return "    {";
            yield return "        if (obj == null) { return Equals(this, null); }";
            yield return "        if (obj.GetType() != typeof(Optional<T>)) { return false; }";
            yield return "        var o = (Optional<T>)(obj);";
            yield return "        return Equals(this, o);";
            yield return "    }";
            yield return "    public override Int32 GetHashCode()";
            yield return "    {";
            yield return "        if (OnNotHasValue) { return 0; }";
            yield return "        return HasValue.GetHashCode();";
            yield return "    }";
            yield return "";
            yield return "    private static Boolean Equals(Optional<T> Left, Optional<T> Right)";
            yield return "    {";
            yield return "        if (Left.OnNotHasValue && Right.OnNotHasValue)";
            yield return "        {";
            yield return "            return true;";
            yield return "        }";
            yield return "        if (Left.OnNotHasValue || Right.OnNotHasValue)";
            yield return "        {";
            yield return "            return false;";
            yield return "        }";
            yield return "        return Left.HasValue.Equals(Right.HasValue);";
            yield return "    }";
            yield return "    private static Boolean Equals(Optional<T>? Left, Optional<T>? Right)";
            yield return "    {";
            yield return "        if ((!Left.HasValue || Left.Value.OnNotHasValue) && (!Right.HasValue || Right.Value.OnNotHasValue))";
            yield return "        {";
            yield return "            return true;";
            yield return "        }";
            yield return "        if (!Left.HasValue || Left.Value.OnNotHasValue || !Right.HasValue || Right.Value.OnNotHasValue)";
            yield return "        {";
            yield return "            return false;";
            yield return "        }";
            yield return "        return Equals(Left.Value, Right.Value);";
            yield return "    }";
            yield return "";
            yield return "    public T Value";
            yield return "    {";
            yield return "        get";
            yield return "        {";
            yield return "            if (OnHasValue)";
            yield return "            {";
            yield return "                return HasValue;";
            yield return "            }";
            yield return "            else";
            yield return "            {";
            yield return "                throw new InvalidOperationException();";
            yield return "            }";
            yield return "        }";
            yield return "    }";
            yield return "    public T ValueOrDefault(T Default)";
            yield return "    {";
            yield return "        if (OnHasValue)";
            yield return "        {";
            yield return "            return HasValue;";
            yield return "        }";
            yield return "        else";
            yield return "        {";
            yield return "            return Default;";
            yield return "        }";
            yield return "    }";
            yield return "";
            yield return "    public override String ToString()";
            yield return "    {";
            yield return "        if (OnHasValue)";
            yield return "        {";
            yield return "            return HasValue.ToString();";
            yield return "        }";
            yield return "        else";
            yield return "        {";
            yield return "            return \"-\";";
            yield return "        }";
            yield return "    }";
            yield return "}";
        }
        public IEnumerable<String> Alias(AliasDef a)
        {
            var Name = GetEscapedIdentifier(a.TypeFriendlyName()) + GetGenericParameters(a.GenericParameters);
            var Type = GetTypeString(a.Type);
            foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
            {
                yield return _Line;
            }
            yield return "[Alias]";
            foreach (var _Line in Combine(Combine(Begin(), "public sealed class "), Name))
            {
                yield return _Line;
            }
            yield return "{";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    public "), Type), " Value;"))
            {
                yield return _Line;
            }
            yield return "";
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "    public static implicit operator "), Name), "("), Type), " o)"))
            {
                yield return _Line;
            }
            yield return "    {";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "        return new "), Name), " {Value = o};"))
            {
                yield return _Line;
            }
            yield return "    }";
            foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "    public static implicit operator "), Type), "("), Name), " c)"))
            {
                yield return _Line;
            }
            yield return "    {";
            yield return "        return c.Value;";
            yield return "    }";
            yield return "}";
        }
        public IEnumerable<String> Record(RecordDef r)
        {
            var Name = GetEscapedIdentifier(r.TypeFriendlyName()) + GetGenericParameters(r.GenericParameters);
            foreach (var _Line in Combine(Begin(), GetXmlComment(r.Description)))
            {
                yield return _Line;
            }
            yield return "[Record]";
            foreach (var _Line in Combine(Combine(Begin(), "public sealed class "), Name))
            {
                yield return _Line;
            }
            yield return "{";
            foreach (var f in r.Fields)
            {
                foreach (var _Line in Combine(Begin(), GetXmlComment(f.Description)))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "public "), GetTypeString(f.Type)), " "), GetEscapedIdentifier(f.Name)), ";"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
            }
            yield return "}";
        }
        public IEnumerable<String> TaggedUnion(TaggedUnionDef tu)
        {
            var Name = GetEscapedIdentifier(tu.TypeFriendlyName()) + GetGenericParameters(tu.GenericParameters);
            var TagName = GetEscapedIdentifier(tu.TypeFriendlyName() + "Tag");
            foreach (var _Line in Combine(Combine(Begin(), "public enum "), TagName))
            {
                yield return _Line;
            }
            yield return "{";
            var k = 0;
            foreach (var a in tu.Alternatives)
            {
                if (k == tu.Alternatives.Count - 1)
                {
                    foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                    foreach (var _Line in Combine(Combine(Combine(Begin(), GetEscapedIdentifier(a.Name)), " = "), k))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
                else
                {
                    foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                    foreach (var _Line in Combine(Combine(Combine(Combine(Begin(), GetEscapedIdentifier(a.Name)), " = "), k), ","))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
                k += 1;
            }
            yield return "}";
            foreach (var _Line in Combine(Begin(), GetXmlComment(tu.Description)))
            {
                yield return _Line;
            }
            yield return "[TaggedUnion]";
            foreach (var _Line in Combine(Combine(Begin(), "public sealed class "), Name))
            {
                yield return _Line;
            }
            yield return "{";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    [Tag] public "), TagName), " _Tag;"))
            {
                yield return _Line;
            }
            yield return "";
            foreach (var a in tu.Alternatives)
            {
                foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "public "), GetTypeString(a.Type)), " "), GetEscapedIdentifier(a.Name)), ";"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
            }
            yield return "";
            foreach (var a in tu.Alternatives)
            {
                if (a.Type.OnTypeRef && (a.Type.TypeRef.Name.Count == 1) && (a.Type.TypeRef.Name.Single() == "Unit") && (a.Type.TypeRef.Version == ""))
                {
                    foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                    foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "public static "), Name), " "), GetEscapedIdentifier(Combine(Combine(Begin(), "Create"), a.Name))), "() { return new "), Name), " { _Tag = "), TagName), "."), GetEscapedIdentifier(a.Name)), ", "), GetEscapedIdentifier(a.Name)), " = default(Unit) }; }"))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
                else
                {
                    foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                    foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "public static "), Name), " "), GetEscapedIdentifier(Combine(Combine(Begin(), "Create"), a.Name))), "("), GetTypeString(a.Type)), " Value) { return new "), Name), " { _Tag = "), TagName), "."), GetEscapedIdentifier(a.Name)), ", "), GetEscapedIdentifier(a.Name)), " = Value }; }"))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
            }
            yield return "";
            foreach (var a in tu.Alternatives)
            {
                foreach (var _Line in Combine(Begin(), GetXmlComment(a.Description)))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "public Boolean "), GetEscapedIdentifier(Combine(Combine(Begin(), "On"), a.Name))), " { get { return _Tag == "), TagName), "."), GetEscapedIdentifier(a.Name)), "; } }"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
            }
            yield return "}";
        }
        public IEnumerable<String> Enum(EnumDef e)
        {
            var Name = GetEscapedIdentifier(e.TypeFriendlyName());
            var ParserName = GetEscapedIdentifier(e.TypeFriendlyName() + "Parser");
            var WriterName = GetEscapedIdentifier(e.TypeFriendlyName() + "Writer");
            foreach (var _Line in Combine(Begin(), GetXmlComment(e.Description)))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Combine(Combine(Combine(Begin(), "public enum "), Name), " : "), GetEnumTypeString(e.UnderlyingType)))
            {
                yield return _Line;
            }
            yield return "{";
            var k = 0;
            foreach (var l in e.Literals)
            {
                if (k == e.Literals.Count - 1)
                {
                    foreach (var _Line in Combine(Begin(), GetXmlComment(l.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                    foreach (var _Line in Combine(Combine(Combine(Begin(), GetEscapedIdentifier(l.Name)), " = "), l.Value))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
                else
                {
                    foreach (var _Line in Combine(Begin(), GetXmlComment(l.Description)))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                    foreach (var _Line in Combine(Combine(Combine(Combine(Begin(), GetEscapedIdentifier(l.Name)), " = "), l.Value), ","))
                    {
                        yield return _Line == "" ? "" : "    " + _Line;
                    }
                }
                k += 1;
            }
            yield return "}";
        }
    }
}
