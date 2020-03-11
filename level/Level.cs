using Godot;
using System;

public class Level : TileMap
{
    [Export] public float Speed=120f;
    [Export] public Vector2 direction=new Vector2(-1f,0f);
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
        TileSet=WorldUtils.world.tileSet;

        Connect("tree_exiting",this,nameof(freeLevel));
        ZIndex=0;
        AddToGroup("Level");
    }

    public void freeLevel() 
    {
        foreach(Node node in GetChildren()) 
        {
            if(node!=null&&!node.IsQueuedForDeletion()) node.CallDeferred("queue_free");
        }
        CallDeferred("queue_free");
    }

    new public void SetCell(int x,int y,int tile,bool flipX=false,bool flipY=false,bool transpose=false,Vector2? autotileCoord=null) 
    {
        base.SetCell(x,y,tile,flipX,flipY,transpose,autotileCoord);
    }

}