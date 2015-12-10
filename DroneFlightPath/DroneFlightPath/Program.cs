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
            var interpreter = new Interpreter(@"G:\work\TechOn2015\DroneFlightPath\DroneFlightPath\TestFile\01_letsGetToKnowEachOther_s3.txt",100);

            interpreter.Read();
        }
    }
}
