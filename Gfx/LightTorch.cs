using Godot;
using System;

public class LightTorch : Light2D
{
    [Export] public float MinLight=1f;
    [Export] public float MaxLight=1f;
    [Export] public int LightDelay=25;
    [Export] public Vector2 ImgScale=new Vector2(1f,1f);
    int lightCounter;

    protected AnimatedSprite animationController;
    protected Placeholder parent;
    protected VisibilityNotifier2D notifier2D;

    public override void _Ready()
    {
        lightCounter=0;
        Scale=ImgScale;

        notifier2D=new VisibilityNotifier2D();
        if(GetParent().GetType().Name=="Placeholder")
        {
            parent=(Placeholder)GetParent();
            notifier2D.Connect("screen_exited",parent,"exitedScreen");
        }
        else 
        {
            notifier2D.Connect("screen_exited",this,"exitedScreen");
        }
        AddChild(notifier2D);

        animationController=(AnimatedSprite)GetNode("AnimatedSprite");
        animationController.Play();
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

    public void exitedScreen()
    {
        CallDeferred("queue_free");
    }
}
