﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniSRC_ASM_CMD
{
    class InstructionParser
    {
        public Dictionary<String, String> opcodeDictionary = new Dictionary<string, string>(); //dictionary to match operation with opcode
        public Dictionary<String, String> branchSuffix = new Dictionary<string, string>();

        public InstructionParser()
        {
            opcodeDictionary.Add("ld", "00000"); // Load and Store
            opcodeDictionary.Add("ldi", "00001");
            //opcodeDictionary.Add("ldr", "00011"); // not in winter 2021?
            opcodeDictionary.Add("st", "00010");
            opcodeDictionary.Add("add", "00011"); // Arithmetic and logic
            opcodeDictionary.Add("sub", "00100");
            opcodeDictionary.Add("shr", "00101");
            opcodeDictionary.Add("shl", "00110");
            opcodeDictionary.Add("ror", "00111");
            opcodeDictionary.Add("rol", "01000");
            opcodeDictionary.Add("and", "01001");
            opcodeDictionary.Add("or", "01010"); //
            opcodeDictionary.Add("addi", "01011");
            opcodeDictionary.Add("andi", "01100");
            opcodeDictionary.Add("ori", "01101");
            opcodeDictionary.Add("mul", "01110");
            opcodeDictionary.Add("div", "01111");
            opcodeDictionary.Add("neg", "10000");//
            opcodeDictionary.Add("not", "10001");//
            opcodeDictionary.Add("brzr", "10010"); // Branch instructions, all same op code
            opcodeDictionary.Add("brnz", "10010"); // see branch suffix for differences
            opcodeDictionary.Add("brmi", "10010");
            opcodeDictionary.Add("brpl", "10010");
            opcodeDictionary.Add("jr", "10011");  // Jump instructions
            opcodeDictionary.Add("jal", "10100");
            opcodeDictionary.Add("in", "10101");  // input/output and MFHI/MFLO 
            opcodeDictionary.Add("out", "10110");
            opcodeDictionary.Add("mfhi", "10111");
            opcodeDictionary.Add("mflo", "11000");
            opcodeDictionary.Add("nop", "11001"); // hardcoded later in the code
            opcodeDictionary.Add("halt", "11010");

            branchSuffix.Add("brzr", "00"); // branch suffix specification
            branchSuffix.Add("brnz", "01");
            branchSuffix.Add("brpl", "10");
            branchSuffix.Add("brmi", "11");
        }

        public static string RxToBin(string rx)
        {
            try
            {
                int reg = Convert.ToInt32(rx.ToLower().Remove(0, 1));

                if(reg >= 0 && reg < 17)
                {
                    return Convert.ToString(reg, 2).PadLeft(4, '0');
                }
                else
                {
                    throw new ArgumentException("Invalid Register: " + rx);
                }
            }
            catch
            {
                throw new ArgumentException("Invalid Register: " + rx);
            }

        }

        public SRCInstruction ParseInstruction(string inst)
        {
            SRCInstruction tmpInst = new SRCInstruction();
            tmpInst.originalInstructionString = inst;
            tmpInst.outputInstructionString = "";
            tmpInst.replaceImmediateWithAddress = false;

            string instruction = inst.ToLower();

            if(instruction == "nop")
            {
                tmpInst.outputInstructionString = "C8000000";
                return tmpInst; //opcode for NOP
            }
            else if(instruction == "halt")
            {
                tmpInst.outputInstructionString = "D0000000"; //opcode for HALT!
                return tmpInst;
            }
            else //not a single word no operand instruction
            {
                string[] wsplit = inst.ToLower().Split(' ');
                StringBuilder s = new StringBuilder();
                s.Append(opcodeDictionary[wsplit[0]]);

                try
                {
                    switch (wsplit[0])
                    {
                        case "ld":
                        case "ldi":
                            {
                                string regString;

                                if (wsplit.Length > 2)
                                {
                                    regString = (wsplit[1] + wsplit[2]);

                                    if (wsplit.Length > 3)
                                    {
                                        //dump warning
                                    }
                                }
                                else
                                {
                                    regString = wsplit[1];
                                }

                                Regex idx = new Regex(@"(([rR][0-9]*),\s*[$|0x|0X|-]*[0-9]+\(([rR][0-9]+)\))\s*");
                                Regex reg = new Regex(@"([rR][0-9]*),\s*[$|0x|0X|-]*[0-9]+\s*");
                                Regex sub = new Regex(@"([rR][0-9]*),\s*[\@][a-zA-Z0-9]+\s*"); //will match an @ declaration.... this should be good

                                if(idx.IsMatch(regString))
                                {
                                    string[] dirString = regString.Split(',');
                                    string[] addString = dirString[1].Split('(');

                                    s.AppendFormat("{0}", RxToBin(FileParser.TrimWhiteSpace(dirString[0])));
                                    s.AppendFormat("{0}", RxToBin(FileParser.TrimWhiteSpace(addString[1].TrimEnd(')'))));


                                    string immString = FileParser.TrimWhiteSpace(addString[0]);
                                    int c_sx = 0;
                                    string c_sx_string = "";

                                    if (immString.StartsWith("$"))
                                    {
                                        immString = immString.Remove(0, 1);
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else if (immString.ToLower().StartsWith("0x"))
                                    {
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else
                                    {
                                        c_sx = Convert.ToInt32(immString);
                                    }

                                    if (c_sx < -524288 || c_sx > 524287)
                                    {
                                        throw new ArgumentOutOfRangeException("Immediate Value out of range.");
                                    }
                                    else
                                    {
                                        c_sx_string = String.Format(Convert.ToString(c_sx, 2)).PadLeft(19, '0');
                                        if(c_sx_string.Length > 19) //remove erroneous 1s
                                        {
                                            c_sx_string = c_sx_string.Substring(c_sx_string.Length - 19, 19);
                                        }
                                    }

                                    s.AppendFormat("{0}", c_sx_string);

                                    tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString(), 2).ToString("X");
                                    return tmpInst;
                                }
                                else if(reg.IsMatch(regString))
                                {
                                    string[] dirString = regString.Split(',');
                                    s.AppendFormat("{0}0000", RxToBin(FileParser.TrimWhiteSpace(dirString[0])));

                                    string immString = FileParser.TrimWhiteSpace(dirString[1]);
                                    int c_sx = 0;
                                    string c_sx_string = "";

                                    if (immString.StartsWith("$"))
                                    {
                                        immString = immString.Remove(0, 1);
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else if (immString.ToLower().StartsWith("0x"))
                                    {
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else
                                    {
                                        c_sx = Convert.ToInt32(immString);
                                    }

                                    if (c_sx < -524288 || c_sx > 524287)
                                    {
                                        throw new ArgumentOutOfRangeException("Immediate Value out of range.");
                                    }
                                    else
                                    {
                                        c_sx_string = String.Format(Convert.ToString(c_sx, 2)).PadLeft(19, '0');
                                        if (c_sx_string.Length > 19) //remove erroneous 1s
                                        {
                                            c_sx_string = c_sx_string.Substring(c_sx_string.Length - 19, 19);
                                        }
                                    }

                                    s.AppendFormat("{0}", c_sx_string);

                                    tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString(), 2).ToString("X");
                                    return tmpInst;
                                }
                                else if(sub.IsMatch(regString))
                                {
                                    //we're matching a subroutine. save this for later.
                                    string[] dirString = regString.Split(',');
                                    tmpInst.immediateToReplace = dirString[1];
                                    tmpInst.replaceImmediateWithAddress = true;
                                    return tmpInst;
                                }
                                else
                                {
                                    throw new ArgumentException("Syntax Error in '" + inst + "' : invalid instruction format.");
                                }

                            }
                        case "st":
                            {
                                string regString;

                                if (wsplit.Length > 2)
                                {
                                    regString = (wsplit[1] + wsplit[2]);

                                    if (wsplit.Length > 3)
                                    {
                                        //dump warning
                                    }
                                }
                                else
                                {
                                    regString = wsplit[1];
                                }

                                Regex idx = new Regex(@"[$|0x|0X|-]([0-9]*[a-f|A-F]*)*\([rR][0-9]+\)\s*,\s*[rR][0-9]+");
                                Regex reg = new Regex(@"[$|0x|0X|-]*[0-9]+\s*,\s*[rR][0-9]+\s*");

                                if (idx.IsMatch(regString))
                                {
                                    string[] dirString = regString.Split(',');
                                    string[] addString = dirString[0].Split('(');

                                    s.AppendFormat("{0}", RxToBin(FileParser.TrimWhiteSpace(dirString[1])));
                                    s.AppendFormat("{0}", RxToBin(FileParser.TrimWhiteSpace(addString[1].TrimEnd(')'))));


                                    string immString = FileParser.TrimWhiteSpace(addString[0]);
                                    int c_sx = 0;
                                    string c_sx_string = "";

                                    if (immString.StartsWith("$"))
                                    {
                                        immString = immString.Remove(0, 1);
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else if (immString.ToLower().StartsWith("0x"))
                                    {
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else
                                    {
                                        c_sx = Convert.ToInt32(immString);
                                    }

                                    if (c_sx < -524288 || c_sx > 524287)
                                    {
                                        throw new ArgumentOutOfRangeException("Immediate Value out of range.");
                                    }
                                    else
                                    {
                                        c_sx_string = String.Format(Convert.ToString(c_sx, 2)).PadLeft(19, '0');
                                        if (c_sx_string.Length > 19) //remove erroneous 1s
                                        {
                                            c_sx_string = c_sx_string.Substring(c_sx_string.Length - 19, 19);
                                        }
                                    }

                                    s.AppendFormat("{0}", c_sx_string);

                                    tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString(), 2).ToString("X");
                                    return tmpInst;
                                }
                                else if (reg.IsMatch(regString))
                                {
                                    string[] dirString = regString.Split(',');
                                    s.AppendFormat("{0}0000", RxToBin(FileParser.TrimWhiteSpace(dirString[1])));

                                    string immString = FileParser.TrimWhiteSpace(dirString[0]);
                                    int c_sx = 0;

                                    string c_sx_string = "";

                                    if (immString.StartsWith("$"))
                                    {
                                        immString = immString.Remove(0, 1);
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else if (immString.ToLower().StartsWith("0x"))
                                    {
                                        c_sx = Convert.ToInt32(immString, 16);
                                    }
                                    else
                                    {
                                        c_sx = Convert.ToInt32(immString);
                                    }

                                    if (c_sx < -524288 || c_sx > 524287)
                                    {
                                        throw new ArgumentOutOfRangeException("Immediate Value out of range.");
                                    }
                                    else
                                    {
                                        c_sx_string = String.Format(Convert.ToString(c_sx, 2)).PadLeft(19, '0');
                                        if (c_sx_string.Length > 19) //remove erroneous 1s
                                        {
                                            c_sx_string = c_sx_string.Substring(c_sx_string.Length - 19, 19);
                                        }
                                    }

                                    s.AppendFormat("{0}", c_sx_string);

                                    tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString(), 2).ToString("X");
                                    return tmpInst;
                                }
                                else
                                {
                                    throw new ArgumentException("Syntax Error in '" + inst + "' : invalid instruction format.");
                                }

                            }


                        //operation,register instructions
                        case "jal":
                        case "jr":
                        case "mflo":
                        case "mfhi":
                        case "in":
                        case "out":
                            {
                                if(wsplit.Length > 2)
                                {
                                    //dump warning here
                                }

                                s.Append(RxToBin(wsplit[1]));
                                tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString().PadRight(32, '0'), 2).ToString("X");
                                return tmpInst;
                            }
                        //operation, three registers, no immediate value
                        case "add":
                        case "sub":
                        case "and":
                        case "or":
                        case "shr":
                        case "shl":
                        case "ror":
                        case "rol":
                            {
                                string[] regString;

                                if (wsplit.Length > 3)
                                {
                                    regString = (wsplit[1] + wsplit[2] + wsplit[3]).Split(',');

                                    if (wsplit.Length > 4)
                                    {
                                        //dump warning
                                    }
                                }
                                else if (wsplit.Length > 2)
                                {
                                    regString = (wsplit[1] + wsplit[2]).Split(',');
                                }
                                else
                                {
                                    regString = wsplit[1].Split(',');
                                }

                                s.AppendFormat("{0}{1}{2}", RxToBin(FileParser.TrimWhiteSpace(regString[0])), RxToBin(FileParser.TrimWhiteSpace(regString[1])), RxToBin(FileParser.TrimWhiteSpace(regString[2])));

                                tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString().PadRight(32,'0'), 2).ToString("X");
                                return tmpInst;
                            }
                        //operation, two registers, immediate value
                        case "addi":
                        case "andi":
                        case "ori":
                            {
                                string[] regString;

                                if (wsplit.Length > 3)
                                {
                                    regString = (wsplit[1] + wsplit[2] + wsplit[3]).Split(',');

                                    if (wsplit.Length > 4)
                                    {
                                        //dump warning
                                    }
                                }
                                else if (wsplit.Length > 2)
                                {
                                    regString = (wsplit[1] + wsplit[2]).Split(',');
                                }
                                else
                                {
                                    regString = wsplit[1].Split(',');
                                }

                                s.AppendFormat("{0}{1}", RxToBin(FileParser.TrimWhiteSpace(regString[0])), RxToBin(FileParser.TrimWhiteSpace(regString[1])));

                                string immString = FileParser.TrimWhiteSpace(regString[2]);
                                int c_sx = 0;

                                string c_sx_string = "";

                                if (immString.StartsWith("$"))
                                {
                                    immString = immString.Remove(0, 1);
                                    c_sx = Convert.ToInt32(immString, 16);
                                }
                                else if (immString.ToLower().StartsWith("0x"))
                                {
                                    c_sx = Convert.ToInt32(immString, 16);
                                }
                                else
                                {
                                    c_sx = Convert.ToInt32(immString);
                                }

                                if (c_sx < -524288 || c_sx > 524287)
                                {
                                    throw new ArgumentOutOfRangeException("Immediate Value out of range.");
                                }
                                else
                                {
                                    c_sx_string = String.Format(Convert.ToString(c_sx, 2)).PadLeft(19, '0');
                                    if (c_sx_string.Length > 19) //remove erroneous 1s
                                    {
                                        c_sx_string = c_sx_string.Substring(c_sx_string.Length - 19, 19);
                                    }
                                }

                                s.AppendFormat("{0}", c_sx_string);

                                tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString(), 2).ToString("X");
                                return tmpInst;
                            }
                        //operation, two registers, nothing else
                        case "not":
                        case "neg":
                        case "div":
                        case "mul":
                            {
                                string[] regString;

                                if (wsplit.Length > 2)
                                {
                                    regString = (wsplit[1]+wsplit[2]).Split(',');

                                    if (wsplit.Length > 3)
                                    {
                                        //dump warning
                                    }
                                }
                                else
                                {
                                    regString = wsplit[1].Split(',');
                                }

                                s.AppendFormat("{0}{1}", RxToBin(FileParser.TrimWhiteSpace(regString[0])), RxToBin(FileParser.TrimWhiteSpace(regString[1])));

                                tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString().PadRight(32, '0'), 2).ToString("X");
                                return tmpInst;
                            }

                        //branches
                        case "brzr":
                        case "brnz":
                        case "brpl":
                        case "brmi":
                            {
                                string[] regString;

                                if (wsplit.Length > 2)
                                {
                                    regString = (wsplit[1] + wsplit[2]).Split(',');

                                    if (wsplit.Length > 3)
                                    {
                                        //dump warning
                                    }
                                }
                                else
                                {
                                    regString = wsplit[1].Split(',');
                                }

                                s.AppendFormat("{0}{1}", RxToBin(FileParser.TrimWhiteSpace(regString[0])), "00");

                                s.Append(branchSuffix[wsplit[0]]);


                                // Constant added to end
                                string immString = FileParser.TrimWhiteSpace(regString[1]);
                                int c_sx = 0;

                                string c_sx_string = "";

                                if (immString.StartsWith("$"))
                                {
                                    immString = immString.Remove(0, 1);
                                    c_sx = Convert.ToInt32(immString, 16);
                                }
                                else if (immString.ToLower().StartsWith("0x"))
                                {
                                    c_sx = Convert.ToInt32(immString, 16);
                                }
                                else
                                {
                                    c_sx = Convert.ToInt32(immString);
                                }

                                if (c_sx < -524288 || c_sx > 524287)
                                {
                                    throw new ArgumentOutOfRangeException("Immediate Value out of range.");
                                }
                                else
                                {
                                    c_sx_string = String.Format(Convert.ToString(c_sx, 2)).PadLeft(19, '0');
                                    if (c_sx_string.Length > 19) //remove erroneous 1s
                                    {
                                        c_sx_string = c_sx_string.Substring(c_sx_string.Length - 19, 19);
                                    }
                                }

                                s.AppendFormat("{0}", c_sx_string);



                                tmpInst.outputInstructionString = Convert.ToUInt32(s.ToString(), 2).ToString("X");
                                return tmpInst;
                            }
                        default:
                            {
                                throw new ArgumentException("Unknown instruction: " + wsplit[0]);
                            }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException("Syntax Error, argument(s) are missing from '" + inst + "'");
                }

                   
            }
        }
    }
}
