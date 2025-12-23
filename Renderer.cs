using Godot;
using System;

public class Renderer : CanvasModulate
{
    [Export] private float ShakeMax=6f;
    public float shake;
    
    public override void _Ready()
    {
        shake=0f;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(shake!=0d) 
        {
            ApplyShake();
        }
    }

    private void ApplyShake()
    {
        shake=Mathf.Min(shake,ShakeMax);
        if(shake>=0.3d)
        {
            Position=new Vector2(MathUtils.RandomRange(-shake,shake),MathUtils.RandomRange(-shake,shake));
            shake*=0.9f;
        } 
        else if(shake>0f)
        {
            shake=0f;
            Position=Vector2.Zero;
        }
    }
}
