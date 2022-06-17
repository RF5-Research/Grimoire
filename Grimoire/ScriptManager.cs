using RF5Game.Define;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Grimoire
{
    public class Command
    {
        public string name;
        public string[] args;
    }
    internal class CommandData
    {
        public string Name { get; set; }
        public short ID { get; set; }
        //Change this to `Params`
        public Dictionary<string, string> Args { get; set; }
    }

    public class ScriptManager
    {
        internal List<CommandData> Commands;
        public List<string> PackedScripts;
        //public Dictionary<string, byte[]> PackedScripts;

        public ScriptManager(string path)
        {
            PackedScripts = new List<string>();
            //PackedScripts = new Dictionary<string, byte[]>();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                Commands = JsonSerializer.Deserialize<List<CommandData>>(reader.ReadToEnd());
            }
        }

        public void ReadPackedFile(string packPath, string advIndexDataPath)
        {
            Dictionary<string, object> advIndexData;
            using (var fs = new FileStream(advIndexDataPath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                advIndexData = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.ReadToEnd());
            }

            int[] offsets = JsonSerializer.Deserialize<int[]>(advIndexData["offset"].ToString());

            using (var fs = new FileStream(packPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                var startOffset = 0;
                for (var index = 0; index < offsets.Length; index++)
                {
                    var endOffset = offsets[index];
                    PackedScripts.Add(DecompileScript(reader.ReadBytes(endOffset - startOffset)));
                    //var name = Enum.GetName((Define.AdvScriptId)index + 1);
                    //if (name != null)
                    //{
                    //    PackedScripts.Add(name, reader.ReadBytes(endOffset - startOffset));
                    //}
                    //else
                    //{
                    //    PackedScripts.Add(index.ToString(), reader.ReadBytes(endOffset - startOffset));
                    //}
                    startOffset = endOffset;
                }
            }
        }

        //public void CompileScript(List<Command> cmds)
        //{
        //    foreach (var cmd in cmds)
        //    {

        //    }
        //}

        public string DecompileScript(byte[] script)
        {
            if (script.Length == 0)
                return null;

            var lines = "";
            using (var ms = new MemoryStream(script))
            using (var reader = new BinaryReader(ms))
            {
                var cmdNum = reader.ReadInt32();

                for (var index = 0; index < cmdNum; index++)
                {
                    var cmdID = reader.ReadInt16();
                    var cmdData = SearchCommand(cmdID);
                    var args = new List<string>();
                    if (cmdData == null)
                    {
                        return null;
                    }
                    foreach (var arg in cmdData.Args)
                    {
                        switch (arg.Value)
                        {
                            case "String":
                                {
                                    var stringLen = reader.ReadInt32();
                                    var stringPTR = reader.ReadInt32();

                                    var pos = reader.BaseStream.Position;
                                    reader.BaseStream.Position = stringPTR;
                                    //UTF-16 Encoding and ignore null-terminator
                                    var text = Encoding.Unicode.GetString(reader.ReadBytes((stringLen) * 2));
                                    args.Add($"\"{text.TrimEnd('\0')}\"");
                                    reader.BaseStream.Position = pos;
                                }
                                break;
                            case "bool":
                            case "i32":
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

        //public string DecompileScript(string id)
        //{
        //    var script = PackedScripts[id];

        //    if (script.Length == 0)
        //        return null;

        //    var lines = "";
        //    using (var ms = new MemoryStream(script))
        //    using (var reader = new BinaryReader(ms))
        //    {
        //        var cmdNum = reader.ReadInt32();

        //        for (var index = 0; index < cmdNum; index++)
        //        {
        //            var cmdID = reader.ReadInt16();
        //            var cmdData = SearchCommand(cmdID);
        //            var args = new List<string>();
        //            foreach (var arg in cmdData.Args)
        //            {
        //                switch (arg.Value)
        //                {
        //                    case "String":
        //                        {
        //                            var stringLen = reader.ReadInt32();
        //                            var stringPTR = reader.ReadInt32();

        //                            var pos = reader.BaseStream.Position;
        //                            reader.BaseStream.Position = stringPTR;
        //                            //UTF-16 Encoding and ignore null-terminator
        //                            var text = Encoding.Unicode.GetString(reader.ReadBytes((stringLen) * 2));
        //                            args.Add($"\"{text.TrimEnd('\0')}\"");
        //                            reader.BaseStream.Position = pos;
        //                        }
        //                        break;
        //                    case "bool":
        //                    case "i32":
        //                        {
        //                            args.Add(reader.ReadInt32().ToString());
        //                        }
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //            lines += $"{cmdData.Name}({FormatArgs(args)});\n";
        //        }
        //    }
        //    return lines;
        //}

        string FormatArgs(List<string> args)
        {
            string formattedArgs = "";
            for (var index = 0; index < args.Count; index++)
            {
                formattedArgs += args[index];
                if (index != args.Count - 1)
                {
                    formattedArgs += ", ";
                }
            }
            return formattedArgs;
        }


        CommandData SearchCommand(short cmdID)
        {
            foreach (var cmd in Commands)
            {
                if (cmdID == cmd.ID)
                {
                    return cmd;
                }
            }
            return null;
        }
    }
}