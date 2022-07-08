using AssetsTools.NET.Extra;
using Grimoire;
using Grimoire.Models.RF5;
using Grimoire.Models.RF5.Define;
using Grimoire.Models.RF5.Loader.ID;
using Grimoire.Models.UnityEngine;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

namespace GrimoireGUI.ViewModels
{

    public class AdvScriptWindowViewModel : ViewModelBase
    {
        private static readonly Regex StatementRegex = new(@".+;");
        private static readonly Regex FunctionRegex = new(@"(?<FunctionName>\w+)(?:\s+)?(?<ParamStart>\()(?:.*)\)(?:\s+)?;");
        private static readonly Regex StringStartRegex = new(@"(\"")|((?:[^\\\\\\n])$)");
        private static readonly Regex StringEndRegex = new(@"(\"")|((?:[^\\\n])$)|\\(['\""\\0abfnrtv]|x[0-9a-fA-F]{1,4}|U[0-9a-fA-F]{8}|u[0-9a-fA-F]{4})");
        //private static readonly Regex StringEscapeCharactersRegex = new(@"\\(['\""\\0abfnrtv]|x[0-9a-fA-F]{1,4}|U[0-9a-fA-F]{8}|u[0-9a-fA-F]{4})");
        private static readonly Regex IntegerRegex = new(@"(\d+)");

        private Dictionary<AdvScriptId, string> Scripts;
        private ObservableCollection<AdvScriptId> ScriptList { get; }
        private AdvIndexData AdvIndexData;
        private TextAsset Pack;
        private string scriptText;
        public string ScriptText { get => scriptText; set => this.RaiseAndSetIfChanged(ref scriptText, value); }

        AdvScriptId selectedItem;
        public AdvScriptId SelectedItem
        {
            get => selectedItem;
            set
            {
                Scripts[selectedItem + 1] = ScriptText;
                this.RaiseAndSetIfChanged(ref selectedItem, value);
                ScriptText = Scripts[value + 1];
            }
        }

        private AdvScript AdvScript;

        public AdvScriptWindowViewModel()
        {
            //Just to ignore XAML designer errors
            try
            {
                AdvScript = new AdvScript($"Resources/AdvScriptFunctions.json");
                var am = new AssetsManager();
                AdvIndexData = AssetsLoader.LoadID<AdvIndexData>((int)Master.ADVINDEXDATA, am);

                Pack = AssetsLoader.LoadID<TextAsset>((int)Event.PACK, am);

                Scripts = AdvScript.DecompilePack(Encoding.Unicode.GetBytes(Pack.m_Script), AdvIndexData);
                ScriptList = new ObservableCollection<AdvScriptId>(Scripts.Keys);
                ScriptText = Scripts[selectedItem + 1];
            }
            catch { }
        }

        public List<CommandData> GetSymbols()
        {
            return AdvScript.Commands;
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
                                        writer.Write(text.Length/2);
                                        stringTable.Add(ms.Position, text);
                                        writer.Write(0x0);
                                    }
                                    break;
                                case "bool":
                                case "int":
                                    {
                                        int value;
                                        int.TryParse(cmd.Args[index], out value);
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
                Pack.m_Script = Encoding.Unicode.GetString(ms.ToArray());
            }
        }

        public void Save()
        {
            var scriptsData = new List<List<Command>>();
            foreach (var script in Scripts.Values)
            {
                scriptsData.Add(Tokenize(script));
            }
            Parse(scriptsData);
            AssetsLoader.WriteAsset(Pack, (int)Event.PACK);
            AssetsLoader.WriteAsset(AdvIndexData, (int)Master.ADVINDEXDATA);

        }
    }
}