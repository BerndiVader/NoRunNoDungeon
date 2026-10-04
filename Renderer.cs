using Godot;
using System;

public class Renderer : CanvasModulate
{
    public static Renderer instance;
    [Export] private float ShakeMax=6f;
    private float shake;
    private float speed=0f;
    private Vector2 direction=Vector2.Zero;
    private Vector2 topH=new Vector2(254f,0f);
    private Vector2 topV=new Vector2(0f,144f);
    private Vector2 bottomH=new Vector2(254f,288f);
    private Vector2 bottomV=new Vector2(512f,144f);
    private readonly Vector2 MIN_SPEED=new Vector2(80f,30f);
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
        Level level=World.level;
        float currentSpeed=level.speed;
        Vector2 currentDirection=level.direction;
        bool speedChanged=currentSpeed!=speed;
        bool directionChanged=currentDirection!=direction;

        if(speedChanged||directionChanged)
        {
            direction=currentDirection;
            speed=currentSpeed;

            bool vertical=currentDirection.y!=0f;
            float minspeed=vertical?MIN_SPEED.y:MIN_SPEED.x;

            if(directionChanged)
            {
                bool flipHorizontal=currentDirection.x!=-1&&currentDirection.y!=-1;
                trailbottom.FlipH=flipHorizontal;
                trailtop.FlipH=flipHorizontal;

                if(vertical)
                {
                    trailtop.RotationDegrees=trailbottom.RotationDegrees=90f;
                    trailtop.Position=topV;
                    trailbottom.Position=bottomV;
                }
                else
                {
                    trailtop.RotationDegrees=trailbottom.RotationDegrees=0f;
                    trailtop.Position=topH;
                    trailbottom.Position=bottomH;
                }
            }

            if(currentSpeed>minspeed)
            {
                Color modulate=trailtop.Modulate;
                float alpha=Mathf.Clamp((currentSpeed-minspeed)/(minspeed*1.45f),0.1f,1f);
                trailtop.Modulate=new Color(modulate.r,modulate.g,modulate.b,alpha);
                trailbottom.Modulate=trailtop.Modulate;
                trailtop.Visible=trailbottom.Visible=true;
            }
            else
            {
                trailtop.Visible=trailbottom.Visible=false;
            }
        }

        if(shake!=0f) 
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
        if(GameSettings.current.Rumble)
        {
            World.instance.input.Rumble(shake/ShakeMax);
        }
    }
    public void SetShake(float amount)
    {
        shake=Mathf.Min(amount,ShakeMax);
        if(GameSettings.current.Rumble)
        {
            World.instance.input.Rumble(shake/ShakeMax);
        }
    }

    public void PlaySfx(AudioStream stream,Vector2 globalPosition,float volume=0f)
    {
        SfxPlayer sfx=new SfxPlayer();
        sfx.Stream=stream;
        sfx.Position=ToLocal(globalPosition);
        AddChild(sfx);
    }

}
