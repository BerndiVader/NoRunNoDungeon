using Godot;
using System;

public class WeaponItem : Bonus
{    
    public static readonly GradientTexture2D gradient=CreateGradient();

    [Export] private WEAPONS WEAPON_TYPE=WEAPONS.SWORD;
    [Export] private bool FORCE_SPAWN=false;

    protected readonly Shader glintShader=ResourceLoader.Load<Shader>("res://shaders/Glint.gdshader");
    protected readonly ShaderMaterial material=new ShaderMaterial();
    protected float shine=0f;

    public override void _Ready()
    {
        if(!FORCE_SPAWN&&!SpawnCondition())
        {
            CallDeferred("queue_free");
            return;
        }
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);
        
        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        animation.Frame=(int)WEAPON_TYPE;

        material.Shader=glintShader;
        material.SetShaderParam("gradient_texture",gradient);
        animation.Material=material;

        Connect("body_entered",this,nameof(OnEnteredBody));
    }

    public override void _PhysicsProcess(float delta)
    {
        shine+=delta*2f;
        float progress=shine%1f;
        material.SetShaderParam("shine_progress",progress);

        float floatOffset=Mathf.Sin(shine*2f)*4f;
        Position=new Vector2(Position.x,Position.y+floatOffset*delta);
    }

    protected override void OnEnteredBody(Node body)
    {
        if(body is Player player)
        {
            ItemTaken taken=ResourceUtils.particles[(int)PARTICLES.ITEMTAKEN].Instance<ItemTaken>();
            taken.Scale=new Vector2(0.5f,0.5f);
            taken.Position=Position;
            taken.Texture=animation.Frames.GetFrame("default",0);
            World.level.CallDeferred("add_child",taken);
            player.CallDeferred("EquipWeapon",ResourceUtils.weapons[(int)WEAPON_TYPE]);
            CallDeferred("queue_free");
        }
    }

    public override void Apply(Player player)
    {
        throw new NotImplementedException();
    }

    public virtual bool SpawnCondition()
    {
        return !Player.instance.HasWeapon();
    }

    private static GradientTexture2D CreateGradient()
    {
        Gradient gradient=new Gradient
        {
            Offsets=new float[]
            {
                0f,
                0.503731f,
                1f
            },
            Colors=new Color[]
            {
                new Color(1f,1f,1f,0f),
                new Color(1f,1f,1f,1f),
                new Color(1f,1f,1f,0f)
            }
        };

        GradientTexture2D texture=new GradientTexture2D
        {
            Gradient=gradient,
            Width=16
        };

        return texture;
    }      
}
