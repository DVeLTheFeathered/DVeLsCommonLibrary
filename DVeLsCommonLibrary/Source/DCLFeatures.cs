using System;
using System.Collections.Generic;
using Verse;

namespace DVeLsCommonLibrary
{
    public static class DCLFeatures
    {
        private static readonly List<DCLFeature> _requestedFeatures = new List<DCLFeature>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requester">Preferably ModContentPack.PackageIdPlayerFacing, but can be anything</param>
        /// <param name="features"></param>
        public static void RequestFeatures(string requester = null, params List<DCLFeature> features)
        {
            Log.Message($"DVeL.CommonLibrary. {requester ?? "Someone"} requests features: {string.Join(", ", features)}");
            _requestedFeatures.AddRangeUnique(features);
        }

        internal static bool FeatureRequested(DCLFeature feature)
        {
            return _requestedFeatures.Contains(feature);
        }
    }
}
