using System;

namespace esharp
{
    public class Parser
    {



        public String[] ParseLine(String line)
        {
            return line.Trim().Split(' ');
        }

        public String ParsePragamaVersion()
        {
            return "4.0.0";
        }

        public void ParseContractKind()
        {
            
        }
    }
}
