using RimWorld;
using System.Collections.Generic;

namespace RandomFactions.filters
{
    public class FactionDefNameFilter : FactionDefFilter
    {
        private HashSet<string> names;
        public FactionDefNameFilter(params string[] names)
        {
            this.names = new HashSet<string>();
            foreach(var n in names)
            {
                this.names.Add(n);
            }
        }

        public override bool Matches(FactionDef f)
        {
            return names.Contains(f.defName);
        }
    }
}
