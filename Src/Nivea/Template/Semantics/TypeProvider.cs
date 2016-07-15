﻿//==========================================================================
//
//  File:        TypeProvider.cs
//  Location:    Nivea <Visual C#>
//  Description: 类型提供器
//  Version:     2016.06.04.
//  Copyright(C) F.R.C.
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Nivea.Template.Semantics
{
    public class TypeProvider
    {
        private Dictionary<String, String> PrimitiveMapping = new Dictionary<String, String> { { "Unit", "Yuki.Unit" }, { "Boolean", "System.Boolean" }, { "String", "System.String" }, { "Int", "System.Int32" }, { "Real", "System.Double" }, { "Byte", "System.Byte" }, { "UInt8", "System.Byte" }, { "UInt16", "System.UInt16" }, { "UInt32", "System.UInt32" }, { "UInt64", "System.UInt64" }, { "Int8", "System.SByte" }, { "Int16", "System.Int16" }, { "Int32", "System.Int32" }, { "Int64", "System.Int64" }, { "Float32", "System.Single" }, { "Float64", "System.Double" }, { "Type", "System.Type" }, { "Optional", "Yuki.Optional" }, { "List", "System.Collections.Generic.List`1" }, { "Set", "System.Collections.Generic.HashSet`1" }, { "Map", "System.Collections.Generic.Dictionary`2" } };
        private HashSet<String> EnabledPrimitives = new HashSet<String>();
        private Dictionary<String, List<TypeDef>> TypeDefDict = new Dictionary<String, List<TypeDef>>();
        private Dictionary<String, List<TypeDefinition>> TypeDefinitionDict = new Dictionary<String, List<TypeDefinition>>();
        private Dictionary<String, AssemblyDefinition> Assemblies = new Dictionary<String, AssemblyDefinition>();

        public TypeProvider()
        {
        }

        public List<TypeDef> GetTypeDefs(List<String> NamespaceParts, String Name, String Version, int GenericParameterCount)
        {
            var FullName = ((NamespaceParts.Count == 0) ? "" : String.Join(".", NamespaceParts.Select(Part => GetCSharpFriendlyName(Part))) + ".") + GetCSharpFriendlyName(Name, Version, GenericParameterCount);
            if (TypeDefDict.ContainsKey(FullName))
            {
                return TypeDefDict[FullName];
            }
            else
            {
                if (TypeDefinitionDict.ContainsKey(FullName))
                {
                    //TODO 提取为TypeDef
                }
                return new List<TypeDef> { };
            }
        }

        public List<TypeDefinition> GetTypeDefinitions(List<String> NamespaceParts, String Name, String Version, int GenericParameterCount)
        {
            if ((NamespaceParts.Count == 0) && (Version == ""))
            {
                if (EnabledPrimitives.Contains(Name))
                {
                    if (PrimitiveMapping.ContainsKey(Name))
                    {
                        var v = PrimitiveMapping[Name];
                        if (TypeDefinitionDict.ContainsKey(v))
                        {
                            return TypeDefinitionDict[v];
                        }
                    }
                    return new List<TypeDefinition> { };
                }
            }

            var FullName = ((NamespaceParts.Count == 0) ? "" : String.Join(".", NamespaceParts.Select(Part => GetCSharpFriendlyName(Part))) + ".") + GetCSharpFriendlyName(Name, Version, GenericParameterCount);
            if (TypeDefinitionDict.ContainsKey(FullName))
            {
                return TypeDefinitionDict[FullName];
            }
            else
            {
                return new List<TypeDefinition> { };
            }
        }

        public Optional<TypeDefinition> GetNestTypeDefinition(TypeDefinition t, String Name, String Version, int GenericParameterCount)
        {
            var FullName = GetCSharpFriendlyName(Name, Version, GenericParameterCount);
            var l = t.NestedTypes.Where(nt => nt.Name == FullName).ToList();
            if (l.Count == 1) { return l.Single(); }
            return Optional<TypeDefinition>.Empty;
        }

        public void AddTypeDef(List<String> NamespaceParts, TypeDef Def)
        {
            if (Def.OnPrimitive)
            {
                if (!EnabledPrimitives.Contains(Def.Primitive.Name))
                {
                    EnabledPrimitives.Add(Def.Primitive.Name);
                }
                return;
            }

            var FullName = ((NamespaceParts.Count == 0) ? "" : String.Join(".", NamespaceParts.Select(Part => GetCSharpFriendlyName(Part))) + ".") + GetCSharpFriendlyName(Def);
            if (TypeDefDict.ContainsKey(FullName))
            {
                TypeDefDict[FullName].Add(Def);
            }
            else
            {
                TypeDefDict.Add(FullName, new List<TypeDef> { Def });
            }
        }

        public void AddTypeDefinition(String FullName, TypeDefinition Def)
        {
            if (TypeDefinitionDict.ContainsKey(FullName))
            {
                TypeDefinitionDict[FullName].Add(Def);
            }
            else
            {
                TypeDefinitionDict.Add(FullName, new List<TypeDefinition> { Def });
            }
        }

        public void LoadAssembly(String Name)
        {
            if (Assemblies.ContainsKey(Name)) { return; }
            var AssemblyPath = Name + ".dll";
            if (!System.IO.File.Exists(AssemblyPath) && (System.IO.Path.GetFileName(AssemblyPath) == AssemblyPath))
            {
                var SystemPath = System.IO.Path.GetDirectoryName(typeof(Object).Assembly.Location);
                var p = System.IO.Path.Combine(SystemPath, System.IO.Path.GetFileName(AssemblyPath));
                if (System.IO.File.Exists(p))
                {
                    AssemblyPath = p;
                }
            }

            var a = AssemblyDefinition.ReadAssembly(AssemblyPath);
            Assemblies.Add(Name, a);
            foreach (var t in a.Modules.SelectMany(m => m.Types))
            {
                AddTypeDefinition(t.FullName, t);
            }
        }

        private static String GetCSharpFriendlyName(TypeDef t)
        {
            switch (t._Tag)
            {
                case TypeDefTag.Primitive:
                    return GetCSharpFriendlyName(t.Primitive.Name, "", t.Primitive.GenericParameters.Count);
                case TypeDefTag.Alias:
                    return GetCSharpFriendlyName(t.Alias.Name, t.Alias.Version, t.Alias.GenericParameters.Count);
                case TypeDefTag.Record:
                    return GetCSharpFriendlyName(t.Record.Name, t.Record.Version, t.Record.GenericParameters.Count);
                case TypeDefTag.TaggedUnion:
                    return GetCSharpFriendlyName(t.TaggedUnion.Name, t.TaggedUnion.Version, t.TaggedUnion.GenericParameters.Count);
                case TypeDefTag.Enum:
                    return GetCSharpFriendlyName(t.Enum.Name, t.Enum.Version, 0);
                default:
                    throw new InvalidOperationException();
            }
        }
        private static String GetCSharpFriendlyName(TypeRef t, int GenericParameterCount)
        {
            return GetCSharpFriendlyName(t.Name, t.Version, GenericParameterCount);
        }
        private static String GetCSharpFriendlyName(String Name, String Version, int GenericParameterCount)
        {
            var s = GetCSharpFriendlyName(Name) + (Version != "" ? "At" + GetCSharpFriendlyName(Version) : "") + (GenericParameterCount != 0 ? "`" + GenericParameterCount.ToString() : "");
            return s;
        }
        private static String GetCSharpFriendlyName(String Name)
        {
            var s = new List<Char>();
            foreach (var c in Name)
            {
                if (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z')) || (c == '_') || (c >= 128))
                {
                    s.Add(c);
                }
                else
                {
                    s.AddRange("_" + ((int)c).ToString("X2"));
                }
            }
            return new String(s.ToArray());
        }
    }
}