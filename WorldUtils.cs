using Godot;
using System;
using System.Collections.Generic;

public static class WorldUtils
{
    static World world;

    public static World getWorld() {
        return world;
    }
    public static void setworld(World world) {
        WorldUtils.world=world;
    }

    public static void mergeMaps(Level newLevel, Level nextLevel) {

        int x=33,y=18;
        int lx=((int)newLevel.GetUsedRect().End.x);

        for(int xx=0;xx<x;xx++) {
            for(int yy=0;yy<y;yy++){
                Vector2 autoTile=nextLevel.GetCellAutotileCoord(xx,yy);
                newLevel.SetCell(lx+xx,yy,nextLevel.GetCell(xx,yy),false,false,false,autoTile);
            }
        }

    }

}