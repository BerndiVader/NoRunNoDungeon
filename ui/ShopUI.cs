using Godot;
using System;

public class ShopUI : TabContainer
{
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

        SetProcess(false);
        SetProcessInput(false);
        SetPhysicsProcess(true);
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
    }

    public void Init()
    {
        CurrentTab=0;
        int size=GetTabCount();
        for(int i=0;i<size;i++)
        {
            ScrollContainer scroller=GetTabControl(i).GetNode<ScrollContainer>("ScrollContainer");
            scroller.ScrollVertical=0;
        }
    }


}
