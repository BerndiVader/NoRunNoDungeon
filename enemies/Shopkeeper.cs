using Godot;
using System;

public class Shopkeeper : KinematicMonster
{
    private Vector2 shopOffset;
    private AudioStreamPlayer player;
    private ShopUI shop;

    public override void _Ready()
    {
        player=GetNode<AudioStreamPlayer>(nameof(AudioStreamPlayer));
        player.Connect("finished",this,nameof(OnMusicStop));
        player.Play();

        base._Ready();

		SetSpawnFacing();

        shop=GetNode<ShopUI>("Shop");
        shopOffset=shop.RectPosition;
        shop.owner=this;

        OnIdle();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
		goal(delta);

        if(!notifier2D.IsOnScreen())
        {
            QueueFree();
        }

    }

    protected override void Idle(float delta)
    {
        if(DistanceToPlayer()<10f)
        {
            if(!shop.Visible)
            {
                RemoveChild(shop);
                shop.RectPosition=GlobalPosition+shopOffset;
                HUD.instance.AddChild(shop);
                shop.Init();
                shop.Visible=true;
                shop.SetPhysicsProcess(true);
                World.instance.musicPlayer.Playing=false;
            }
        }
        else if(shop.Visible)
        {
            shop.SetPhysicsProcess(false);
            shop.Visible=false;
            HUD.instance.RemoveChild(shop);
            AddChild(shop);
        }

        Navigation(delta);
    }

    protected override void OnDamage(Node2D node=null,float amount=0f)
    {
        if(state!=STATE.damage&&state!=STATE.die)
        {
            return;
        }

    }

    private void OnMusicStop()
    {
        player.Play();
    }

}
