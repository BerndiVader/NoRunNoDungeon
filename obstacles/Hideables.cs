using Godot;
using System;
using System.Threading.Tasks;

public class Hideables : Area2D,ISwitchable
{
    private static readonly PackedScene EXPLODER_PACK=ResourceLoader.Load<PackedScene>("res://gfx/TileExploder.tscn");

    [Export] private bool TERRAFORM=true;
    [Export] private bool NOT_PLAYER=false;
    [Export] private string switchID="";

    private int tileID=TileMap.InvalidCell;
    private Vector2 pos=Vector2.Zero;
    private Vector2 localPos=Vector2.Zero;

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

            Alert alert=ResourceUtils.particles[(int)PARTICLES.ALERT].Instance<Alert>();
            alert.chr="+"[0];
            AddChild(alert);
        }
        else
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }

        GetAndHideTile();

    }

    private void GetAndHideTile()
    {
        Vector2 local=World.level.ToLocal(GlobalPosition);
        pos=World.level.WorldToMap(local);
        tileID=World.level.GetCellv(pos);

        if(tileID!=TileMap.InvalidCell)
        {
            World.level.SetCellv(pos,-1);
            World.level.Terraform(pos);
        }


    }

    private void OnDamage(Node2D node=null,float amount=0f)
    {
        if(NOT_PLAYER&&node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            return;
        }

        if(tileID!=TileMap.InvalidCell)
        {
            World.level.SetCellv(pos,tileID);
            if(TERRAFORM)
            {
                World.level.Terraform(pos);
            }

        }

        CallDeferred("queue_free");
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
