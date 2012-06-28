﻿//==========================================================================
//
//  Notice:      This file is automatically generated.
//               Please don't modify this file.
//
//==========================================================================

//Reference:

using System;
using System.Collections.Generic;
using Boolean = System.Boolean;
using String = System.String;
using Type = System.Type;
using Int = System.Int32;
using Real = System.Double;
using Byte = System.Byte;

namespace Database
{
    [Record]
    public struct Unit
    {
    }
    
    public class RecordAttribute : Firefly.Mapping.MetaSchema.RecordAttribute
    {
    }
    
    public class AliasAttribute : Firefly.Mapping.MetaSchema.AliasAttribute
    {
    }
    
    public class TaggedUnionAttribute : Firefly.Mapping.MetaSchema.TaggedUnionAttribute
    {
    }
    
    public class TagAttribute : Firefly.Mapping.MetaSchema.TagAttribute
    {
    }
    
    public class TupleAttribute : Firefly.Mapping.MetaSchema.TupleAttribute
    {
    }
    
    /// <summary>邮件</summary>
    [Record]
    public sealed class Mail
    {
        /// <summary>邮件ID</summary>
        public Int32 Id { get; set; }
        /// <summary>标题</summary>
        public String Title { get; set; }
        /// <summary>发件用户ID</summary>
        public Int32 FromId { get; set; }
        /// <summary>时间(UTC)：yyyy-MM-ddTHH:mm:ssZ形式</summary>
        public String Time { get; set; }
        /// <summary>内容</summary>
        public String Content { get; set; }
    }
    
    /// <summary>邮件收件人</summary>
    [Record]
    public sealed class MailTo
    {
        /// <summary>邮件ID</summary>
        public Int32 Id { get; set; }
        /// <summary>收件用户ID</summary>
        public Int32 ToId { get; set; }
    }
    
    /// <summary>邮件附件</summary>
    [Record]
    public sealed class MailAttachment
    {
        /// <summary>邮件ID</summary>
        public Int32 Id { get; set; }
        /// <summary>名称</summary>
        public String Name { get; set; }
        /// <summary>内容</summary>
        public List<Byte> Content { get; set; }
    }
    
    /// <summary>邮件所有关系</summary>
    [Record]
    public sealed class MailOwner
    {
        /// <summary>邮件ID</summary>
        public Int32 Id { get; set; }
        /// <summary>所有者用户ID</summary>
        public Int32 OwnerId { get; set; }
        /// <summary>是否是新邮件</summary>
        public Boolean IsNew { get; set; }
        /// <summary>时间(UTC)：yyyy-MM-ddTHH:mm:ssZ形式</summary>
        public String Time { get; set; }
    }
    
    /// <summary>测试记录</summary>
    [Record]
    public sealed class TestRecord
    {
        /// <summary>测试Session索引</summary>
        public Int32 SessionIndex { get; set; }
        /// <summary>测试数据</summary>
        public Int32 Value { get; set; }
    }
    
    /// <summary>测试锁记录</summary>
    [Record]
    public sealed class TestLockRecord
    {
        /// <summary>1</summary>
        public Int32 Id { get; set; }
        /// <summary>测试数据</summary>
        public Int32 Value { get; set; }
    }
    
    /// <summary>用户账号信息</summary>
    [Record]
    public sealed class UserProfile
    {
        /// <summary>用户号</summary>
        public Int32 Id { get; set; }
        /// <summary>用户名</summary>
        public String Name { get; set; }
    }
}