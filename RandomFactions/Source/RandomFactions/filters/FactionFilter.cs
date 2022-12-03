using RimWorld;
using System.Collections.Generic;

public abstract class FactionFilter
{
	public FactionFilter()
	{
	}

	public abstract bool Matches(Faction f);

	public static List<RimWorld.Faction> filterFactions(IEnumerable<Faction> allFactions, params FactionFilter[] filters)
	{
		List<RimWorld.Faction> output = new List<RimWorld.Faction>();
		foreach (Faction fac in allFactions)
		{
			bool matches = true;
			foreach(FactionFilter filter in filters)
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