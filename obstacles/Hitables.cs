using Godot;
using System;
using System.Linq.Expressions;

public class Hitables : Area2D
{
    private static readonly AudioStream BLOCK_FX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_Battle_SFX/39_Block_03.wav");
    private static readonly AudioStream BONUS_FX=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PickUp/Retro PickUp Coin 07.wav");

    private const double TIMEOUT=0.1d;

    [Export] private int LOAD=3;

    private bool active=true;
    private double lastTriggered=0d;

    private ImageTexture texture;
    private Sprite marker;

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

        marker=GetNode<Sprite>(nameof(Sprite));

        Vector2 local=World.level.ToLocal(GlobalPosition);
        Vector2 tile=World.level.WorldToMap(local);

        int id=World.level.GetCellv(tile);
        if(id!=TileMap.InvalidCell)
        {
            texture=World.level.CreateTextureForTile(id,tile);
            marker.Texture=texture;
        }
        marker.ZIndex=World.level.ZIndex+1;
        marker.Visible=true;
        marker.Modulate=new Color(1f,1f,1f,0f);

        Alert alert=ResourceUtils.particles[(int)PARTICLES.ALERT].Instance<Alert>();
        alert.chr="?"[0];
        AddChild(alert);        

    }

    private void OnBodyExited(Node node)
    {
        if(!active&&(node is Player))
        {
            active=true;
        }
    }

    private void OnBodyEntered(Node node)
    {
        if(active&&(node is Player player))
        {
            if(player.GlobalPosition.y>GlobalPosition.y)
            {
                if(Mathf.Abs(player.GlobalPosition.x-GlobalPosition.x)<8f)
                {
                    double now=Time.GetUnixTimeFromSystem();
                    if(now-lastTriggered>TIMEOUT)
                    {
                        LOAD--;
                        if(LOAD>0)
                        {
                            FlashMarker();
                        }
                        else
                        {
                            CoinTakenParticles particles=ResourceUtils.particles[(int)PARTICLES.COINTAKEN].Instance<CoinTakenParticles>();
                            particles.Position=Position;
                            World.level.AddChild(particles);
                            Renderer.instance.Shake(1.5f);
                            Renderer.instance.PlaySfx(BONUS_FX,Position);
                            player.ApplyCoins(1);
                            CallDeferred("queue_free");
                        }
                        active=false;
                        lastTriggered=now;
                    }
                }
            }
        }
    }

    private async void FlashMarker()
    {
        Renderer.instance.PlaySfx(BLOCK_FX,Position);
        Renderer.instance.Shake(1f);
        marker.Modulate=new Color(1,1,1,1f);
        await ToSignal(GetTree().CreateTimer(0.11f),"timeout");
        marker.Modulate=new Color(1f,1f,1f,0f);
    }    

}
