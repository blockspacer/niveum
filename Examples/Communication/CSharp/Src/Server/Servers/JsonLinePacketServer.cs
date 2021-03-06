﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Firefly.TextEncoding;

namespace Server
{
    public class JsonLinePacketServer : IStreamedVirtualTransportServer
    {
        private class Context
        {
            public ArraySegment<Byte> ReadBuffer;
            public Object WriteBufferLockee = new Object();
            public List<Byte[]> WriteBuffer = new List<Byte[]>();

            public Context(int ReadBufferSize)
            {
                ReadBuffer = new ArraySegment<Byte>(new Byte[ReadBufferSize], 0, 0);
            }
        }

        public delegate Boolean CheckCommandAllowedDelegate(String CommandName);

        private IJsonSerializationServerAdapter ss;
        private Context c;
        private CheckCommandAllowedDelegate CheckCommandAllowed;
        private IBinaryTransformer Transformer;
        public JsonLinePacketServer(IJsonSerializationServerAdapter SerializationServerAdapter, CheckCommandAllowedDelegate CheckCommandAllowed, IBinaryTransformer Transformer = null, int ReadBufferSize = 8 * 1024)
        {
            this.ss = SerializationServerAdapter;
            this.c = new Context(ReadBufferSize);
            this.CheckCommandAllowed = CheckCommandAllowed;
            this.Transformer = Transformer;
            this.ss.ServerEvent += (CommandName, CommandHash, Parameters) =>
            {
                var Bytes = TextEncoding.UTF8.GetBytes(String.Format(@"/svr {0} {1}" + "\r\n", CommandName + "@" + CommandHash.ToString("X8", System.Globalization.CultureInfo.InvariantCulture), Parameters));
                lock (c.WriteBufferLockee)
                {
                    if (Transformer != null)
                    {
                        Transformer.Transform(Bytes, 0, Bytes.Length);
                    }
                    c.WriteBuffer.Add(Bytes);
                }
                if (OutputByteLengthReport != null)
                {
                    OutputByteLengthReport(CommandName, Bytes.Length);
                }
                if (this.ServerEvent != null)
                {
                    this.ServerEvent();
                }
            };
        }

        public ArraySegment<Byte> GetReadBuffer()
        {
            return c.ReadBuffer;
        }

        public Byte[][] TakeWriteBuffer()
        {
            lock (c.WriteBufferLockee)
            {
                var WriteBuffer = c.WriteBuffer.ToArray();
                c.WriteBuffer = new List<Byte[]>();
                return WriteBuffer;
            }
        }

