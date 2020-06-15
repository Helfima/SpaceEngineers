using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using System.ComponentModel;

namespace IngameScript
{
    partial class Program
    {
        public class Basic
        {
            public Program myProgram;

            private List<Instruction> Instructions = new List<Instruction>();

            private Dictionary<string, Object> Vars;

            private int Line = 0;

            private StateBasic State = StateBasic.None;
            public Basic(Program program)
            {
                myProgram = program;
            }

            public void Init()
            {
                Line = 0;
                Instructions = new List<Instruction>();
                Vars = new Dictionary<string, object>();
                Vars.Add("PI", 3.14159f);
                ParseCustomData(myProgram.Me.CustomData);
                myProgram.Echo($"Basic program Loaded ({Instructions.Count} line)");
                myProgram.drawingSurface.WriteText($"\nBasic program Loaded ({Instructions.Count} line)", true);
            }
            private void ParseCustomData(string script)
            {
                if (script == null) return;
                string aLine;
                string[] lines = script.Split('\n');
                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        aLine = lines[i];
                        Instructions.Add(Instruction.Create(myProgram, aLine));
                    }
                }
            }

            public void Execute()
            {
                if (State != StateBasic.Completed && (Line >= Instructions.Count || Instructions.Count == 0))
                {
                    Line = 0;
                    State = StateBasic.Completed;
                    myProgram.drawingSurface.WriteText("\nProgram completed", true);
                }
                switch (State)
                {
                    case StateBasic.Running:
                        ExecuteInstruction();
                        break;
                }
            }

