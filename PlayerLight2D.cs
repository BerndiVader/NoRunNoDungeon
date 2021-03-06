using Godot;
using System;

public class PlayerLight2D : Light2D
{
    [Export] public float MinLight=1f;
    [Export] public float MaxLight=1.2f;
    [Export] public int LightDelay=10;
    [Export] public Vector2 ImgScale=new Vector2(1.5f,1.5f);

    int lightCounter;

    public override void _Ready()
    {
        lightCounter=0;
        Scale=ImgScale;
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

}
