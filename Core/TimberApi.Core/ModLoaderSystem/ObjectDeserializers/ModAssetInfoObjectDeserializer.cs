﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimberApi.Common.Extensions;
using TimberApi.SceneSystem;
using Timberborn.Persistence;

namespace TimberApi.Core.ModLoaderSystem.ObjectDeserializers
{
    internal class ModAssetInfoObjectDeserializer : IObjectSerializer<ModAssetInfo>
    {
        public void Serialize(ModAssetInfo value, IObjectSaver objectSaver)
        {
        }

        public Obsoletable<ModAssetInfo> Deserialize(IObjectLoader objectLoader)
        {
            string prefix = objectLoader.Get(new PropertyKey<string>("Prefix"));
            SceneEntrypoint sceneEntrypoint = ConvertToFlag(objectLoader.Get(new ListKey<string>("Scenes")).Select(Enum.Parse<SceneEntrypoint>));

            string path = PathToCrossPlatformPath(objectLoader.GetValueOrDefault(new PropertyKey<string>("Path"), "assets"));
            return new ModAssetInfo(prefix, sceneEntrypoint, path);
        }

        private static T ConvertToFlag<T>(IEnumerable<T> flags) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException($"{typeof(T)} must be an enumerated type");
            }

            return (T) (object) flags.Cast<int>().Aggregate(0, (c, n) => c |= n);
        }

        private static string PathToCrossPlatformPath(string path)
        {
            return Path.Combine(path.Split("/"));
        }
    }
}