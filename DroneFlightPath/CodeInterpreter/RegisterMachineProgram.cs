using System.Collections.Generic;

namespace CodeInterpreter {
  public class RPMPrimitive { }

  public class RPMVariable {
    public string Name;
    public int Value;
  }

  public class RegisterMachineProgram {
    private static int VarMemStart = 1000;
    private static int VarMemEnd = 2000;
    private readonly bool[] memoryOccupancy;

    private readonly Dictionary<string, int> variableAddresses;
    private readonly Dictionary<string, int> variableValues;
    private readonly Dictionary<string, int> labels;

    public RegisterMachineProgram() {
      variableAddresses = new Dictionary<string, int>();
      variableValues = new Dictionary<string, int>();
      memoryOccupancy = new bool[VarMemEnd - VarMemStart];
    }

    //    public void SetVariableValue(string name, int value) {
    //      if (!variableValues.ContainsKey(name))
    //        throw new ArgumentException($"Unknown variable {name}.");
    //      variableValues[name] = value;
    //    }
    //
    //    public int GetVariableValue(string name) {
    //      if (!variableValues.ContainsKey(name))
    //        throw new ArgumentException($"Unknown variable {name}.");
    //      return variableValues[name];
    //    }
    //
    //    public void AddVariable(string name, int value) {
    //      if (variableValues.ContainsKey(name))
    //        throw new ArgumentException($"Variable {name} was already added.");
    //      variableValues[name] = value;
    //      int addr = 0;
    //      while (memoryOccupancy[addr]) ++addr;
    //      if (addr == memoryOccupancy.Length)
    //        throw new InvalidOperationException($"Out of memory.");
    //      variableAddresses[name] = addr + VarMemStart;
    //      memoryOccupancy[addr] = true;
    //    }

    //    public void RemoveVariable(string name) {
    //      variableValues.Remove(name);
    //      memoryOccupancy[variableAddresses[name] - VarMemStart] = false; // set to not occupied
    //      variableAddresses.Remove(name); // this could lead to memory fragmentation
    //    }
  }
}
