using Godot;
using System;
using System.Collections;

public class SwingingBaton : Area2D,ISwitchable
{
    private enum SWINGTYPE
    {
        LINEAR,
        SINUS,
        SINUS_DAMPING
    }
    private enum MODE
    {
        NORMAL,
        SWITCH,
        DISTANCE
    }

    [Export] private float minSpeed=0.2f;
    [Export] private float maxSpeed=4f;
    [Export] private float maxRotation=90f;
    [Export] private float SinusDamping=0.995f;
    [Export] private MODE mode=MODE.NORMAL;
    [Export] private SWINGTYPE type=SWINGTYPE.LINEAR;
    [Export] private string switchID="";
    [Export] private bool oneShoot=false;
    [Export] private float activationRange=100f;
    
    private Vector2 rotateTo,rot;
    private float linMinSpeed;
    private float linMaxSpeed;
    private float startRotation;
    private float sinSwingTime;
    private float sinAmplitude;
    private float sinSwingPhase;
    private float sinFrequency;

    private delegate void Goal(float delta);
    private Goal goal;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        AddToGroup(GROUPS.OBSTACLES.ToString());

        switch(mode)
        {
            case MODE.DISTANCE:
            case MODE.NORMAL:
                break;
            case MODE.SWITCH:
                AddToGroup(GROUPS.SWITCHABLES.ToString());
                SetPhysicsProcess(false);
                break;
        }
        
        startRotation=maxRotation*Mathf.Sign(RotationDegrees);

        sinAmplitude=Mathf.Deg2Rad(startRotation);
        sinFrequency=Mathf.Lerp(minSpeed,maxSpeed,0.5f);

        linMinSpeed=sinAmplitude*minSpeed;
        linMaxSpeed=sinAmplitude*maxSpeed;

        switch(type)
        {
            case SWINGTYPE.LINEAR:
                rotateTo=new Vector2(Mathf.Deg2Rad(maxRotation),0f);
                rot=new Vector2(Rotation,0f);
                goal=Linear;
                break;
            case SWINGTYPE.SINUS:
                sinSwingTime=0f;
                sinSwingPhase=(sinAmplitude!=0f)?Mathf.Asin(Mathf.Clamp(Rotation/sinAmplitude,-1f,1f)):0f;
                goal=Sinus;
                break;
            case SWINGTYPE.SINUS_DAMPING:
                sinSwingTime=0f;
                sinSwingPhase=(sinAmplitude!=0f)?Mathf.Asin(Mathf.Clamp(Rotation/sinAmplitude,-1f,1f)):0f;
                goal=SinDamping;            
                break;
        }

        Connect("body_entered",this,nameof(OnBodyEntered));
    }

    private void OnBodyEntered(Node2D body)
    {
        if(body.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            body.EmitSignal(STATE.damage.ToString(),1f,this);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        goal(delta);
    }

    private void Linear(float delta)
    {
        float speed=rot.x;
        rot=rot.LinearInterpolate(rotateTo,delta*3f);
        speed=rot.x-speed;
        speed=MathUtils.MinMax(linMinSpeed,linMaxSpeed,Math.Abs(speed))*Math.Sign(speed)*delta*8f;
        Rotate(speed);

        if(RotationDegrees>maxRotation-1f)
        {
            rotateTo=new Vector2(Mathf.Deg2Rad(maxRotation*-1f),0f);
            if(mode==MODE.SWITCH||oneShoot)
            {
                SetPhysicsProcess(false);
            }
        }
        else if(RotationDegrees<(maxRotation-1f)*-1f)
        {
            rotateTo=new Vector2(Mathf.Deg2Rad(maxRotation),0f);
            if(mode==MODE.SWITCH||oneShoot)
            {
                SetPhysicsProcess(false);
            }
        }
    }

    private void Sinus(float delta)
    {
        sinSwingTime+=delta;
        Rotation=Mathf.Sin(sinSwingTime*sinFrequency+sinSwingPhase)*sinAmplitude;
    }

    private void SinDamping(float delta)
    {
        sinSwingTime+=delta;
        sinAmplitude*=SinusDamping;
        Rotation=Mathf.Sin(sinSwingTime*sinFrequency+sinSwingPhase)*sinAmplitude;

        if(Mathf.Abs(sinAmplitude)<0.01f&&Mathf.Abs(RotationDegrees)<2f)
        {
            SetPhysicsProcess(false);
        }
    }

    public void SwitchCall(string id)
    {
        if(switchID==id&&!IsPhysicsProcessing())
        {
            SetPhysicsProcess(true);
        }
    }
}
