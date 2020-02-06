using Godot;
using System;

public class Level : TileMap
{
    [Export] public float xSpeed=70f;
    public int mapLength;
    public int pixelLength;

    public Position2D startingPoint;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        mapLength=((int)GetUsedRect().End.x)-1;
        pixelLength=mapLength*(int)this.CellSize.x;
        startingPoint=(Position2D)GetNode("StartingPoint");
        this.TileSet=WorldUtils.world.tileSet;
    }

    new public void SetCell(int x,int y,int tile,bool flipX=false,bool flipY=false,bool transpose=false,Vector2? autotileCoord=null) {
        base.SetCell(x,y,tile,flipX,flipY,transpose,autotileCoord);
    }

}