            public void Start()
            {
                State = StateBasic.Running;
            }
            public void ExecuteInstruction()
            {
                if (Instructions.Count <= 0 || Instructions.Count < Line) return;
                Instruction instruction = Instructions[Line];
                string name, mode, action, search, property, source;
                float value1 = 0f;
                float value2 = 0f;
                float epsilon = 0.1f;
                bool bool1 = false;
                bool bool2 = false;
                Object var;

                switch (instruction.Command)
                {
                    case Command.Comment:
                        Line++;
                        break;
                    case Command.Var:
                        // var <var name> <float value>
                        name = instruction.Fields[0];
                        ParseFieldFloat(instruction.Fields[1], out value1);
                        Vars.Add(name, value1);
                        Line++;
                        break;
                    case Command.Add:
                    case Command.Sub:
                    case Command.Mul:
                    case Command.Div:
                        // <operator> <var name> <var name|float value> <float value>
                        name = instruction.Fields[0];
                        ParseFieldFloat(instruction.Fields[1], out value1);
                        ParseFieldFloat(instruction.Fields[2], out value2);
                        float result = 0f;
                        switch (instruction.Command)
                        {
                            case Command.Add:
                                result = value1 + value2;
                                break;
                            case Command.Sub:
                                result = value1 - value2;
                                break;
                            case Command.Mul:
                                result = value1 * value2;
                                break;
                            case Command.Div:
                                result = value1 / value2;
                                break;
                        }
                        Vars[name] = result;
                        Line++;
                        break;
                    case Command.Print:
                        bool append = true;
                        string message = ParseFieldString(instruction.Fields[0]);
                        message = "\n" + message;
                        if (instruction.Fields.Count > 1)
                        {
                            List<string> args = new List<string>();
                            for (int i = 1; i < instruction.Fields.Count; i++)
                            {
                                float value;
                                ParseFieldFloat(instruction.Fields[1], out value);
                                args.Add(value.ToString());
                            }
                            myProgram.drawingSurface.WriteText(string.Format(message, args.ToArray()), append);
                        }
                        else
                        {
                            myProgram.drawingSurface.WriteText(message, append);
                        }
                        Line++;
                        break;
                    case Command.Select:
                        // select <var name> <mode> <string value>
                        //myProgram.drawingSurface.WriteText($"\nSelect fields={instruction.Fields.Count}", true);
                        name = instruction.Fields[0];
                        mode = instruction.Fields[1];
                        search = instruction.Fields[2];
                        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                        switch (mode)
                        {
                            case "tag":
                                myProgram.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, myBlock => myBlock.CustomName.Contains(search));
                                if (blocks.Count > 0)
                                {
                                    Vars.Add(name, blocks);
                                }
                                break;
                            case "group":
                                IMyBlockGroup group = myProgram.GridTerminalSystem.GetBlockGroupWithName(search);
                                group.GetBlocks(blocks);
                                if (blocks.Count > 0)
                                {
                                    Vars.Add(name, blocks);
                                }
                                break;
                            default:
                                IMyTerminalBlock block = myProgram.GridTerminalSystem.GetBlockWithName(search);
                                if(block != null)
                                {
                                    blocks.Add(block);
                                    Vars.Add(name, blocks);
                                }
                                break;
                        }
                        Line++;
                        break;
                    case Command.Action:
                        // action <var name> <action>
                        name = instruction.Fields[0];
                        action = instruction.Fields[1];
                        Vars.TryGetValue(name, out var);
                        if (instruction.Fields.Count == 3)
                        {
                            ParseFieldFloat(instruction.Fields[2], out value1);
                        }
                        if (var != null)
                        {
                            blocks = (List<IMyTerminalBlock>)var;
                            blocks.ForEach(delegate (IMyTerminalBlock block) {
                                block.ApplyAction(action);
                            });
                        }
                        Line++;
                        break;
                    case Command.Set:
                        // set <var name> <property> <value>
                        name = instruction.Fields[0];
                        property = instruction.Fields[1];
                        Vars.TryGetValue(name, out var);
                        if (var != null)
                        {
                            if (var is List<IMyTerminalBlock>)
                            {
                                blocks = (List<IMyTerminalBlock>)var;
                                if (blocks.Count > 0)
                                {
                                    IMyTerminalBlock firstBlock = blocks[0];
                                    ITerminalProperty prop = firstBlock.GetProperty(property);
                                    //myProgram.drawingSurface.WriteText($"\nProperty={prop.TypeName}", true);
                                    switch (prop.TypeName)
                                    {
                                        case "Single":
                                            ParseFieldFloat(instruction.Fields[2], out value1);
                                            break;
                                        case "Boolean":
                                            ParseFieldBool(instruction.Fields[2], out bool1);
                                            break;
                                    }
                                    blocks.ForEach(delegate (IMyTerminalBlock block)
                                    {
                                        switch (prop.TypeName)
                                        {
                                            case "Single":
                                                block.SetValueFloat(property, value1);
                                                break;
                                            case "Boolean":
                                                block.SetValueBool(property, bool1);
                                                break;
                                        }
                                    });
                                }
                            }
                        }
                        Line++;
                        break;
                    case Command.Get:
                        // get <var name> <property> <var name>
                        name = instruction.Fields[0];
                        property = instruction.Fields[1];
                        source = instruction.Fields[2];
                        Vars.TryGetValue(source, out var);
                        if (var != null)
                        {
                            if (var is List<IMyTerminalBlock>)
                            {
                                blocks = (List<IMyTerminalBlock>)var;
                                if (blocks.Count > 0)
                                {
                                    IMyTerminalBlock firstBlock = blocks[0];
                                    ITerminalProperty prop = firstBlock.GetProperty(property);
                                    switch (prop.TypeName)
                                    {
                                        case "Single":
                                            Vars.Add(name, firstBlock.GetValueFloat(property));
                                            break;
                                        case "Boolean":
                                            Vars.Add(name, firstBlock.GetValueBool(property));
                                            break;
                                    }
                                }
                            }
                        }
                        Line++;
                        break;
                    case Command.Wait:
                        // wait <var name> <property> <value> <epsilon>
                        name = instruction.Fields[0];
                        property = instruction.Fields[1];
                        if(instruction.Fields.Count > 3)
                        {
                            ParseFieldFloat(instruction.Fields[3], out epsilon);
                        }
                        Vars.TryGetValue(name, out var);
                        bool isState = true;
                        if (var != null)
                        {
                            if (var is List<IMyTerminalBlock>)
                            {
                                blocks = (List<IMyTerminalBlock>)var;
                                if (blocks.Count > 0)
                                {
                                    ITerminalProperty prop = null;
                                    IMyTerminalBlock firstBlock = blocks[0];
                                    prop = firstBlock.GetProperty(property);
                                    if (prop == null)
                                    {
                                        switch (GetAttibutType(property))
                                        {
                                            case "Single":
                                                ParseFieldFloat(instruction.Fields[2], out value1);
                                                break;
                                            case "Boolean":
                                                ParseFieldBool(instruction.Fields[2], out bool1);
                                                break;
                                        }
                                    }
                                    else 
                                    { 
                                        switch (prop.TypeName)
                                        {
                                            case "Single":
                                                ParseFieldFloat(instruction.Fields[2], out value1);
                                                break;
                                            case "Boolean":
                                                ParseFieldBool(instruction.Fields[2], out bool1);
                                                break;
                                        }
                                        break;
                                    }

                                    blocks.ForEach(delegate (IMyTerminalBlock block)
                                    {
                                        if (prop == null) { 
                                            switch (GetAttibutType(property))
                                            {
                                                case "Single":
                                                    value2 = GetAttibutFloat(block, property);
                                                    if (Math.Abs(value2 - value1) > epsilon) isState = false;
                                                    break;
                                                case "Boolean":
                                                    bool2 = GetAttibutBool(block, property);
                                                    if (bool2 != bool1) isState = false;
                                                    break;
                                            }
                                        } 
                                        else
                                        {
                                            switch (prop.TypeName)
                                            {
                                                case "Single":
                                                    value2 = block.GetValueFloat(property);
                                                    if (Math.Abs(value2 - value1) > epsilon) isState = false;
                                                    break;
                                                case "Boolean":
                                                    bool2 = block.GetValueBool(property);
                                                    if (bool2 != bool1) isState = false;
                                                    break;
                                            }
                                        }
                                    });
                                }
                            }
                        }
                        if (isState)
                        {
                            Line++;
                        }
                        break;
                }
            }

            private float GetAttibutFloat(IMyTerminalBlock block, string name)
            {
                if (block is IMyPistonBase)
                {
                    switch (name)
                    {
                        case "CurrentPosition":
                            return ((IMyPistonBase)block).CurrentPosition;
                        default:
                            return 0f;
                    }

                }
                if (block is IMyMotorStator)
                {
                    switch (name)
                    {
                        case "Angle":
                            return ((IMyMotorStator)block).Angle;
                        default:
                            return 0f;
                    }

                }
                return 0f;
            }

            private bool GetAttibutBool(IMyTerminalBlock block, string name)
            {
                if (block is IMyShipMergeBlock)
                {
                    switch (name)
                    {
                        case "IsConnected":
                            return ((IMyShipMergeBlock)block).IsConnected;
                        default:
                            return false;
                    }
                }
                if (block is IMySensorBlock)
                {
                    switch (name)
                    {
                        case "IsActive":
                            return ((IMySensorBlock)block).IsActive;
                        default:
                            return false;
                    }
                }
                return false;
            }

            private string GetAttibutType(string name)
            {
                switch (name)
                {
                    case "IsActive":
                    case "IsConnected":
                        return "Boolean";
                    default:
                        return "Single";
                }
            }

            private string ParseFieldString(string field)
            {
                Object var;
                if (Vars.TryGetValue(field, out var))
                {
                    return var.ToString();
                }
                return field;
            }
            private void ParseFieldFloat(string field, out float value)
            {
                if (!float.TryParse(field, out value))
                {
                    Object var;
                    if (Vars.TryGetValue(field, out var))
                    {
                        value = (float)var;
                    }
                }
            }
            private void ParseFieldBool(string field, out bool value)
            {
                if (!bool.TryParse(field, out value))
                {
                    Object var;
                    if (Vars.TryGetValue(field, out var))
                    {
                        value = (bool)var;
                    }
                }
            }
        }

        public class Instruction 
        {
            public Command Command = Command.Comment;
            public string Text = "#Comment";

            public List<string> Fields = new List<string>();

            public static Instruction Create(Program program, string line)
            {
                Command com = Command.Comment;
                Instruction inst = new Instruction()
                {
                    Text = line,
                    Command = com
                };
                if (line != null && !line.StartsWith("#") && !line.Equals(""))
                {
                    int index = line.IndexOf(' ');
                    string word = line.Substring(0, index).Trim();
                    string input = word.First().ToString().ToUpper() + word.Substring(1);
                    Command.TryParse(input, out com);
                    inst.Command = com;

                    string arguments = line.Substring(index).Trim();
                    bool quote = false;
                    if (arguments.Length > 0 )
                    {
                        StringBuilder expression = new StringBuilder();
                        for (int i = 0; i < arguments.Length; i++)
                        {
                            if((!arguments[i].Equals('"') && !arguments[i].Equals(' ')) || (arguments[i].Equals(' ') && quote))
                            {
                                expression.Append(arguments[i]);
                                //program.drawingSurface.WriteText("\n"+expression.ToString(), true);
                            }
                            else if(arguments[i].Equals('"'))
                            {
                                quote = !quote;
                            }
                            else
                            {
                                inst.Fields.Add(expression.ToString());
                                expression.Clear();
                            }

                        }
                        inst.Fields.Add(expression.ToString());
                    }
                }
                return inst;
            }
        }

        public enum StateBasic
        {
            None,
            Stopped,
            Completed,
            Running,
            Waitting,
            Sleeped
        }
        public enum Command
        {
            Comment,
            Var,
            Add,
            Sub,
            Mul,
            Div,
            Print,
            Select,
            Action,
            Wait,
            Property,
            Label,
            Goto,
            Set,
            Get,
            Sleep,
        }
    }
}
