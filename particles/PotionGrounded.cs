using Godot;

public class PotionGrounded : CPUParticles2D
{

    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://particles/PotionGrounded.tscn");

    public static PotionGrounded Create()
    {
        PotionGrounded particles=pack.Instance<PotionGrounded>();
        return particles;
    }

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        
        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting) 
        {
            QueueFree();
            SetPhysicsProcess(false);
        }
    }
}
