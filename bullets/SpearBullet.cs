using Godot;

public class SpearBullet : Area2D
{
    private static readonly PackedScene PACK=ResourceLoader.Load<PackedScene>("res://bullets/SpearBullet.tscn");

    [Export] private float throwSpeed=300f;
    [Export] private float throwAngle=10f;
    [Export] private float gravity=600f;

    private Vector2 direction;
    private Vector2 velocity;
    private Sprite sprite;
    private int xdir=1;

    public static SpearBullet Create(int facing)
    {
        SpearBullet bullet=PACK.Instance<SpearBullet>();
        bullet.xdir=facing;
        return bullet;
    }

    public override void _Ready()
    {
        direction=new Vector2(xdir,0f);
        velocity=new Vector2(direction.x*Mathf.Cos(Mathf.Deg2Rad(throwAngle)),-Mathf.Sin(Mathf.Deg2Rad(throwAngle)))*throwSpeed;

        sprite=GetNode<Sprite>(nameof(Sprite));

        Connect("body_entered",this,nameof(OnBodyEntered));
        Connect("area_entered",this,nameof(OnBodyEntered));
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity.y+=gravity*delta;
        Rotation=velocity.Angle()+Mathf.Pi*0.5f;

        Translate(velocity*delta);
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

        particles.GetNode<CPUParticles2D>("Second").Texture=sprite.Texture;

        World.level.AddChild(particles);
        CallDeferred("queue_free");
    }

}
