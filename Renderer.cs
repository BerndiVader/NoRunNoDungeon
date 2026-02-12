using Godot;
using System;

public class Renderer : CanvasModulate
{
    public static Renderer instance;
    [Export] private float ShakeMax=6f;
    private float shake;
    private Sprite trailtop,trailbottom;
    
    public override void _Ready()
    {
        instance=this;
        shake=0f;

        trailtop=GetNode<Sprite>("SpeedTrailsTop");
        trailbottom=GetNode<Sprite>("SpeedTrailsBottom");

    }

    public override void _PhysicsProcess(float delta)
    {
        if(World.level.speed>120f)
        {
            trailtop.Visible=trailbottom.Visible=true;
        }
        else
        {
            trailtop.Visible=trailbottom.Visible=false;
        }

        if(shake!=0d) 
        {
            ApplyShake();
        }

    }

    private void ApplyShake()
    {
        shake=Mathf.Min(shake,ShakeMax);
        if(shake>=0.3f)
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

    public float Shake()
    {
        return shake;
    }
    public void Shake(float amount)
    {
        shake=Mathf.Min(shake+amount,ShakeMax);
    }
    public void SetShake(float amount)
    {
        shake=Mathf.Min(amount,ShakeMax);
    }

    public void PlaySfx(AudioStream stream,Vector2 position)
    {
        SfxPlayer sfx=new SfxPlayer();
        sfx.Stream=stream;
        sfx.Position=position;
        AddChild(sfx);
    }

}
