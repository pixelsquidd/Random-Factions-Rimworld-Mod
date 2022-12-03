using RimWorld;
using System.Collections.Generic;

public abstract class FactionDefFilter
{
	public FactionDefFilter()
	{
	}

	public abstract bool Matches(FactionDef f);

	public static List<RimWorld.FactionDef> filterFactionDefs(IEnumerable<FactionDef> allFactionDefs, params FactionDefFilter[] filters)
	{
		List<RimWorld.FactionDef> output = new List<RimWorld.FactionDef>();
		foreach (FactionDef fac in allFactionDefs)
		{
			bool matches = true;
			foreach(FactionDefFilter filter in filters)
			{
				if (!filter.Matches(fac))
				{
					matches = false;
					break;
				}
			}
			if (matches)
			{
				output.Add(fac);
			}
		}
		return output;
    }
}