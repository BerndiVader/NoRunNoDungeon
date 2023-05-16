using Godot;
using System;

[Tool]
public class AutoTile : TileSet
{
    public override bool _IsTileBound(int drawnId, int neighborId)
    {
        return GetTilesIds().Contains(neighborId);
    }
}
