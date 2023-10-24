using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class Instructions
        {
            public Program myProgram;

            private List<string> logger = new List<string>();

            private char separator = ' ';

            public List<Instruction> Items = new List<Instruction>();

            public Dictionary<string, object> Vars = new Dictionary<string, object>();
            
            public Dictionary<string, InstructionType> Words = new Dictionary<string, InstructionType>();

            public StateBasic State = StateBasic.None;

            public int Index = 0;
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

                InterpretMisk.AppendWords(Words);

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
                        ExecuteInstruction();
                        break;
                }
                myProgram.drawingSurface.WriteText(String.Join("\n", logger));
            }

            public void Start()
            {
                State = StateBasic.Running;
            }

            public void ExecuteInstruction()
            {
                try
                {
                    if (Items.Count == 0 || Index >= Items.Count)
                    {
                        State = StateBasic.Completing;
                    }
                    else
                    {
                        //Log($"Instruction Index:{Index}");
                        var instruction = Items[Index];
                        instruction.Execute();
                    }
                } catch
                {
                    //Log($"ExecuteInstruction error {Index}/{Items.Count}");
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
                        if (instruction != null)
                        {
                            Items.Add(instruction);
                        }
                    }
                }
                Log($"Parse items {Items.Count}");
            }

            private void BuildTree()
            {

            }

            public void Log(string message)
            {
                if (logger.Count > 10) logger.RemoveAt(0);
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

            private Instruction ParseLine(string line, int index)
            {
                if (line.Trim() != null && !line.Trim().Equals(""))
                {
                    if (!line.StartsWith("#"))
                    {
                        char firstChar = line.Trim().First();
                        string[] values = line.Trim().Split(separator);
                        if (line.Trim().LastIndexOf(':') != -1)
                        {
                            var instanciated = InstanciateInstruction("Label", index, values);
                            Log(instanciated.ToString());
                            return instanciated;
                        }
                        else
                        {
                            string type = values[0];
                            if (string.IsNullOrEmpty(type) == false)
                            {
                                type = type.ToLower();
                            }
                            var instanciated = InstanciateInstruction(type, index, values);
                            Log(instanciated.ToString());
                            return instanciated;
                        }
                    }
                }
                return null;
            }

            private Instruction InstanciateInstruction(string type, int index, string[] Args)
            {
                Instruction instanciated = new Instruction();
                instanciated.Index = index;
                instanciated.Parent = this;
                instanciated.Args = Args;

                if (Words.ContainsKey(type))
                {
                    instanciated.Type = Words[type];
                }
                else
                {
                    instanciated.Type = InstructionType.None;
                }

                return instanciated;
            }

            protected void MappingWords()
            {
                // Misc
                // alias str r?|d?
                Words.Add("alias", InstructionType.Misc);
                // define str num
                Words.Add("define", InstructionType.Misc);
                // Pauses execution for 1 tick
                Words.Add("yield", InstructionType.Misc);
                // move r? a(r?|num)
                Words.Add("move", InstructionType.Misc);

                // Device IO
                Words.Add("l", InstructionType.DeviceIO);
                Words.Add("s", InstructionType.DeviceIO);

                // Branch
                Words.Add("bap", InstructionType.Branch);
                Words.Add("bapal", InstructionType.Branch);
                Words.Add("bapz", InstructionType.Branch);
                Words.Add("bapzal", InstructionType.Branch);
                Words.Add("beq", InstructionType.Branch);
                Words.Add("beqal", InstructionType.Branch);
                Words.Add("beqz", InstructionType.Branch);
                Words.Add("beqzal", InstructionType.Branch);
                Words.Add("bge", InstructionType.Branch);
                Words.Add("bgeal", InstructionType.Branch);
                Words.Add("bgez", InstructionType.Branch);
                Words.Add("bgezal", InstructionType.Branch);
                Words.Add("bgt", InstructionType.Branch);
                Words.Add("bgtal", InstructionType.Branch);
                Words.Add("bgtz", InstructionType.Branch);
                Words.Add("bgtzal", InstructionType.Branch);
                Words.Add("ble", InstructionType.Branch);
                Words.Add("bleal", InstructionType.Branch);
                Words.Add("blez", InstructionType.Branch);
                Words.Add("blezal", InstructionType.Branch);
                Words.Add("blt", InstructionType.Branch);
                Words.Add("bltal", InstructionType.Branch);
                Words.Add("bltz", InstructionType.Branch);
                Words.Add("bltzal", InstructionType.Branch);
                Words.Add("bna", InstructionType.Branch);
                Words.Add("bnaal", InstructionType.Branch);
                Words.Add("bnaz", InstructionType.Branch);
                Words.Add("bnazal", InstructionType.Branch);
                Words.Add("bne", InstructionType.Branch);
                Words.Add("bneal", InstructionType.Branch);
                Words.Add("bnez", InstructionType.Branch);
                Words.Add("bnezal", InstructionType.Branch);
                Words.Add("brap", InstructionType.Branch);
                Words.Add("brapz", InstructionType.Branch);
                Words.Add("breq", InstructionType.Branch);
                Words.Add("breqz", InstructionType.Branch);
                Words.Add("brge", InstructionType.Branch);
                Words.Add("brgez", InstructionType.Branch);
                Words.Add("brgt", InstructionType.Branch);
                Words.Add("brgtz", InstructionType.Branch);
                Words.Add("brle", InstructionType.Branch);
                Words.Add("brlez", InstructionType.Branch);
                Words.Add("brlt", InstructionType.Branch);
                Words.Add("brltz", InstructionType.Branch);
                Words.Add("brna", InstructionType.Branch);
                Words.Add("brnaz", InstructionType.Branch);
                Words.Add("brne", InstructionType.Branch);
                Words.Add("brnez", InstructionType.Branch);

                // Selection
                Words.Add("sap", InstructionType.Selection);
                Words.Add("sapz", InstructionType.Selection);
                Words.Add("sdns", InstructionType.Selection);
                Words.Add("sdse", InstructionType.Selection);
                Words.Add("select", InstructionType.Selection);
                Words.Add("seq", InstructionType.Selection);
                Words.Add("seqz", InstructionType.Selection);
                Words.Add("sge", InstructionType.Selection);
                Words.Add("sgez", InstructionType.Selection);
                Words.Add("sgt", InstructionType.Selection);
                Words.Add("sgtz", InstructionType.Selection);
                Words.Add("sle", InstructionType.Selection);
                Words.Add("slez", InstructionType.Selection);
                Words.Add("slt", InstructionType.Selection);
                Words.Add("sltz", InstructionType.Selection);
                Words.Add("sna", InstructionType.Selection);
                Words.Add("snaz", InstructionType.Selection);
                Words.Add("sne", InstructionType.Selection);
                Words.Add("snez", InstructionType.Selection);

                // Jump
                Words.Add("j", InstructionType.Jump);
                Words.Add("jal", InstructionType.Jump);
                Words.Add("jr", InstructionType.Jump);

                // Mathematic
                Words.Add("abs", InstructionType.Mathematical);
                Words.Add("acos", InstructionType.Mathematical);
                Words.Add("add", InstructionType.Mathematical);
                Words.Add("asin", InstructionType.Mathematical);
                Words.Add("atan", InstructionType.Mathematical);
                Words.Add("ceil", InstructionType.Mathematical);
                Words.Add("cos", InstructionType.Mathematical);
                Words.Add("div", InstructionType.Mathematical);
                Words.Add("exp", InstructionType.Mathematical);
                Words.Add("floor", InstructionType.Mathematical);
                Words.Add("log", InstructionType.Mathematical);
                Words.Add("max", InstructionType.Mathematical);
                Words.Add("min", InstructionType.Mathematical);
                Words.Add("mod", InstructionType.Mathematical);
                Words.Add("mul", InstructionType.Mathematical);
                Words.Add("rand", InstructionType.Mathematical);
                Words.Add("round", InstructionType.Mathematical);
                Words.Add("sin", InstructionType.Mathematical);
                Words.Add("sqrt", InstructionType.Mathematical);
                Words.Add("sub", InstructionType.Mathematical);
                Words.Add("tan", InstructionType.Mathematical);
                Words.Add("trunc", InstructionType.Mathematical);

                // Logic
                Words.Add("and", InstructionType.Logic);
                Words.Add("nor", InstructionType.Logic);
                Words.Add("or", InstructionType.Logic);
                Words.Add("xor", InstructionType.Logic);

                // Stack
                Words.Add("peek", InstructionType.Stack);
                Words.Add("pop", InstructionType.Stack);
                Words.Add("push", InstructionType.Stack);
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
            DeviceIO,
            Branch,
            Jump,
            Selection,
            Mathematical,
            Logic,
            Stack,
            Misc
        }
    }
}
