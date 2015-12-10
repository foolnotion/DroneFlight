using CodeInterpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneFlightPath
{
    class Program
    {
        static void Main(string[] args)
        {
            var interpreter = new Interpreter();

            interpreter.Read();
        }
    }
}
