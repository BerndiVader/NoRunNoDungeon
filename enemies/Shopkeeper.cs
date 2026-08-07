using Godot;
using System;

public class Shopkeeper : KinematicMonster
{
    private static readonly PackedScene itemPack=ResourceLoader.Load<PackedScene>("res://ui/ShopItem.tscn");

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
        shop.Visible=false;
        shop.owner=this;
        shopOffset=shop.RectPosition;
        PopulateShop();
        
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
                shop.Visible=true;
                shop.SetPhysicsProcess(true);
                World.instance.musicPlayer.StreamPaused=true;
                shop.Init();
            }
        }
        else if(shop.Visible)
        {
            shop.SetPhysicsProcess(false);
            shop.Visible=false;
            HUD.instance.RemoveChild(shop);
            AddChild(shop);
        }
        else
        {
            if(!World.instance.musicPlayer.StreamPaused)
            {
                World.instance.musicPlayer.StreamPaused=true;
            }
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

    private void PopulateShop()
    {
        for(int i=0;i<20;i++)
        {
            ShopItem item=itemPack.Instance<ShopItem>();
            shop.Populate(ShopUI.Cat.WEAPONS,item);
            item=itemPack.Instance<ShopItem>();
            shop.Populate(ShopUI.Cat.SPECIALS,item);
            item=itemPack.Instance<ShopItem>();
            shop.Populate(ShopUI.Cat.POWERUPS,item);
        }
        
    }
}
