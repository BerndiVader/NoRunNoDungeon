using Godot;
using System;

public class Solid : Node2D
{
    private enum SOLID_TYPE
    {
        SLIME,
        WATER
    }

    [Export] private SOLID_TYPE SOLID=SOLID_TYPE.SLIME;

    private static readonly PackedScene SURFACE_SLIME_PACK=ResourceLoader.Load<PackedScene>("res://level/SlimeSurface.tscn");
    private static readonly PackedScene SURFACE_WATER_PACK=ResourceLoader.Load<PackedScene>("res://level/WaterSurface.tscn");
    private VisibilityNotifier2D notifier;
    private AnimatedSprite sprite;
    private Area2D area;

    public override void _Ready()
    {
        sprite=GetNode<AnimatedSprite>(nameof(Sprite));
        sprite.Frame=(int)SOLID;

        area=GetNode<Area2D>(nameof(Area2D));
        area.Connect("body_entered",this,nameof(OnBodyEntered));
        area.Connect("body_exited",this,nameof(OnBodyExited));

        notifier=GetNode<VisibilityNotifier2D>(nameof(VisibilityNotifier2D));
        notifier.Rect=new Rect2(notifier.Rect.Position,Scale*16f);
        notifier.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));

        bool backwards=MathUtils.RandBool();
        AnimatedSprite surface=SOLID==SOLID_TYPE.SLIME?SURFACE_SLIME_PACK.Instance<AnimatedSprite>():SURFACE_WATER_PACK.Instance<AnimatedSprite>();
        surface.Position=Vector2.Zero;
        surface.Play("default",backwards);
        AddChild(surface);

        sprite.Scale=Scale;
        area.Scale=Scale;
        area.Position=new Vector2(Scale.x*16f*0.5f,Scale.y*16f*0.5f);

        Scale=Vector2.One;

        int steps=Mathf.CeilToInt(sprite.Scale.x*16f)/16;
        int x=16;

        for(int i=1;i<steps;i++)
        {
            if(surface.Duplicate() is AnimatedSprite surf)
            {
                surf.Position=new Vector2(x*i,0f);
                surf.Play("default",backwards);
                AddChild(surf);
            }
        }

    }

    private void OnBodyEntered(Node node)
    {
        Player.instance.SetFloating(true);
    }
    private void OnBodyExited(Node node)
    {
        Player.instance.SetFloating(false);
    }

}
