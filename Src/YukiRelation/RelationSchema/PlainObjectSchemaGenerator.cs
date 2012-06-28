﻿//==========================================================================
//
//  File:        PlainObjectSchemaGenerator.cs
//  Location:    Yuki.Relation <Visual C#>
//  Description: 简单对象类型结构生成器
//  Version:     2012.06.26.
//  Copyright(C) F.R.C.
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OS = Yuki.ObjectSchema;
using RS = Yuki.RelationSchema;

namespace Yuki.RelationSchema
{
    public static class PlainObjectSchemaGenerator
    {
        public static OS.Schema Generate(RS.Schema Schema)
        {
            var s = (new Generator { Schema = Schema }).Generate();
            return s;
        }

        private class Generator
        {
            public RS.Schema Schema;

            public OS.Schema Generate()
            {
                var TypeRefs = Schema.TypeRefs.Select(t => TranslateTypeDef(t)).ToArray();
                var Types = Schema.Types.Select(t => TranslateTypeDef(t)).ToList();
                if (ByteUsed && !Types.Concat(TypeRefs).Where(t => t.OnPrimitive && t.Primitive.Name.Equals("Byte", StringComparison.OrdinalIgnoreCase)).Any())
                {
                    Types.Add(OS.TypeDef.CreatePrimitive(new OS.PrimitiveDef { Name = "Byte", GenericParameters = new OS.VariableDef[] { }, Description = "" }));
                }
                if (ListUsed && !Types.Concat(TypeRefs).Where(t => t.OnPrimitive && t.Primitive.Name.Equals("List", StringComparison.OrdinalIgnoreCase)).Any())
                {
                    var GenericParameter = new OS.VariableDef { Name = "T", Type = OS.TypeSpec.CreateTypeRef(new OS.TypeRef { Name = "Type", Version = "" }), Description = "" };
                    Types.Add(OS.TypeDef.CreatePrimitive(new OS.PrimitiveDef { Name = "List", GenericParameters = new OS.VariableDef[] { GenericParameter }, Description = "" }));
                }
                if (TypeUsed && !Types.Concat(TypeRefs).Where(t => t.OnPrimitive && t.Primitive.Name.Equals("Type", StringComparison.OrdinalIgnoreCase)).Any())
                {
                    Types.Add(OS.TypeDef.CreatePrimitive(new OS.PrimitiveDef { Name = "Type", GenericParameters = new OS.VariableDef[] { }, Description = "" }));
                }
                return new OS.Schema { Types = Types.ToArray(), TypeRefs = TypeRefs, Imports = Schema.Imports.ToArray(), TypePaths = new OS.TypePath[] { } };
            }

            private OS.TypeDef TranslateTypeDef(RS.TypeDef t)
            {
                if (t.OnPrimitive)
                {
                    return OS.TypeDef.CreatePrimitive(TranslatePrimitive(t.Primitive));
                }
                else if (t.OnRecord)
                {
                    return OS.TypeDef.CreateRecord(TranslateRecord(t.Record));
                }
                else if (t.OnEnum)
                {
                    return OS.TypeDef.CreateEnum(TranslateEnum(t.Enum));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            private OS.PrimitiveDef TranslatePrimitive(RS.PrimitiveDef e)
            {
                return new OS.PrimitiveDef { Name = e.Name, GenericParameters = new OS.VariableDef[] { }, Description = e.Description };
            }

            private OS.LiteralDef TranslateLiteral(RS.LiteralDef l)
            {
                return new OS.LiteralDef { Name = l.Name, Value = l.Value, Description = l.Description };
            }
            private OS.EnumDef TranslateEnum(RS.EnumDef e)
            {
                return new OS.EnumDef { Name = e.Name, Version = "", UnderlyingType = TranslateTypeSpec(e.UnderlyingType), Literals = e.Literals.Select(l => TranslateLiteral(l)).ToArray(), Description = e.Description };
            }

            private Boolean ByteUsed = false;
            private Boolean ListUsed = false;
            private Boolean TypeUsed = false;
            private OS.TypeSpec TranslateTypeSpec(RS.TypeSpec t)
            {
                if (t.OnTypeRef)
                {
                    if (t.TypeRef.Value.Equals("Binary", StringComparison.OrdinalIgnoreCase))
                    {
                        ByteUsed = true;
                        ListUsed = true;
                        TypeUsed = true;
                        var tBinary = OS.TypeSpec.CreateTypeRef(new OS.TypeRef { Name = "List", Version = "" });
                        var tByte = OS.TypeSpec.CreateTypeRef(new OS.TypeRef { Name = "Byte", Version = "" });
                        return OS.TypeSpec.CreateGenericTypeSpec(new OS.GenericTypeSpec { TypeSpec = tBinary, GenericParameterValues = new OS.GenericParameterValue[] { OS.GenericParameterValue.CreateTypeSpec(tByte) } });
                    }
                    else
                    {
                        return OS.TypeSpec.CreateTypeRef(new OS.TypeRef { Name = t.TypeRef.Value, Version = "" });
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            private OS.VariableDef TranslateField(RS.VariableDef f)
            {
                return new OS.VariableDef { Name = f.Name, Type = TranslateTypeSpec(f.Type), Description = f.Description };
            }
            private OS.RecordDef TranslateRecord(RS.RecordDef r)
            {
                var Fields = r.Fields.Where(f => f.Attribute.OnColumn).Select(f => TranslateField(f)).ToArray();
                return new OS.RecordDef { Name = r.Name, Version = "", GenericParameters = new OS.VariableDef[] { }, Fields = Fields, Description = r.Description };
            }
        }
    }
}