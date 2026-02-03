using Godot;

public class Destroyables : Area2D,ISwitchable
{
    private static readonly PackedScene ExpolderPack=ResourceLoader.Load<PackedScene>("res://gfx/TileExploder.tscn");
    private static readonly AudioStream hitFx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/26_sword_hit_1.wav");

    [Export] private bool terraform=true;
    [Export] private bool notPlayer=false;
    [Export] private bool destroyParent=false;
    [Export] private string switchID="";

    private CollisionShape2D collisionController;
    private Alert alert;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen), new Godot.Collections.Array(this));
        AddChild(notifier2D);

        collisionController=GetNode<CollisionShape2D>(nameof(CollisionShape2D));

        if(switchID=="")
        {
            AddUserSignal(STATE.damage.ToString());
            Connect(STATE.damage.ToString(),this,nameof(OnDamage));

            alert=ResourceUtils.particles[(int)PARTICLES.ALERT].Instance<Alert>();
            alert.chr="x"[0];
            AddChild(alert);
        }
        else
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }

    }

    private void OnDamage(Node2D node=null,float amount=0f)
    {
        if(notPlayer&&node is Player)
        {
            return;
        }

        if(node is Player)
        {
            DaggerShoot particle=ResourceUtils.particles[(int)PARTICLES.DAGGERSHOOT].Instance<DaggerShoot>();
            particle.Position=Position+GetCollisionRectEdge(node.GlobalPosition);

            SfxPlayer sfx=new SfxPlayer
            {
                Stream=hitFx,
                Position=Position
            };
            World.level.AddChild(sfx);
            sfx.Play();
            World.level.AddChild(particle);
        }

        if(!destroyParent)
        {
            Vector2 local=World.level.ToLocal(GlobalPosition);
            Vector2 tile=World.level.WorldToMap(local);

            int id=World.level.GetCellv(tile);
            if(id!=TileMap.InvalidCell)
            {
                ImageTexture texture=ExtractTexture(id,tile);
                if(texture!=null)
                {
                    TileExploder exploder=ExpolderPack.Instance<TileExploder>();
                    exploder.Texture=texture;
                    exploder.Position=local;
                    World.level.AddChild(exploder);
                }
            }

            World.level.SetCellv(tile,-1);
            if(terraform)
            {
                World.level.Terraform(tile);
            }
            CallDeferred("queue_free");
        }
        else
        {
            ExplodeGfx gfx=ResourceUtils.gfx[0].Instance<ExplodeGfx>();
            gfx.useParticles=false;
            gfx.Position=World.level.ToLocal(GlobalPosition);
            World.level.AddChild(gfx);
            GetParent().CallDeferred("queue_free");
        }

    }

    protected Vector2 GetCollisionRectEdge(Vector2 source)
    {
        Vector2 direction=GlobalPosition.DirectionTo(source);
        Vector2 extents=((RectangleShape2D)collisionController.Shape).Extents;

        float px=Mathf.Sign(direction.x)*extents.x;
        float py=Mathf.Sign(direction.y)*extents.y;
        float rx=Mathf.Abs(direction.x/extents.x);
        float ry=Mathf.Abs(direction.y/extents.y);

        if(rx>ry)
        {
            return new Vector2(px,direction.y/Mathf.Abs(direction.x)*extents.x);
        }
        else
        {
            return new Vector2(direction.x/Mathf.Abs(direction.y)*extents.y,py);
        }

    }    

    private static ImageTexture ExtractTexture(int id,Vector2 tile)
    {
        Image big=new Image();
        big.Create(64,64,false,Image.Format.Rgba8);
        big.Fill(new Color(0,0,0,0));
        big.BlitRect(World.level.CreateImageForTile(id,tile),new Rect2(0f,0f,16f,16f),new Vector2(24f,24f));

        ImageTexture texture=new ImageTexture();
        texture.CreateFromImage(big);
        texture.Flags=0;
        return texture;
    }

    public void SwitchCall(string id)
    {
        if(switchID==id)
        {
            switchID="";
            OnDamage();
        }
    }
}
