using Godot;
using System;

public class Hitables : Area2D
{
    private Alert alert;
    private ImageTexture texture;
    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen), new Godot.Collections.Array(this));
        AddChild(notifier2D);

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("body_exited",this,nameof(OnBodyExited));

        Vector2 local=World.level.ToLocal(GlobalPosition);
        Vector2 tile=World.level.WorldToMap(local);

        int id=World.level.GetCellv(tile);
        if(id!=TileMap.InvalidCell)
        {
            texture=ExtractTexture(id,tile);
        }

    }

    private void OnBodyExited(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            SetDeferred("monitoring",true);
        }
    }

    private void OnBodyEntered(Node node)
    {
        if(!node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            return;
        }

        SpawnBumpParticle();

        SetDeferred("monitoring",false);

    }

    private void SpawnBumpParticle()
    {
        CPUParticles2D particles=new CPUParticles2D();
        particles.Amount=1;
        particles.Texture=texture;
        particles.Direction=Vector2.Up;
        particles.Spread=0.0f;
        particles.InitialVelocity=260f;
        particles.Gravity=new Vector2(0f,-60f);

        var ramp=new Gradient();
        ramp.AddPoint(0f,new Color(1,1,1,1));
        ramp.AddPoint(1f,new Color(1,1,1,0));
        particles.ColorRamp=ramp;

        particles.Lifetime=0.6f;
        particles.OneShot=true;
        particles.Emitting=true;

        AddChild(particles);
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

}
