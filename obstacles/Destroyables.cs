using Godot;
using System;

public class Destroyables : Area2D,ISwitchable
{
    private static readonly PackedScene ExpolderPack=ResourceLoader.Load<PackedScene>("res://gfx/TileExploder.tscn");

    [Export] private bool terraform=true;
    [Export] private bool notPlayer=false;
    [Export] private string switchID="";
    private Alert alert;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen), new Godot.Collections.Array(this));
        AddChild(notifier2D);

        if(switchID=="")
        {
            AddUserSignal(STATE.damage.ToString());
            Connect(STATE.damage.ToString(),this,nameof(OnDamage));

            alert=ResourceUtils.particles[(int)PARTICLES.ALERT].Instance<Alert>();
            alert.chr="!"[0];
            AddChild(alert);
        }
        else
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }

    }

    private void OnDamage(Node2D node=null, int amount=0)
    {
        if(notPlayer&&node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            return;
        }
        Vector2 local=World.level.ToLocal(GlobalPosition);
        Vector2 tile=World.level.WorldToMap(local);

        int id=World.level.GetCellv(tile);
        if(id!=TileMap.InvalidCell)
        {
            ImageTexture texture=ExtractTexture(id,tile);
            if(texture!=null)
            {
                TileExploder exploder=ExpolderPack.Instance<TileExploder>();
                exploder.Texture=texture;
                exploder.Position=local;
                World.level.AddChild(exploder);
            }
        }

        World.level.SetCellv(tile,-1);
        if(terraform)
        {
            Terraform(tile);
        }
        
        CallDeferred("queue_free");
    }

    private static ImageTexture ExtractTexture(int id,Vector2 tile)
    {
        Vector2 subtile=World.level.GetCellAutotileCoord((int)tile.x,(int)tile.y);
        Texture tileset=World.level.TileSet.TileGetTexture(id);
        Rect2 region=new Rect2(subtile.x*16f,subtile.y*16f,16f,16f);

        Image image=tileset.GetData().GetRect(region);
        Image big=new Image();
        big.Create(64,64,false,Image.Format.Rgba8);
        big.Fill(new Color(0,0,0,0));
        big.BlitRect(image,new Rect2(0f,0f,16f,16f),new Vector2(24f,24f));

        ImageTexture texture=new ImageTexture();
        texture.CreateFromImage(big);
        texture.Flags=0;
        return texture;
    }

    private static void Terraform(Vector2 delta)
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
            int id=World.level.GetCellv(tile);
            if(id!=TileMap.InvalidCell)
            {
                World.level.SetCellv(tile,id);
            }
        }
    }

    public void SwitchCall(string id)
    {
        if(switchID==id)
        {
            switchID="";
            OnDamage();
        }
    }
}
