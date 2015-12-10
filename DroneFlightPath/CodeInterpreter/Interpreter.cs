using DroneFlightPath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInterpreter
{
    public class Interpreter
    {
        private string _filePath;
        private int _numberOfTicks;

        public Interpreter(string filePath, int numberOfTicks)
        {
            _filePath = filePath;
            _numberOfTicks = numberOfTicks;
        }
        public List<string> Read()
        {
            var moves = new List<string>();
            var lines = File.ReadAllLines(_filePath).Where(x => !string.IsNullOrEmpty(x)).ToList();

            int a = 0;
            int n = 0;
            Dictionary<int, int> memory = new Dictionary<int, int>();
            for (int i = 0; i < _numberOfTicks; i++)
            {
                var lineIndex = 0;
                bool critical = false;
                do
                {
                    var line = lines[lineIndex];
                    var interpretedLine = GetCommand(line);

                    var command = interpretedLine.Key;
                    var rawArgument = interpretedLine.Value;

                    switch (command)
                    {
                        case CpuCommands.LDA:
                            {
                                var argument = ReadArgument(memory, a, n, rawArgument);
                                a = argument;
                                ++lineIndex;
                                break;
                            }
                        case CpuCommands.LDN:
                            {
                                var argument = ReadArgument(memory, a, n, rawArgument);
                                n = argument;
                                ++lineIndex;
                                break;
                            }
                        case CpuCommands.ADDA:
                            {
                                var argument = ReadArgument(memory, a, n, rawArgument);
                                a += argument;
                                ++lineIndex;
                                break;
                            }
                        case CpuCommands.JGE:
                            {
                                var argument = ReadArgument(memory, a, n, rawArgument);
                                if (a >= 0)
                                {
                                    lineIndex = argument;
                                }
                                else
                                {
                                    ++lineIndex;
                                }
                                break;
                            }
                        case CpuCommands.STA:
                            {
                                var argument = ReadStaArgument(memory, a, n, rawArgument);
                              
                                    memory[argument] = a;
                                ++lineIndex;
                                break;
                            }
                        case CpuCommands.SUBA:
                            {
                                var argument = ReadArgument(memory, a, n, rawArgument);
                                a -= argument;
                                ++lineIndex;
                                break;
                            }
                        case CpuCommands.HLT:
                            break;
                        case CpuCommands.UNK:
                            {
                                critical = true;
                                break;
                            }
                    }
                }
                while (!critical && lines[lineIndex] != "HLT");

                moves.Add(ReadMove(memory[0]));

            }
            return moves;
        }

        private string ReadMove(int rawValue)
        {
            switch (rawValue)
            {
                case 0: return "H";
                case 1: return "U";
                case 2: return "R";
                case 3: return "D";
                case 4: return "L";
                default:
                    {
                        throw new ArgumentException("Invalid direction provided " + rawValue);
                    }
            }
        }

        private int ReadArgument(Dictionary<int, int> memory, int a, int n, string rawArgument)
        {
            if (rawArgument.StartsWith("["))
            {
                var argumentWithoutBrakets = rawArgument.Replace("[", "").Replace("]", "");
                var result = -1;

                if (int.TryParse(argumentWithoutBrakets, out result))
                {
                    if (!memory.Keys.Contains(result))
                    {
                        memory.Add(result, 0);
                    }
                    return memory[result];
                }
                else
                {
                    if (argumentWithoutBrakets == "A")
                    {
                        if (!memory.Keys.Contains(a))
                        {
                            memory.Add(a, 0);
                        }
                        return memory[a];

                    }
                    if (!memory.Keys.Contains(n))
                    {
                        memory.Add(n, 0);
                    }
                    return memory[n];

                }
            }
            else
            {
                return int.Parse(rawArgument);
            }
        }

        private int ReadStaArgument(Dictionary<int, int> memory, int a, int n, string rawArgument)
        {
            if (rawArgument.StartsWith("["))
            {
                var argumentWithoutBrakets = rawArgument.Replace("[", "").Replace("]", "");
                var result = -1;

                if (int.TryParse(argumentWithoutBrakets, out result))
                {
                    if (!memory.Keys.Contains(result))
                    {
                        memory.Add(result, 0);
                    }
                    return result;
                }
                else
                {
                    if (argumentWithoutBrakets == "A")
                    {
                        if (!memory.Keys.Contains(a))
                        {
                            memory.Add(a, 0);
                        }
                        return a;

                    }
                    if (!memory.Keys.Contains(n))
                    {
                        memory.Add(n, 0);
                    }
                    return n;

                }
            }
            else
            {
                return int.Parse(rawArgument);
            }
        }

        private KeyValuePair<CpuCommands, string> GetCommand(string line)
        {
            if (line.StartsWith("LDA"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.LDA, line.Remove(0, 4));
            }

            if (line.StartsWith("LDN"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.LDN, line.Remove(0, 4));
            }

            if (line.StartsWith("STA"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.STA, line.Remove(0, 4));
            }

            if (line.StartsWith("ADDA"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.ADDA, line.Remove(0, 5));
            }

            if (line.StartsWith("SUBA"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.SUBA, line.Remove(0, 5));
            }

            if (line.StartsWith("JGE"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.JGE, line.Remove(0, 4));
            }

            if (line.StartsWith("HLT"))
            {
                return new KeyValuePair<CpuCommands, string>(CpuCommands.HLT, "");
            }

            return new KeyValuePair<CpuCommands, string>(CpuCommands.UNK, "");
        }
    }
}
