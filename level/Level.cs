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
        TileSet=World.instance.tileSet;
        CellYSort=false;
        CellCustomTransform=new Transform2D(128f,0f,0f,128f,0f,0f);
        CellQuadrantSize=8;
        ZIndex=0;

        startingPoint=GetNode<Position2D>("StartingPoint");
        startingPoint.Visible=false;

        Connect("tree_exiting",this,nameof(freeLevel));
        AddToGroup(GROUPS.LEVEL.ToString());

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