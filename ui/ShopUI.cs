using Godot;
using System;

public class ShopUI : TabContainer
{
    public enum Cat
    {
        WEAPONS,
        POWERUPS,
        SPECIALS
    }

    public Shopkeeper owner;

    private AnimatedSprite icons;

    public override void _Ready()
    {
        icons=GetNode<AnimatedSprite>(nameof(AnimatedSprite));

        SetTabIcon(0,icons.Frames.GetFrame("default",7));
        SetTabIcon(1,icons.Frames.GetFrame("default",2));
        SetTabIcon(2,icons.Frames.GetFrame("default",3));
        SetTabTitle(0,"");
        SetTabTitle(1,"");
        SetTabTitle(2,"");

        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(float delta)
    {

        if(GetTree().Paused&&Visible)
        {
            Visible=false;
            HUD.instance.RemoveChild(this);
            owner.AddChild(this);
            SetPhysicsProcess(false);
        }
        else
        {
            int sum=GetTabCount();

            if(Player.instance.input.JustRight)
            {
                CurrentTab=(CurrentTab+sum+1)%sum;
            }
            else if(Player.instance.input.JustLeft)
            {
                CurrentTab=(CurrentTab+sum-1)%sum;
            }
            else if(Player.instance.input.Down)
            {
                GetCurrentTabControl().GetNode<ScrollContainer>(nameof(ScrollContainer)).ScrollVertical+=5;
            }
            else if(Player.instance.input.Up)
            {
                GetCurrentTabControl().GetNode<ScrollContainer>(nameof(ScrollContainer)).ScrollVertical-=5;
            }
            else if(Player.instance.input.JustAttack)
            {
            }
        }

    }

    public void Init()
    {
        CurrentTab=0;
        int sum=GetTabCount();
        for(int i=0;i<sum;i++)
        {
            ScrollContainer scroller=GetTabControl(i).GetNode<ScrollContainer>("ScrollContainer");
            scroller.ScrollVertical=0;
        }
    }

    public void Populate(Cat what,ShopItem item)
    {
        Control tab=GetTabControl((int)what);
        GridContainer grid=tab.GetNode<GridContainer>("ScrollContainer/GridContainer");
        grid.AddChild(item);
    }

}
