﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Timberborn.Goods;
using Timberborn.MechanicalSystem;
using Timberborn.PreviewSystem;
using Timberborn.Workshops;
using TimberbornAPI.Internal;
using UnityEngine;

namespace TimberbornAPI.SpecificationSystem.Buildings
{
    /// <summary>
    /// Patch recipes in ManufactoryInventoryInitializer.Initialize, so that the Manufactory
    /// Inventory get's initialized correctly too
    /// </summary>
    [HarmonyPatch(typeof(ManufactoryInventoryInitializer), nameof(ManufactoryInventoryInitializer.Initialize))]
    public static class ManufactoryInventoryInitializerPatch
    {
        public static void Prefix(Manufactory subject)
        {
            var specificationService = TimberAPI.DependencyContainer.GetInstance<BuildingSpecificationService>();
            var recipes = specificationService.GetRecipesByManufactory(subject);
            if (recipes == null)
            {
                return;
            }

            subject._productionRecipeIds = recipes.Where(x => x.Id != "")
                                                  .Select(x => x.Id)
                                                  .ToArray();
        }
    }

    /// <summary>
    /// Changing building component stuff here just works. 
    /// </summary>
    [HarmonyPatch(typeof(PreviewFactory), nameof(PreviewFactory.Create))]
    public static class PreviewFactoryPatch
    {
        public static void Prefix(ref GameObject prefab)
        {
            var specificationService = TimberAPI.DependencyContainer.GetInstance<BuildingSpecificationService>();
            if (prefab.TryGetComponent<Timberborn.Buildings.Building>(out var buildingComponent))
            {
                var building = specificationService.GetBuildingByBuilding(buildingComponent);
                if (building != null)
                {
                    buildingComponent._scienceCost = building.ScienceCost;

                    /// HACK: Reverse BuildingCost List so that custom costs are first, and so are kept
                    var reverseList = new List<BuildingCost>(building.BuildingCost);
                    reverseList.Reverse();

                    buildingComponent._buildingCost = reverseList.Select(x => new GoodAmountSpecification(x.GoodId, x.Amount))
                                                                 .Distinct(new GoodAmountSpecificationComparer())
                                                                 .ToArray();
                }
            }

            if(prefab.TryGetComponent<MechanicalNodeSpecification>(out var mechanicalNodeSpec))
            {
                var mechanicalNode = specificationService.GetMechanicalNodeByMechanicalNodeSpecification(mechanicalNodeSpec);
                if(mechanicalNode != null)
                {
                    mechanicalNodeSpec._powerInput = mechanicalNode.PowerInput;
                    mechanicalNodeSpec._powerOutput = mechanicalNode.PowerOutput;
                }
            }
        }
    }

    /// <summary>
    /// Custom EqualityComparer to detect duplicate BuildingCosts. Equality is based on GoodId
    /// </summary>
    class GoodAmountSpecificationComparer : IEqualityComparer<GoodAmountSpecification>
    {
        public bool Equals(GoodAmountSpecification gas1, GoodAmountSpecification gas2)
        {
            //Console.WriteLine($"good1: {gas1.GoodId}: {gas1.Amount}. good2: {gas2.GoodId}: {gas2.Amount}.");
            if (gas1.Equals(default(GoodAmountSpecification)) && gas2.Equals(default(GoodAmountSpecification)))
            {
                return true;
            }
            else if (gas1.Equals(default(GoodAmountSpecification)) || gas2.Equals(default(GoodAmountSpecification)))
            {
                return false;
            }
            else if (gas1.GoodId == gas2.GoodId)
            {
                TimberAPIPlugin.Log.LogWarning($"Duplicate BuildingCost detected. Keeping \"GoodId: {gas1.GoodId}\". Cost: {gas1.Amount}");
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(GoodAmountSpecification gas)
        {
            int hCode = gas.GoodId.GetHashCode();
            return hCode.GetHashCode();
        }
    }
}
