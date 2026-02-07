using Godot;

public class DaggerBullet : Area2D
{
    [Export] private Vector2 OFFSET=new Vector2(150f,50f);

    private Vector2 start, end, height; 
    private float elapsed=0f;
    private const float FLIGHT_TIME=0.5f;

    public override void _Ready()
    {
        int xDir=Player.instance.AnimationController.FlipH?-1:1;
        start=Position;
        end=new Vector2(start.x+(OFFSET.x*xDir),start.y+OFFSET.y);
        height=(start+end)*0.5f+new Vector2(0f,-50f);

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("area_entered",this,nameof(OnBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        Rotation+=delta*10f;
        elapsed+=delta;

        float t=Mathf.Clamp(elapsed/FLIGHT_TIME,0f,1f);
        Position=Step(t);

        if(elapsed>FLIGHT_TIME)
        {
            QueueFree();
        }
    }

    Vector2 Step(float t)
    {
        Vector2 q0=start.LinearInterpolate(height,t);
        Vector2 q1=height.LinearInterpolate(end,t);
        return q0.LinearInterpolate(q1,t);
    }

    public void OnBodyEntered(Node node)
    {
        if(node.HasUserSignal(STATE.damage.ToString()))
        {
            node.EmitSignal(STATE.damage.ToString(),Player.instance,1f);
        }
        Destroy();
    }

    void Destroy()
    {
        BulletMiss particles=(BulletMiss)ResourceUtils.particles[(int)PARTICLES.BULLETMISS].Instance();
        particles.Position=Position;
        World.level.AddChild(particles);
        CallDeferred("queue_free");
    }

}
