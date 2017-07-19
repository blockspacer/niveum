//==========================================================================
//
//  Notice:      This file is automatically generated.
//               Please don't modify this file.
//
//==========================================================================

using System;
using System.Collections.Generic;
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

namespace Nivea.Template.Semantics
{
    /// <summary>根结点</summary>
    [Record]
    public sealed class File
    {
        /// <summary>过滤器</summary>
        public List<FilterDef> Filters;
        /// <summary>节列表</summary>
        public List<SectionDef> Sections;
    }
    /// <summary>过滤器定义</summary>
    [Record]
    public sealed class FilterDef
    {
        /// <summary>名称</summary>
        public String Name;
        /// <summary>参数</summary>
        public String Parameter;
    }
    public enum SectionDefTag
    {
        /// <summary>命名空间</summary>
        Namespace = 0,
        /// <summary>程序集引用</summary>
        Assembly = 1,
        /// <summary>命名空间和类空间导入</summary>
        Import = 2,
        /// <summary>类型定义</summary>
        Type = 3,
        /// <summary>常量值</summary>
        Constant = 4,
        /// <summary>模板定义</summary>
        Template = 5
    }
    /// <summary>节定义</summary>
    [TaggedUnion]
    public sealed class SectionDef
    {
        [Tag] public SectionDefTag _Tag;

        /// <summary>命名空间</summary>
        public List<String> Namespace;
        /// <summary>程序集引用</summary>
        public List<String> Assembly;
        /// <summary>命名空间和类空间导入</summary>
        public List<List<String>> Import;
        /// <summary>类型定义</summary>
        public TypeDef Type;
        /// <summary>常量值</summary>
        public ConstantValue Constant;
        /// <summary>模板定义</summary>
        public TemplateDef Template;

        /// <summary>命名空间</summary>
        public static SectionDef CreateNamespace(List<String> Value) { return new SectionDef { _Tag = SectionDefTag.Namespace, Namespace = Value }; }
        /// <summary>程序集引用</summary>
        public static SectionDef CreateAssembly(List<String> Value) { return new SectionDef { _Tag = SectionDefTag.Assembly, Assembly = Value }; }
        /// <summary>命名空间和类空间导入</summary>
        public static SectionDef CreateImport(List<List<String>> Value) { return new SectionDef { _Tag = SectionDefTag.Import, Import = Value }; }
        /// <summary>类型定义</summary>
        public static SectionDef CreateType(TypeDef Value) { return new SectionDef { _Tag = SectionDefTag.Type, Type = Value }; }
        /// <summary>常量值</summary>
        public static SectionDef CreateConstant(ConstantValue Value) { return new SectionDef { _Tag = SectionDefTag.Constant, Constant = Value }; }
        /// <summary>模板定义</summary>
        public static SectionDef CreateTemplate(TemplateDef Value) { return new SectionDef { _Tag = SectionDefTag.Template, Template = Value }; }

        /// <summary>命名空间</summary>
        public Boolean OnNamespace { get { return _Tag == SectionDefTag.Namespace; } }
        /// <summary>程序集引用</summary>
        public Boolean OnAssembly { get { return _Tag == SectionDefTag.Assembly; } }
        /// <summary>命名空间和类空间导入</summary>
        public Boolean OnImport { get { return _Tag == SectionDefTag.Import; } }
        /// <summary>类型定义</summary>
        public Boolean OnType { get { return _Tag == SectionDefTag.Type; } }
        /// <summary>常量值</summary>
        public Boolean OnConstant { get { return _Tag == SectionDefTag.Constant; } }
        /// <summary>模板定义</summary>
        public Boolean OnTemplate { get { return _Tag == SectionDefTag.Template; } }
    }
    /// <summary>常量值</summary>
    [Record]
    public sealed class ConstantValue
    {
        /// <summary>名称</summary>
        public String Name;
        /// <summary>类型</summary>
        public TypeSpec Type;
        /// <summary>值</summary>
        public Expr Value;
    }
    /// <summary>模板定义</summary>
    [Record]
    public sealed class TemplateDef
    {
        /// <summary>签名</summary>
        public TemplateSignature Signature;
        /// <summary>主体</summary>
        public List<TemplateExpr> Body;
    }
    /// <summary>模板签名</summary>
    [Record]
    public sealed class TemplateSignature
    {
        /// <summary>名称</summary>
        public String Name;
        /// <summary>参数</summary>
        public List<VariableDef> Parameters;
    }
}
