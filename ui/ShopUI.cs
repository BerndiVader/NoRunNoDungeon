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
        SetTabIcon(1,icons.Frames.GetFrame("default",3));
        SetTabTitle(0,"");
        SetTabTitle(1,"");

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
        }
    }


}
