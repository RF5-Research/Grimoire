using AssetsTools.NET.Extra;
using Grimoire.Models.RF5;
using Grimoire.Models.RF5.Define;
using Grimoire.Models.RF5.Loader.ID;
using Grimoire.Models.UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static readonly Regex StatementRegex = new(@".+;");
        private static readonly Regex FunctionRegex = new(@"(?<FunctionName>\w+)(?:\s+)?(?<ParamStart>\()(?:.*)\)(?:\s+)?;");
        private static readonly Regex StringStartRegex = new(@"(\"")|((?:[^\\\\\\n])$)");
        private static readonly Regex StringEndRegex = new(@"(\"")|((?:[^\\\n])$)|\\(['\""\\0abfnrtv]|x[0-9a-fA-F]{1,4}|U[0-9a-fA-F]{8}|u[0-9a-fA-F]{4})");
        //private static readonly Regex StringEscapeCharactersRegex = new(@"\\(['\""\\0abfnrtv]|x[0-9a-fA-F]{1,4}|U[0-9a-fA-F]{8}|u[0-9a-fA-F]{4})");
        private static readonly Regex IntegerRegex = new(@"(\d+)");

        private AdvIndexData AdvIndexData;
        private TextAsset Pack;

        public static List<CommandData> Commands;

        public AdvScript(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                Commands = JsonSerializer.Deserialize<List<CommandData>>(reader.ReadToEnd())!;
            }
        }

        private string? ParseArg(string text, ref int start, string param)
        {
            switch (param)
            {
                case "string":
                    {
                        var stringStart = StringStartRegex.Match(text, start);
                        if (stringStart.Success)
                        {
                            var startOffset = stringStart.Index + stringStart.Length;
                            int endOffset = startOffset;
                            Match escape;
                            while ((escape = StringEndRegex.Match(text, endOffset)).Success && escape.Value[0] != '"')
                            {
                                endOffset = escape.Index + escape.Length;
                            }
                            var stringEnd = StringEndRegex.Match(text, endOffset);
                            if (stringEnd.Success)
                            {
                                start = stringEnd.Index + stringEnd.Length;
                                return text.Substring(startOffset, stringEnd.Index - startOffset)
                                    .Replace(@"\r", "\r")
                                    .Replace(@"\t", "\t")
                                    .Replace(@"\n", "\n");
                            }
                            else
                            {
                                Debug.WriteLine("Failed to parse string arg");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Failed to parse string arg");
                        }
                    }
                    break;
                case "bool":
                case "int":
                    {
                        var match = IntegerRegex.Match(text, start);
                        if (match.Success)
                        {
                            start = match.Index + match.Length;
                            return match.Value;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to parse int arg");
                        }
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        private List<Command> Tokenize(string script)
        {
            var commands = new List<Command>();
            if (script != null)
            {
                //This is still hacky
                var matches = StatementRegex.Matches(script);
                if (matches != null)
                {
                    var statements = matches.Cast<Match>().Select(x => x.Value).ToArray();

                    foreach (var statement in statements)
                    {
                        var args = new List<string>();
                        Command command;
                        var function = FunctionRegex.Match(statement);
                        if (function.Success)
                        {
                            var commandData = AdvScript.Commands.Find(x => x.Name == function.Groups["FunctionName"].Value);
                            if (commandData != null)
                            {
                                var paramStart = function.Groups["ParamStart"];
                                var startIndex = paramStart.Index + paramStart.Length;

                                foreach (var param in commandData.Params)
                                {
                                    args.Add(ParseArg(statement, ref startIndex, param.Value));
                                }
                                command = new(commandData, args.ToArray());
                                commands.Add(command);
                            }
                            else
                            {
                                //Error handling
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Couldn't tokenize `{statement}`");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Couldn't tokenize script:\n{script}");
                }
            }
            return commands;
        }

        private void Parse(List<List<Command>> scriptsData)
        {
            AdvIndexData.offset = new List<int>();
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                foreach (var script in scriptsData)
                {
                    var relativeOffset = ms.Position;
                    var stringTable = new Dictionary<long, byte[]>();
                    if (script.Count != 0)
                        writer.Write(script.Count);
                    foreach (var cmd in script)
                    {
                        writer.Write(cmd.Data.ID);
                        var argsData = cmd.Data.Params.Values.ToArray();
                        for (int index = 0; index < argsData.Length; index++)
                        {
                            switch (argsData[index])
                            {
                                case "string":
                                    {
                                        //Special case where this isn't actually used??
                                        if (cmd.Data.ID == 71)
                                        {
                                            writer.Write(0x0);
                                            writer.Write(0x8);
                                            break;
                                        }
                                        var text = Encoding.Unicode.GetBytes($"{cmd.Args[index].Trim('\"')}\0");
                                        writer.Write(text.Length / 2);
                                        stringTable.Add(ms.Position, text);
                                        writer.Write(0x0);
                                    }
                                    break;
                                case "bool":
                                case "int":
                                    {
                                        int.TryParse(cmd.Args[index], out int value);
                                        writer.Write(value);
                                        //Add error handling here
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    //Resolve string table
                    foreach (var item in stringTable)
                    {
                        var startOffset = ms.Position;
                        writer.Write(item.Value);
                        var curOffset = ms.Position;
                        ms.Position = item.Key;
                        writer.Write((int)(startOffset - relativeOffset));
                        ms.Position = curOffset;
                    }
                    AdvIndexData.offset.Add((int)ms.Position);
                }
                Pack.m_Script = ms.ToArray();
            }
        }

        public Dictionary<AdvScriptId, string> LoadPackScript()
        {
            var am = new AssetsManager();
            AdvIndexData = AssetsLoader.LoadID<AdvIndexData>(AssetsLoader.Master["ADVINDEXDATA"], am);
            Pack = AssetsLoader.LoadID<TextAsset>(AssetsLoader.Event["PACK"], am);
            return DecompilePack(Pack.m_Script, AdvIndexData);
        }

        public void SavePackScript(string[] scripts)
        {
            var scriptsData = new List<List<Command>>();
            foreach (var script in scripts)
            {
                scriptsData.Add(Tokenize(script));
            }
            Parse(scriptsData);
            AssetsLoader.WriteAsset(AdvIndexData, AssetsLoader.Master["ADVINDEXDATA"]);
            AssetsLoader.WriteAsset(Pack, AssetsLoader.Event["PACK"]);
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

            var stringBuilder = new StringBuilder();
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
                    stringBuilder.Append($"{cmdData.Name}({FormatArgs(args)});");
                    if (index + 1 < cmdNum)
                        stringBuilder.Append('\n');
                }
            }
            return stringBuilder.ToString();
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