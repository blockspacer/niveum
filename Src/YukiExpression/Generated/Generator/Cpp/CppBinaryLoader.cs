//==========================================================================
//
//  Notice:      This file is automatically generated.
//               Please don't modify this file.
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Firefly;
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

namespace Yuki.ExpressionSchema.CppBinaryLoader
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
        public IEnumerable<String> Assembly(Schema Schema)
        {
            var Hash = Schema.Hash().ToString("X16", System.Globalization.CultureInfo.InvariantCulture);
            yield return "class Calculation";
            yield return "{";
            yield return "public:";
            foreach (var m in Schema.Modules)
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "std::shared_ptr<class "), GetEscapedIdentifier(m.Name)), "> "), GetEscapedIdentifier(m.Name)), ";"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
            }
            yield return "";
            yield return "    Calculation(std::shared_ptr<Yuki::ExpressionSchema::Assembly> a)";
            yield return "    {";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "        if (a->Hash != 0x"), Hash), ") { throw std::logic_error(\"InvalidOperation\"); }"))
            {
                yield return _Line;
            }
            yield return "        std::unordered_map<std::wstring, std::shared_ptr<Yuki::ExpressionSchema::ModuleDef>> _d_;";
            yield return "        for (auto m : a->Modules)";
            yield return "        {";
            yield return "            _d_[m->Name] = m;";
            yield return "        }";
            foreach (var m in Schema.Modules)
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Combine(Begin(), "this->"), GetEscapedIdentifier(m.Name)), " = std::make_shared<class "), GetEscapedIdentifier(m.Name)), ">(_d_["), GetEscapedStringLiteral(m.Name)), "]);"))
                {
                    yield return _Line == "" ? "" : "        " + _Line;
                }
            }
            yield return "    }";
            yield return "};";
        }
        public IEnumerable<String> TypePredefinition(String Name)
        {
            foreach (var _Line in Combine(Combine(Combine(Begin(), "class "), GetEscapedIdentifier(Name)), ";"))
            {
                yield return _Line;
            }
        }
        public IEnumerable<String> Module(ModuleDecl m)
        {
            foreach (var _Line in Combine(Combine(Begin(), "class "), GetEscapedIdentifier(m.Name)))
            {
                yield return _Line;
            }
            yield return "{";
            yield return "private:";
            foreach (var f in m.Functions)
            {
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "std::function<"), f.ReturnValue.ToString()), "(Yuki::Expression::ExpressionParameterContext &)> "), GetEscapedIdentifier(Combine(Combine(Begin(), "Func_"), f.Name))), ";"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
            }
            yield return "";
            yield return "public:";
            foreach (var _Line in Combine(Combine(Combine(Begin(), "    "), GetEscapedIdentifier(m.Name)), "(std::shared_ptr<Yuki::ExpressionSchema::ModuleDef> md)"))
            {
                yield return _Line;
            }
            yield return "    {";
            yield return "        std::unordered_map<std::wstring, std::shared_ptr<Yuki::ExpressionSchema::Expr>> fd;";
            yield return "        for (auto _f_ : md->Functions)";
            yield return "        {";
            yield return "            fd[_f_->Name] = _f_->Body;";
            yield return "        }";
            foreach (var f in m.Functions)
            {
                yield return "        " + "{";
                foreach (var _Line in Combine(Combine(Combine(Begin(), "    auto Body = fd["), GetEscapedStringLiteral(f.Name)), "];"))
                {
                    yield return _Line == "" ? "" : "        " + _Line;
                }
                yield return "        " + "";
                yield return "        " + "    Yuki::Expression::ExpressionParameterTypeProvider eptp;";
                foreach (var p in f.Parameters)
                {
                    foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "eptp.Parameters["), GetEscapedStringLiteral(p.Name)), "] = Yuki::ExpressionSchema::PrimitiveType::"), p.Type.ToString()), ";"))
                    {
                        yield return _Line == "" ? "" : "            " + _Line;
                    }
                }
                yield return "        " + "    Yuki::Expression::ExpressionCalculator ec;";
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "    "), GetEscapedIdentifier(Combine(Combine(Begin(), "Func_"), f.Name))), " = ec.BuildExpression<"), f.ReturnValue.ToString()), ">(eptp, Body);"))
                {
                    yield return _Line == "" ? "" : "        " + _Line;
                }
                yield return "        " + "}";
            }
            yield return "    }";
            yield return "";
            foreach (var f in m.Functions)
            {
                var ParameterList = String.Join(", ", f.Parameters.Select(p => p.Type.ToString() + " " + GetEscapedIdentifier(p.Name)));
                foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Combine(Begin(), f.ReturnValue.ToString()), " "), GetEscapedIdentifier(f.Name)), "("), ParameterList), ")"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
                yield return "    " + "{";
                yield return "    " + "    Yuki::Expression::ExpressionParameterContext epc;";
                foreach (var p in f.Parameters)
                {
                    foreach (var _Line in Combine(Combine(Combine(Combine(Combine(Begin(), "epc.Parameters["), GetEscapedStringLiteral(p.Name)), "] = "), GetEscapedIdentifier(p.Name)), ";"))
                    {
                        yield return _Line == "" ? "" : "        " + _Line;
                    }
                }
                foreach (var _Line in Combine(Combine(Combine(Begin(), "    return "), GetEscapedIdentifier(Combine(Combine(Begin(), "Func_"), f.Name))), "(epc);"))
                {
                    yield return _Line == "" ? "" : "    " + _Line;
                }
                yield return "    " + "}";
            }
            yield return "};";
        }
        public IEnumerable<String> Main(Schema Schema, String NamespaceName)
        {
            yield return "//==========================================================================";
            yield return "//";
            yield return "//  Notice:      This file is automatically generated.";
            yield return "//               Please don't modify this file.";
            yield return "//";
            yield return "//==========================================================================";
            yield return "";
            yield return "#pragma once";
            yield return "";
            yield return "#include <cstdint>";
            yield return "#include <memory>";
            yield return "#include <string>";
            yield return "#include <functional>";
            yield return "#include <stdexcept>";
            yield return "#include <unordered_map>";
            foreach (var _Line in Combine(Combine(Begin(), "#include "), Schema.Imports.Where(i => IsInclude(i))))
            {
                yield return _Line;
            }
            yield return "typedef std::int32_t Int;";
            yield return "typedef double Real;";
            yield return "";
            var SimpleTypes = GetSimpleTypes(Schema);
            var ComplexTypes = GetComplexTypes(Schema);
            foreach (var _Line in Combine(Begin(), WrapContents(NamespaceName, SimpleTypes)))
            {
                yield return _Line;
            }
            foreach (var _Line in Combine(Begin(), WrapContents(NamespaceName, ComplexTypes)))
            {
                yield return _Line;
            }
            yield return "";
        }
    }
}