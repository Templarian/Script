using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Script
{
    public class ScriptScope
    {
        public ScriptScope(ScriptError error)
        {
            Indent = 0;
            Error = error;
        }

        private ScriptError Error { get; set; }

        public int Indent { get; set; }

        // We need to track the boolean values
        public List<bool> Depths = new List<bool>();

        public void DepthCondition(bool result)
        {
            if (Depths.Count() < Indent)
            {
                Depths.Add(result);
            }
            else
            {
                Depths[Indent] = result;
            }
        }

        public void DepthInverseCondition()
        {
            if (Depths.Count() < Indent)
            {
                Depths[Indent] = !Depths[Indent - 1];
            }
            else
            {

            }
            
        }
    }
}
