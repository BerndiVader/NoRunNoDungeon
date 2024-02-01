using Godot;
using System;

public class PlayerLight2D : Light2D
{
    [Export] private float MinLight=1f;
    [Export] private float MaxLight=1.5f;
    [Export] private int LightDelay=10;
    [Export] private Vector2 ImgScale=new Vector2(2.5f,2.5f);

    private int lightCounter;

    public override void _Ready()
    {
        lightCounter=0;
        Scale=ImgScale;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(lightCounter==LightDelay)
        {
            Energy=(float)MathUtils.randomRange(MinLight,MaxLight);
            lightCounter=0;
        }
        lightCounter++;
    }

}
