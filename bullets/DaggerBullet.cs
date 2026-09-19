using Godot;

public class DaggerBullet : Area2D
{

    private static PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bullets/DaggerBullet.tscn");

    [Export] private Vector2 HEIGHT_OFFSET=new Vector2(150f,50f);
    private readonly Vector2 END_OFFSET=new Vector2(0f,-50f);

    private Vector2 start,end,height; 
    private float elapsed=0f;
    private const float FLIGHT_TIME=0.4f;
    private int xdir=1;

    public static DaggerBullet Create(int facing)
    {
        DaggerBullet bullet=PACK.Instance<DaggerBullet>();
        bullet.xdir=facing;
        return bullet;
    }


    public override void _Ready()
    {
        start=Position;
        end=new Vector2(start.x+(HEIGHT_OFFSET.x*xdir),start.y+HEIGHT_OFFSET.y);
        height=(start+end)*0.5f+END_OFFSET;

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
            Destroy();
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
            node.EmitSignal(STATE.damage.ToString(),Player.instance,1f,false);
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
