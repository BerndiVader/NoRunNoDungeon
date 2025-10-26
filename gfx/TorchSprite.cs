using Godot;
using System;

public class TorchSprite : Godot.Sprite
{
    private Vector2 defaultScale;
    private readonly int delay = 5;
    private int delayCount = 0;

    private ShaderMaterial shader;

    public override void _Ready()
    {
        defaultScale = Scale;
        shader = (ShaderMaterial)Material;

        SetProcess(false);
        SetPhysicsProcess(true);

        VisibilityNotifier2D notifier2D = new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited", World.instance, nameof(World.onObjectExitedScreen), new Godot.Collections.Array(this));
        AddChild(notifier2D);

    }

    public override void _PhysicsProcess(float delta)
    {
        delayCount++;
        if (delayCount >= delay)
        {
            delayCount = 0;
            Scale = defaultScale * (float)MathUtils.randomRange(0.8f, 1.2f);
            shader.SetShaderParam("amount", MathUtils.randomRange(40f, 60f));
        }
    }

}
