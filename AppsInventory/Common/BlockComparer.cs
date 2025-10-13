using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInventory.Common
{
    internal class BlockComparer : IComparer<IMyTerminalBlock>
    {
        public int Compare(IMyTerminalBlock block1, IMyTerminalBlock block2)
        {
            return block1.CustomName.CompareTo(block2.CustomName);
        }
    }
}
