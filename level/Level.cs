using System.Collections.Generic;
using Godot;

public class Level : TileMap
{
    [Export] public bool KEEP_TILESET=false;
    [Export] public float speed=120f;
    [Export] public Vector2 direction=Vector2.Left;
    [Export(PropertyHint.Range,"0.5,1.5,0.1")] public float zoom=1f;

    public int mapLength;
    public Rect2 usedRect2;
    public Vector2 pixelHeight;
    public int pixelLength;
    public Vector2 startingPoint;
    public Settings settings;
    public Settings DEFAULT_SETTING;
    public Stack<Settings>modifiers=new Stack<Settings>();

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        PlayerCamera.instance.Zoom=Vector2.One;

        if(!KEEP_TILESET)
        {
            TileSet=World.instance.tileSet;
        }

        usedRect2=GetUsedRect();

        mapLength=(int)usedRect2.End.x;
        pixelLength=mapLength*16;
        pixelHeight=LevelHeight()*16;
        CellYSort=false;
        CellCustomTransform=new Transform2D(128f,0f,0f,128f,0f,0f);
        CellQuadrantSize=8;
        ZIndex=0;

        Position2D pos=GetNode<Position2D>("StartingPoint");
        startingPoint=pos.GlobalPosition;
        pos.QueueFree();
        
        Connect("tree_exiting",this,nameof(FreeLevel));
        AddToGroup(GROUPS.LEVEL.ToString());

        Godot.Collections.Dictionary<string,object>settingsList=Settings.DEFAULT_LEVEL_SETTINGS.Duplicate();
        settingsList["Speed"]=speed;
        settingsList["Dir"]=direction;
        settingsList["Zoom"]=zoom;
        settingsList["AutoRestore"]=false;

        settings=new Settings(this,settingsList);
        DEFAULT_SETTING=new Settings(this,settingsList);
        settings.Set();
    }

    public void FreeLevel() 
    {
        if(!IsQueuedForDeletion())
        {
            Settings.Stop();

            var nodes=GetChildren();
            foreach(Node node in nodes)
            {
                if(IsInstanceValid(node)&&!node.IsQueuedForDeletion())
                {
                    node.CallDeferred("queue_free");
                }
            }
            CallDeferred("queue_free");
        }
    }

    public void Terraform(Vector2 delta)
    {
        Vector2[]tiles=new Vector2[]
        {
            delta+Vector2.Up,
            delta+Vector2.Down,
            delta+Vector2.Left,
            delta+Vector2.Right,
            delta+Vector2.Up+Vector2.Left,
            delta+Vector2.Up+Vector2.Right,
            delta+Vector2.Down+Vector2.Left,
            delta+Vector2.Down+Vector2.Right
        };

        foreach(Vector2 tile in tiles)
        {
            int id=GetCellv(tile);
            SetCellv(tile,id);
            UpdateBitmaskArea(tile);
        }
    }

    public Image CreateImageForTile(int id,Vector2 tile)
    {
        Vector2 subtile=GetCellAutotileCoord((int)tile.x,(int)tile.y);
        Texture tileset=TileSet.TileGetTexture(id);
        Rect2 region=new Rect2(subtile.x*16f,subtile.y*16f,16f,16f);
        return tileset.GetData().GetRect(region);
    }

    public ImageTexture CreateTextureForTile(int id,Vector2 tile)
    {
        ImageTexture texture=new ImageTexture();
        texture.CreateFromImage(CreateImageForTile(id,tile));
        texture.Flags=0;
        return texture;
    }

    private Vector2 LevelHeight()
    {
        return new Vector2((int)usedRect2.Position.y,(int)(usedRect2.Size.y-1f));
    }

    new public void SetCell(int x,int y,int tile,bool flipX=false,bool flipY=false,bool transpose=false,Vector2? autotileCoord=null) 
    {
        base.SetCell(x,y,tile,flipX,flipY,transpose,autotileCoord);
    }

    public void OnSettingsTweenCompleted()
    {
        Settings.NewPropertyTween(this);
    }

}