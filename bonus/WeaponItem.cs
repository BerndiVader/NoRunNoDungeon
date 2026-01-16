using Godot;
using System;

public class WeaponItem : Bonus
{
    [Export] private WEAPONS weaponType=WEAPONS.SWORD;
    protected Shader glintShader=ResourceLoader.Load<Shader>("res://shaders/Glint.gdshader");
    protected ShaderMaterial material;
    protected static GradientTexture2D gradient=CreateGradient();
    protected float shine=0f;

    public override void _Ready()
    {
        base._Ready();

        material=new ShaderMaterial
        {
            Shader=glintShader,
        };
        material.SetShaderParam("gradient_texture",gradient);
        animation.Material=material;

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
            World.level.AddChild(taken);

            CallDeferred("queue_free");
        }
    }

    public override void Apply(Player player)
    {
        throw new NotImplementedException();
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