        public StreamedVirtualTransportServerHandleResult Handle(int Count)
        {
            var ret = StreamedVirtualTransportServerHandleResult.CreateContinue();

            var Buffer = c.ReadBuffer.Array;
            var FirstPosition = c.ReadBuffer.Offset;
            var BufferLength = c.ReadBuffer.Offset + c.ReadBuffer.Count;
            var CheckPosition = FirstPosition;
            if (Transformer != null)
            {
                Transformer.Inverse(Buffer, BufferLength, Count);
            }
            BufferLength += Count;

            var LineFeedPosition = -1;
            for (int i = CheckPosition; i < BufferLength; i += 1)
            {
                Byte b = Buffer[i];
                if (b == '\n')
                {
                    LineFeedPosition = i;
                    break;
                }
            }
            if (LineFeedPosition >= FirstPosition)
            {
                var LineBytes = Buffer.Skip(FirstPosition).Take(LineFeedPosition - FirstPosition).Where(b => b != '\r').ToArray();
                var Line = TextEncoding.UTF8.GetString(LineBytes, 0, LineBytes.Length);
                var cmd = ParseCommand(Line);
                if (cmd.OnSome)
                {
                    var CommandName = cmd.Some.CommandName;
                    var CommandHash = cmd.Some.CommandHash;
                    var Parameters = cmd.Some.Parameters;
                    if (InputByteLengthReport != null)
                    {
                        InputByteLengthReport(CommandName, LineBytes.Length);
                    }
                    if ((CommandHash.OnSome ? ss.HasCommand(CommandName, CommandHash.Some) : ss.HasCommand(CommandName)) && (CheckCommandAllowed != null ? CheckCommandAllowed(CommandName) : true))
                    {
                        if (CommandHash.OnSome)
                        {
                            ret = StreamedVirtualTransportServerHandleResult.CreateCommand(new StreamedVirtualTransportServerHandleResultCommand
                            {
                                CommandName = CommandName,
                                ExecuteCommand = (OnSuccess, OnFailure) =>
                                {
                                    Action<String> OnSuccessInner = OutputParameters =>
                                    {
                                        var Bytes = TextEncoding.UTF8.GetBytes(String.Format(@"/svr {0} {1}" + "\r\n", CommandName + "@" + CommandHash.Some.ToString("X8", System.Globalization.CultureInfo.InvariantCulture), OutputParameters));
                                        lock (c.WriteBufferLockee)
                                        {
                                            if (Transformer != null)
                                            {
                                                Transformer.Transform(Bytes, 0, Bytes.Length);
                                            }
                                            c.WriteBuffer.Add(Bytes);
                                        }
                                        if (OutputByteLengthReport != null)
                                        {
                                            OutputByteLengthReport(CommandName, Bytes.Length);
                                        }
                                        OnSuccess();
                                    };
                                    ss.ExecuteCommand(CommandName, CommandHash.Some, Parameters, OnSuccessInner, OnFailure);
                                }
                            });
                        }
                        else
                        {
                            ret = StreamedVirtualTransportServerHandleResult.CreateCommand(new StreamedVirtualTransportServerHandleResultCommand
                            {
                                CommandName = CommandName,
                                ExecuteCommand = (OnSuccess, OnFailure) =>
                                {
                                    Action<String> OnSuccessInner = OutputParameters =>
                                    {
                                        var Bytes = TextEncoding.UTF8.GetBytes(String.Format(@"/svr {0} {1}" + "\r\n", CommandName, OutputParameters));
                                        lock (c.WriteBufferLockee)
                                        {
                                            c.WriteBuffer.Add(Bytes);
                                        }
                                        if (OutputByteLengthReport != null)
                                        {
                                            OutputByteLengthReport(CommandName, Bytes.Length);
                                        }
                                        OnSuccess();
                                    };
                                    ss.ExecuteCommand(CommandName, Parameters, OnSuccessInner, OnFailure);
                                }
                            });
                        }
                    }
                    else
                    {
                        ret = StreamedVirtualTransportServerHandleResult.CreateBadCommand(new StreamedVirtualTransportServerHandleResultBadCommand { CommandName = CommandName });
                    }
                }
                else if (cmd.OnNone)
                {
                    ret = StreamedVirtualTransportServerHandleResult.CreateBadCommandLine(new StreamedVirtualTransportServerHandleResultBadCommandLine { CommandLine = Line });
                }
                else
                {
                    throw new InvalidOperationException();
                }
                FirstPosition = LineFeedPosition + 1;
                CheckPosition = FirstPosition;
            }

            if (BufferLength >= Buffer.Length && FirstPosition > 0)
            {
                var CopyLength = BufferLength - FirstPosition;
                for (int i = 0; i < CopyLength; i += 1)
                {
                    Buffer[i] = Buffer[FirstPosition + i];
                }
                BufferLength = CopyLength;
                FirstPosition = 0;
            }
            if (FirstPosition >= BufferLength)
            {
                c.ReadBuffer = new ArraySegment<Byte>(Buffer, 0, 0);
            }
            else
            {
                c.ReadBuffer = new ArraySegment<Byte>(Buffer, FirstPosition, BufferLength - FirstPosition);
            }

            return ret;
        }

        public UInt64 Hash { get { return ss.Hash; } }
        public event Action ServerEvent;
        public event Action<String, int> InputByteLengthReport;
        public event Action<String, int> OutputByteLengthReport;

        private static Regex r = new Regex(@"^/(?<Name>\S+)(\s+(?<Params>.*))?$", RegexOptions.ExplicitCapture); //Regex是线程安全的
        private static Regex rName = new Regex(@"^(?<CommandName>.*?)@(?<CommandHash>.*)$", RegexOptions.ExplicitCapture); //Regex是线程安全的

        private class Command
        {
            public String CommandName;
            public Optional<UInt32> CommandHash;
            public String Parameters;
        }
        private Optional<Command> ParseCommand(String CommandLine)
        {
            var m = r.Match(CommandLine);
            if (!m.Success) { return Optional<Command>.Empty; }

            var Name = m.Result("${Name}");
            var mName = rName.Match(Name);
            String CommandName = Name;
            var CommandHash = Optional<UInt32>.Empty;
            if (mName.Success)
            {
                CommandName = mName.Result("${CommandName}");
                UInt32 ch;
                if (!UInt32.TryParse(mName.Result("${CommandHash}"), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out ch))
                {
                    return Optional<Command>.Empty;
                }
                CommandHash = ch;
            }
            var Parameters = m.Result("${Params}") ?? "";
            if (Parameters == "") { Parameters = "{}"; }

            return new Command { CommandName = CommandName, CommandHash = CommandHash, Parameters = Parameters };
        }
    }
}
