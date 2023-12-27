using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program
    {
        public class Instructions
        {
            public Program myProgram;

            private List<string> logger = new List<string>();

            private MyCommandLine commandLine = new MyCommandLine();

            private char separator = ' ';

            public List<Instruction> Items = new List<Instruction>();

            public Dictionary<string, object> Devices = new Dictionary<string, object>();

            public Dictionary<string, object> Vars = new Dictionary<string, object>();

            public Dictionary<string, int> Labels = new Dictionary<string, int>();

            public Dictionary<string, InstructionType> Words = new Dictionary<string, InstructionType>();

            public StateBasic State = StateBasic.None;

            public int Index = 0;
            public bool FirstStarted = false;
            public Instructions(Program program)
            {
                myProgram = program;
            }
            public void Init()
            {
                logger.Clear();
                Index = 0;
                Vars.Clear();
                Items.Clear();
                Words.Clear();
                Labels.Clear();

                SetVar("PY", Math.PI);

                InterpretProgram.AppendWords(Words);
                InterpretBranch.AppendWords(Words);
                InterpretDevice.AppendWords(Words);
                InterpretJump.AppendWords(Words);
                InterpretLogic.AppendWords(Words);
                InterpretMath.AppendWords(Words);
                InterpretMisk.AppendWords(Words);
                InterpretSelection.AppendWords(Words);
                Log($"Words {Words.Count}");

                Parse(myProgram.Me.CustomData);
            }

            public void Execute()
            {
                switch (State)
                {
                    case StateBasic.Completing:
                        State = StateBasic.Completed;
                        Log("Program completed");
                        break;
                    case StateBasic.Running:
                        ExecuteProgram();
                        break;
                }
                myProgram.drawingSurface.WriteText(String.Join("\n", logger));
            }
            public void ExecuteLabel(string name)
            {
                try
                {
                    if (Labels.ContainsKey(name))
                    {
                        if (Items.Count == 0 || Index >= Items.Count)
                        {
                            State = StateBasic.Completing;
                        }
                        else
                        {
                            ExecuteInstructions(true);
                        }
                        var adress = Labels[name];
                        Index = adress;
                        State = StateBasic.Running;
                    }
                    else
                    {
                        Log($"Execution error: The label does not exist");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Execution Label error {Index + 1}: {ex.Message}");
                    Log(ex.StackTrace);
                    State = StateBasic.Completing;
                }
            }
            public void Start()
            {
                State = StateBasic.Running;
                FirstStarted = true;
            }

            public void ExecuteProgram()
            {
                try
                {
                    if (Items.Count == 0 || Index >= Items.Count)
                    {
                        State = StateBasic.Completing;
                    }
                    else
                    {
                        ExecuteInstructions();
                    }
                }
                catch (Exception ex)
                {
                    Log($"Program error {Index + 1}: {ex.Message}");
                    Log(ex.StackTrace);
                    State = StateBasic.Completing;
                }
            }

            public void ExecuteInstructions(bool isSettings = false)
            {
                try
                {
                    var loop = 0;
                    var wait = true;
                    while (wait)
                    {
                        //Log($"** loop = {loop}");
                        var instruction = Items[Index];
                        if (instruction == null)
                        {
                            Index++;
                            if (Index >= Items.Count)
                            {
                                wait = false;
                            }
                        }
                        else
                        {
                            if (instruction.Type == InstructionType.Label)
                            {
                                Log($"== {instruction.Command} at {Index + 1}");
                            }
                            else
                            {
                                Log($"-> {instruction.Command} at {Index + 1}");
                            }
                            switch (instruction.Type)
                            {
                                case InstructionType.Program:
                                    InterpretProgram.Interpret(instruction);
                                    break;
                                case InstructionType.Branch:
                                    InterpretBranch.Interpret(instruction);
                                    break;
                                case InstructionType.Device:
                                    InterpretDevice.Interpret(instruction);
                                    break;
                                case InstructionType.Jump:
                                    InterpretJump.Interpret(instruction);
                                    break;
                                case InstructionType.Logic:
                                    InterpretLogic.Interpret(instruction);
                                    break;
                                case InstructionType.Math:
                                    InterpretMath.Interpret(instruction);
                                    break;
                                case InstructionType.Misc:
                                    InterpretMisk.Interpret(instruction);
                                    break;
                                case InstructionType.Selection:
                                    InterpretSelection.Interpret(instruction);
                                    break;
                            }
                            Index = instruction.NextIndex;
                            if (instruction.Name == MiskWords.yield.ToString() || Index >= Items.Count)
                            {
                                wait = false;
                            }
                            else
                            {
                                loop++;
                                wait = loop < 1024;
                            }
                            if (isSettings)
                            {
                                if (instruction.Type == InstructionType.Label)
                                {
                                    wait = false;
                                }
                            }
                        }

                    }
                }
                catch (InstructionException ex)
                {
                    Log($"Instruction error {Index + 1}: {ex.Message}");
                    State = StateBasic.Completing;
                }
                catch (Exception ex)
                {
                    Log($"Instruction error {Index + 1}: {ex.Message}");
                    Log(ex.StackTrace);
                    State = StateBasic.Completing;
                }
            }
            public int LogLine = 20;
            public void Log(string message)
            {
                if (logger.Count > LogLine) logger.RemoveAt(0);
                logger.Add(message);
            }

            public void SetVar(string name, object value)
            {
                if (Vars.ContainsKey(name))
                {
                    Vars[name] = value;
                }
                else
                {
                    Vars.Add(name, value);
                }
            }

            public void SetDevice(string name, object value)
            {
                if (Devices.ContainsKey(name))
                {
                    Devices[name] = value;
                }
                else
                {
                    Devices.Add(name, value);
                }
            }

            private void Parse(string script)
            {
                if (script == null) return;
                string[] lines = script.Split('\n');
                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string aline = lines[i];
                        var instruction = ParseLine(aline, i);
                        Items.Add(instruction);
                    }
                }
                Log($"Parsed lines {Items.Count}");
            }

            private Instruction ParseLine(string line, int index)
            {
                if(line != null) line = line.Trim();
                if (line != null && !line.Equals(""))
                {
                    if (line.StartsWith("#"))
                    {
                        Instruction instanciated = new Instruction();
                        instanciated.Command = line;
                        instanciated.Index = index;
                        instanciated.NextIndex = index + 1;
                        instanciated.Parent = this;
                        instanciated.Type = InstructionType.None;
                    }
                    else
                    {
                        if (line.Contains("#"))
                        {
                            var splited = line.Split('#');
                            line = splited[0];
                        }
                        commandLine.TryParse(line);
                        //char firstChar = line.Trim().First();
                        //string[] values = line.Trim().Split(separator);
                        string[] values = commandLine.Items.ToArray();
                        string name = values[0];
                        if (string.IsNullOrEmpty(name) == false)
                        {
                            name = name.ToLower();
                        }
                        Log($"try instanciate {line} at {index + 1}");
                        var instanciated = InstanciateInstruction(line, name, index, values);
                        return instanciated;
                    }

                }
                return null;
            }

            private Instruction InstanciateInstruction(string command, string name, int index, string[] Args)
            {
                Instruction instanciated = new Instruction();
                instanciated.Command = command;
                instanciated.Name = name;
                instanciated.Index = index;
                instanciated.NextIndex = index + 1;
                instanciated.Parent = this;
                instanciated.Args = Args;

                if (name.LastIndexOf(':') != -1)
                {
                    instanciated.Type = InstructionType.Label;
                    var label = name.Substring(0, name.LastIndexOf(":"));
                    Labels.Add(label, index);
                }
                else
                {
                    if (Words.ContainsKey(name))
                    {
                        instanciated.Type = Words[name];
                    }
                    else
                    {
                        throw new InstructionException($"Instruction '{name}' not found at {index}");
                    }
                }
                return instanciated;
            }

        }

        public enum StateBasic
        {
            None,
            Stopped,
            Completing,
            Completed,
            Running,
            Waitting,
            Sleeped
        }

        public enum InstructionType
        {
            None,
            Label,
            Device,
            Branch,
            Jump,
            Selection,
            Math,
            Logic,
            Stack,
            Misc,
            Import,
            Program
        }
    }
}
