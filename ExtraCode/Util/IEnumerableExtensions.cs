namespace ExtraCode.Util;

using System;
using System.Linq;
using System.Collections.Generic;

// https://stackoverflow.com/questions/56692/random-weighted-choice
public static class EnumerableExtensions {
    public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector) {
        var sequenceList = sequence.ToList();
        var totalWeight = sequenceList.Sum(weightSelector);
        // The weight we are after...
        var itemWeightIndex =  (float)new Random().NextDouble() * totalWeight;
        float currentWeightIndex = 0;

        foreach(var item in from weightedItem in sequenceList select new { Value = weightedItem, Weight = weightSelector(weightedItem) }) {
            currentWeightIndex += item.Weight;
            // If we've hit or passed the weight we are after for this item then it's the one we want....
            if(currentWeightIndex >= itemWeightIndex)
                return item.Value;
        }
        return default;
    }
}