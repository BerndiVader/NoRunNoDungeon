using Godot;
using System;

#if TOOLS
[Tool]
public class AutoTile : TileSet
{
    public override bool _IsTileBound(int drawnId, int neighborId)
    {
        return GetTilesIds().Contains(neighborId);
    }

}
#else

public class AutoTile : TileSet {}

#endif