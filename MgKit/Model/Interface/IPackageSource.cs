﻿namespace MgKit.Model.Interface
{
    public interface IPackageSource
    {
        string Location { get; }
        string Name { get; }
        string Provider { get; }
    }
}
