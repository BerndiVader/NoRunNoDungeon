using Godot;
using System;

public class PlayerLight2D : Light2D
{
    [Export] private float MIN_LIGHT=1f;
    [Export] private float MAX_LIGHT=1.5f;
    [Export] private int LIGHT_DELAY=10;
    [Export] private Vector2 IMAGE_SCALE=new Vector2(2.5f,2.5f);

    private int lightCounter;

    public override void _Ready()
    {
        lightCounter=0;
        Scale=IMAGE_SCALE;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(lightCounter==LIGHT_DELAY)
        {
            Energy=(float)MathUtils.RandomRange(MIN_LIGHT,MAX_LIGHT);
            lightCounter=0;
        }
        lightCounter++;
    }

}
