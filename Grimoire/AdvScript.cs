using Grimoire.Models.RF5;
using Grimoire.Models.RF5.Define;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Grimoire
{
    public class CommandData
    {
        public string Name { get; set; }
        public short ID { get; set; }
        public string Description { get; set; }
        //Change this to `Params`
        public Dictionary<string, string> Params { get; set; }
    }

    public class Command
    {
        public CommandData Data { get; set; }
        public string[] Args { get; set; }

        public Command(CommandData data, string[] args)
        {
            Data = data;
            Args = args;
        }
        public Command() { }
    }

    public class AdvScript
    {
        public static List<CommandData> Commands;

        public AdvScript(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                Commands = JsonSerializer.Deserialize<List<CommandData>>(reader.ReadToEnd())!;
            }
        }

        private static string ToLiteral(string input)
        {
            //Better way to do this?
            return input
                .Replace("\r", @"\r")
                .Replace("\t", @"\t")
                .Replace("\n", @"\n");
        }

        public static Dictionary<AdvScriptId, string?> DecompilePack(byte[] pack, AdvIndexData advIndexData)
        {
            var scripts = new Dictionary<AdvScriptId, string?>(advIndexData.offset.Count);
            using (var fs = new MemoryStream(pack))
            using (var reader = new BinaryReader(fs))
            {
                var startOffset = 0;
                for (var index = 0; index < advIndexData.offset.Count; index++)
                {
                    var endOffset = advIndexData.offset[index];
                    scripts.Add(
                        (AdvScriptId)index + 1,
                        DecompileScript(reader.ReadBytes(endOffset - startOffset))
                    );
                    startOffset = endOffset;
                }
            }
            return scripts;
        }

        public static string? DecompileScript(byte[] script)
        {
            if (script.Length == 0)
                return null;

            var lines = "";
            //Use stringbuilder
            using (var ms = new MemoryStream(script))
            using (var reader = new BinaryReader(ms))
            {
                var cmdNum = reader.ReadInt32();

                for (var index = 0; index < cmdNum; index++)
                {
                    var cmdID = reader.ReadInt16();
                    var cmdData = SearchCommand(cmdID);
                    var args = new List<string>();
                    
                    foreach (var arg in cmdData.Params)
                    {
                        switch (arg.Value)
                        {
                            case "string":
                                {
                                    var stringLen = reader.ReadInt32();
                                    var stringPTR = reader.ReadInt32();

                                    var pos = reader.BaseStream.Position;
                                    reader.BaseStream.Position = stringPTR;
                                    //UTF-16 Encoding, pop null-terminator, and unescape escape characters
                                    var text = ToLiteral(
                                        Encoding.Unicode.GetString(reader.ReadBytes((stringLen) * 2))
                                        .TrimEnd('\0')
                                        );
                                    args.Add($"\"{text}\"");
                                    reader.BaseStream.Position = pos;
                                }
                                break;
                            case "bool":
                            case "int":
                                {
                                    args.Add(reader.ReadInt32().ToString());
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    lines += $"{cmdData.Name}({FormatArgs(args)});";
                    if (index + 1 < cmdNum)
                        lines += '\n';
                }
            }
            return lines;
        }

        static string FormatArgs(List<string> args)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < args.Count; index++)
            {
                stringBuilder.Append(args[index]);
                if (index != args.Count - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            return stringBuilder.ToString();
        }


        static CommandData SearchCommand(short cmdID)
        {
            foreach (var cmd in Commands)
            {
                if (cmdID == cmd.ID)
                {
                    return cmd;
                }
            }
            throw new Exception("Couldn't find command");
        }
    }
}