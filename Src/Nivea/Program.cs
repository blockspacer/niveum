﻿//==========================================================================
//
//  File:        Program.cs
//  Location:    Nivea <Visual C#>
//  Description: 模板语言运行时
//  Version:     2016.05.19.
//  Copyright(C) F.R.C.
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Firefly;
using Firefly.Mapping.XmlText;
using Firefly.Streaming;
using Firefly.TextEncoding;
using Firefly.Texting;
using Firefly.Texting.TreeFormat;

namespace Yuki.SchemaManipulator
{
    public static class Program
    {
        public static int Main()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return MainInner();
            }
            else
            {
                try
                {
                    return MainInner();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex));
                    return -1;
                }
            }
        }

        public static int MainInner()
        {
            TextEncoding.WritingDefault = TextEncoding.UTF8;

            var CmdLine = CommandLine.GetCmdLine();
            var argv = CmdLine.Arguments;

            if (CmdLine.Arguments.Length != 0)
            {
                DisplayInfo();
                return -1;
            }

            if (CmdLine.Options.Length == 0)
            {
                DisplayInfo();
                return 0;
            }

            var xs = new XmlSerializer();
            foreach (var opt in CmdLine.Options)
            {
                var optNameLower = opt.Name.ToLower();
                if ((optNameLower == "?") || (optNameLower == "help"))
                {
                    DisplayInfo();
                    return 0;
                }
                else
                {
                    throw new ArgumentException(opt.Name);
                }
            }
            return 0;
        }

        public static void DisplayInfo()
        {
            Console.WriteLine(@"模板语言运行时");
            Console.WriteLine(@"Nivea，按BSD许可证分发");
            Console.WriteLine(@"F.R.C.");
            Console.WriteLine(@"");
            Console.WriteLine(@"本工具用于从类型结构和类型模板生成代码。");
            Console.WriteLine(@"");
            Console.WriteLine(@"用法:");
            Console.WriteLine(@"");
            Console.WriteLine(@"示例:");
            Console.WriteLine(@"Nivea /schemakind:ObjectSchema /template:CSharp /loadtype:Schema /p:Namespace=Communication,Output=Src\Generated\Communication.cs");
        }
    }
}