﻿using Timberborn.SingletonSystem;

namespace TimberApi.Common.SingletonSystem
{
    [Singleton]
    public interface ITimberApiPostLoadableSingleton
    {
        void PostLoad();
    }
}