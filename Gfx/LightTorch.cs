using Godot;
using System;

public class LightTorch : Light2D
{
    [Export] private float MinLight=1f;
    [Export] private float MaxLight=1f;
    [Export] private int LightDelay=25;
    [Export] private Vector2 ImgScale=new Vector2(1f,1f);
    private int lightCounter;

    public override void _Ready()
    {
        lightCounter=0;
        Scale=ImgScale;

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",this,nameof(onExitedScreen));
        AddChild(notifier2D);

        GetNode<AnimatedSprite>("AnimatedSprite").Play();
    }

    public override void _Process(float delta)
    {
        if(lightCounter==LightDelay)
        {
            Energy=(float)MathUtils.randomRange(MinLight,MaxLight);
            lightCounter=0;
        }
        lightCounter++;
    }

    private void onExitedScreen()
    {
        QueueFree();
    }
}
