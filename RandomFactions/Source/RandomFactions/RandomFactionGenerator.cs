using RandomFactions;
using RandomFactions.filters;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

public class RandomFactionGenerator
{
	private System.Random prng;
	private RimWorld.Planet.World world;
	private List<RimWorld.FactionDef> definedFactionDefs = new List<RimWorld.FactionDef>();
    private bool hasRoyalty = false;
    private bool hasIdeology = false;
    private bool hasBiotech = false;
    //private RandFacDataStore dataStore;

    HugsLib.Utils.ModLogger Logger;
    public RandomFactionGenerator(RimWorld.Planet.World world, IEnumerable<FactionDef> allFactionDefs, bool hasRoyaltyExpansion, bool hasIdeologyExpansion, bool hasBiotechExpansion,  HugsLib.Utils.ModLogger logger)
	{
		// init globals
		this.Logger = logger;
		this.world = world;
        //this.dataStore = dataStore;
        this.hasBiotech = hasBiotechExpansion;
        this.hasRoyalty = hasRoyaltyExpansion;
        this.hasIdeology= hasIdeologyExpansion;
        System.Random seeder = new System.Random(world.ConstantRandSeed);
		byte[] seed_buffer = new byte[4];
		seeder.NextBytes(seed_buffer);
		int seed = BitConverter.ToInt32(seed_buffer, 0);
		this.prng = new System.Random(seed);
		// load existing faction definitions
		foreach (var def in allFactionDefs) {
            if (def.categoryTag.EqualsIgnoreCase(RandomFactionsMod.RANDOM_CATEGORY_NAME)) { continue; } // skip factions from this mod
            this.definedFactionDefs.Add(def); 
        }
        logger.Trace(string.Format("RandomFactionGenerator constructed with random number seed {0}", world.ConstantRandSeed));
    }

    public Faction replaceWithRandomNonHiddenFaction(Faction faction)
    {
        var priorFactions = this.world.factionManager.AllFactions;
        var newFaction = randomNPCFaction(priorFactions);
        return replaceFaction(faction, newFaction, priorFactions);
    }
    public Faction replaceWithRandomNonHiddenEnemyFaction(Faction faction)
    {
        var priorFactions = this.world.factionManager.AllFactions;
        var newFaction = randomEnemyFaction(priorFactions);
        return replaceFaction(faction, newFaction, priorFactions);
    }
    public Faction replaceWithRandomNonHiddenWarlordFaction(Faction faction)
    {
        var priorFactions = this.world.factionManager.AllFactions;
        var newFaction = randomRoughFaction(priorFactions);
        return replaceFaction(faction, newFaction, priorFactions);
    }
    public Faction replaceWithRandomNonHiddenTraderFaction(Faction faction)
    {
        var priorFactions = this.world.factionManager.AllFactions;
        var newFaction = randomNeutralFaction(priorFactions);
        return replaceFaction(faction, newFaction, priorFactions);
    }
    public Faction replaceWithRandomNamedFaction(Faction faction, params string[] validDefNames)
    {
        var priorFactions = this.world.factionManager.AllFactions;
        var newFaction = randomNamedFaction(priorFactions, validDefNames);
        return replaceFaction(faction, newFaction, priorFactions);
    }

    private Faction replaceFaction(Faction oldFaction, Faction newFaction, IEnumerable<Faction> priorFactions)
    {
        Logger.Message(string.Format("Replacing faction {0} ({1}) with faction {2} ({3})", oldFaction.Name, oldFaction.def.defName, newFaction.Name, newFaction.def.defName));
        var ignoreRelation = new FactionRelation(oldFaction, FactionRelationKind.Neutral);
        ignoreRelation.baseGoodwill = 0;
        foreach (var faction in priorFactions)
        {
            if (faction.IsPlayer) continue;
            if (faction.Equals(oldFaction)) continue;
            faction.SetRelation(ignoreRelation);
        }
        foreach(var stl in Find.WorldObjects.Settlements)
        {
            if (stl.Faction.Equals(oldFaction))
            {
                stl.SetFaction(newFaction);
            }
        }
        oldFaction.defeated = true;
        this.world.factionManager.Add(newFaction);
        return newFaction;
    }

    private int countFactionsOfType(FactionDef def, IEnumerable<Faction> factions)
    {
        int count = 0;
        foreach(var f in factions)
        {
            if(f.def.defName == def.defName)
            {
                count++;
            }
        }
        return count;
    }

