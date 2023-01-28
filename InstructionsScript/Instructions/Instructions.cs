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

            public List<Instruction> Children = new List<Instruction>();

            private List<Instruction> WhileItems = new List<Instruction>();

            public Dictionary<string, Object> Vars = new Dictionary<string, object>();

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
                Children.Clear();
                WhileItems.Clear();
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
                    if (Children.Count == 0 || Index >= Children.Count)
                    {
                        State = StateBasic.Completing;
                    }
                    else
                    {
                        //Log($"Instruction Index:{Index}");
                        var instruction = Children[Index];
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
                        var instruction = ParseLine(aline);
                        if (instruction != null)
                        {
                            Children.Add(instruction);
                        }
                    }
                }
                Log($"Parse items {Children.Count}");
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

            private Instruction ParseLine(string line)
            {
                if (line.Trim() != null && !line.Trim().Equals(""))
                {
                    if (!line.StartsWith("#"))
                    {
                        char firstChar = line.Trim().First();
                        int deep = line.IndexOf(firstChar);
                        string[] values = line.Trim().Split(separator);
                        if (line.Trim().LastIndexOf(':') != -1)
                        {
                            var instanciated = InstanciateInstruction("Label", deep, values);
                            Log(instanciated.ToString());
                            return instanciated;
                        }
                        else
                        {
                            string type = values[0];
                            var instanciated = InstanciateInstruction(type, deep, values);
                            Log(instanciated.ToString());
                            return instanciated;
                        }
                    }
                }
                return null;
            }

            private Instruction InstanciateInstruction(string type, int deep, string[] Args)
            {
                int index = Children.Count;
                Instruction instanciated = null;
                switch (type)
                {
                    case InstructionAction.Name:
                        instanciated = new InstructionAction();
                        break;
                    case InstructionDefine.Name:
                        instanciated = new InstructionDefine();
                        break;
                    case InstructionDevice.Name:
                        instanciated = new InstructionDevice();
                        break;
                    case InstructionGet.Name:
                        instanciated = new InstructionGet();
                        break;
                    case InstructionJump.Name:
                        instanciated = new InstructionJump();
                        break;
                    case InstructionLabel.Name:
                        instanciated = new InstructionLabel();
                        break;
                    case InstructionPrint.Name:
                        instanciated = new InstructionPrint();
                        break;
                    case InstructionSet.Name:
                        instanciated = new InstructionSet();
                        break;
                    case InstructionWhile.Name:
                        instanciated = new InstructionWhile();
                        WhileItems.Add(instanciated);
                        break;
                    case InstructionEndWhile.Name:
                        instanciated = new InstructionEndWhile();
                        var whileItem = WhileItems.Pop();
                        whileItem.ReturnIndex = index + 1;
                        instanciated.ReturnIndex = whileItem.Index;
                        break;
                }
                instanciated.Deep = deep;
                instanciated.Index = index;
                instanciated.Main = this;
                instanciated.Args = Args;
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

        
    }
}
