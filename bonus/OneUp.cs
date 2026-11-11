using Godot;
using System;

public class OneUp : WeaponItem
{
    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);
        
        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        animation.Frame=0;

        material.Shader=glintShader;
        material.SetShaderParam("gradient_texture",gradient);
        animation.Material=material;

        Connect("body_entered",this,nameof(OnEnteredBody));
    }

    public override void _PhysicsProcess(float delta)
    {
        shine+=delta*1.5f;
        float progress=shine%1f;
        material.SetShaderParam("shine_progress",progress);

        float floatOffset=Mathf.Sin(shine*2f)*4f;
        Position=new Vector2(Position.x,Position.y+floatOffset*delta);
    }

    protected override void OnEnteredBody(Node body)
    {
        ItemTaken taken=ResourceUtils.particles[(int)PARTICLES.ITEMTAKEN].Instance<ItemTaken>();
        taken.Scale=new Vector2(0.5f,0.5f);
        taken.Position=Position;
        taken.Texture=animation.Frames.GetFrame("default",0);
        World.level.CallDeferred("add_child",taken);

        Apply(Player.instance);
        CallDeferred("queue_free");
    }
    public override void Apply(Player player)
    {
        Player.Lives(1);
    }

}
