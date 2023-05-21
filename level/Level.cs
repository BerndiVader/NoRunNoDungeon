using Godot;
using System;

public class Level : TileMap
{
    [Export] public float Speed=120f;
    [Export] public Vector2 direction=new Vector2(-1f,0f);
    [Export] public bool KeepTileset=false;
    public int mapLength;
    public int pixelLength;
    public Vector2 startingPoint;
    public Settings settings;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        PlayerCamera.instance.Zoom=new Vector2(1f,1f);

        if(!KeepTileset)
        {
            TileSet=World.instance.tileSet;
        }

        mapLength=((int)GetUsedRect().End.x)-1;
        pixelLength=mapLength*(int)this.CellSize.x;
        CellYSort=false;
        CellCustomTransform=new Transform2D(128f,0f,0f,128f,0f,0f);
        CellQuadrantSize=8;
        ZIndex=0;

        Position2D pos=GetNode<Position2D>("StartingPoint");
        startingPoint=pos.GlobalPosition;
        pos.QueueFree();
        
        Connect("tree_exiting",this,nameof(freeLevel));
        AddToGroup(GROUPS.LEVEL.ToString());

        settings=new Settings(this);
    }

    public void freeLevel() 
    {
        if(!IsQueuedForDeletion())
        {
            foreach(Node node in GetChildren())
            {
                if(node!=null&&!node.IsQueuedForDeletion())
                {
                    node.CallDeferred("queue_free");
                }
            }
            CallDeferred("queue_free");
        }
    }

    new public void SetCell(int x,int y,int tile,bool flipX=false,bool flipY=false,bool transpose=false,Vector2? autotileCoord=null) 
    {
        base.SetCell(x,y,tile,flipX,flipY,transpose,autotileCoord);
    }

}