    private RimWorld.FactionDef drawRandomFactionDef(List<RimWorld.FactionDef> fdefList, IEnumerable<RimWorld.Faction> existingFactions)
    {
        FactionDef fdef = null;
        int retyLimit = 100;
        while (--retyLimit > 0)
        {
            fdef = fdefList[this.prng.Next(fdefList.Count)];
            int count = countFactionsOfType(fdef, existingFactions);
            if (fdef.maxCountAtGameStart <= 0 || count < fdef.maxCountAtGameStart)
            {
                break;
            }
        }
        return fdef;
    }

    public Faction randomNPCFaction(IEnumerable<RimWorld.Faction> existingFactions)
	{
        var fdefList = FactionDefFilter.filterFactionDefs(this.definedFactionDefs, 
            new PlayerFactionDefFilter(false), new HiddenFactionDefFilter(false)
            );
        var fdef = drawRandomFactionDef(fdefList, existingFactions);
        return generateFactionFromDef(fdef, existingFactions);
    }

    public Faction randomEnemyFaction(IEnumerable<RimWorld.Faction> existingFactions)
    {
        var fdefList = FactionDefFilter.filterFactionDefs(this.definedFactionDefs,
            new PlayerFactionDefFilter(false), new HiddenFactionDefFilter(false), 
            new PermanentEnemyFactionDefFilter(true)
            );
        var fdef = drawRandomFactionDef(fdefList, existingFactions);
        return generateFactionFromDef(fdef, existingFactions);
    }

    public Faction randomRoughFaction(IEnumerable<RimWorld.Faction> existingFactions)
    {
        var fdefList = FactionDefFilter.filterFactionDefs(this.definedFactionDefs,
            new PlayerFactionDefFilter(false), new HiddenFactionDefFilter(false),
            new PermanentEnemyFactionDefFilter(false), new NaturalEnemyFactionDefFilter(true)
            );
        var fdef = drawRandomFactionDef(fdefList, existingFactions);
        return generateFactionFromDef(fdef, existingFactions);
    }

    public Faction randomNeutralFaction(IEnumerable<RimWorld.Faction> existingFactions)
    {
        var fdefList = FactionDefFilter.filterFactionDefs(this.definedFactionDefs,
            new PlayerFactionDefFilter(false), new HiddenFactionDefFilter(false),
            new PermanentEnemyFactionDefFilter(false), new NaturalEnemyFactionDefFilter(false)
            );
        var fdef = drawRandomFactionDef(fdefList, existingFactions);
        return generateFactionFromDef(fdef, existingFactions);
    }

    public Faction randomNamedFaction(IEnumerable<RimWorld.Faction> existingFactions, params string[] nameList)
    {
        var fdefList = FactionDefFilter.filterFactionDefs(this.definedFactionDefs,
            new PlayerFactionDefFilter(false), new FactionDefNameFilter(nameList)
            );
        var fdef = drawRandomFactionDef(fdefList, existingFactions);
        return generateFactionFromDef(fdef, existingFactions);
    }

    public Faction generateFactionFromDef(FactionDef def, System.Collections.Generic.IEnumerable<RimWorld.Faction> existingFactions)
	{
        var relations = defaultRelations(def, existingFactions);
        Faction fac = FactionGenerator.NewGeneratedFactionWithRelations(def, relations, def.hidden);
		return fac;
    }

    private int defaultGoodwill(FactionDef def)
    {
        if (def.categoryTag.EqualsIgnoreCase(RandomFactionsMod.RANDOM_CATEGORY_NAME)) return 0;
        if (def.permanentEnemy) return -100;
        if (def.naturalEnemy) return -80;
        return 0;
    }
    private int defaultGoodwill(Faction fac)
    {
        if (fac.def.categoryTag.EqualsIgnoreCase(RandomFactionsMod.RANDOM_CATEGORY_NAME)) return 0;
        int defGW = defaultGoodwill(fac.def);
        return defGW == 0 ? fac.NaturalGoodwill : defGW;
    }

    private List<FactionRelation> defaultRelations(FactionDef target, IEnumerable<Faction> allFactions)
    {
        var relList = new List<FactionRelation>();
        foreach(var fac in allFactions)
        {
            if (fac.IsPlayer) continue;
            int initGW = lowestOf(defaultGoodwill(target), defaultGoodwill(fac));
        }
        return relList;
    }

    private static int lowestOf(params int[] n)
    {
        int lowest = n[0];
        foreach (int i in n)
        {
            if (i < lowest) lowest = i;
        }
        return lowest;
    }

    private static int highestOf(params int[] n)
    {
        int highest = n[0];
        foreach (int i in n)
        {
            if (i > highest) highest = i;
        }
        return highest;
    }

}
