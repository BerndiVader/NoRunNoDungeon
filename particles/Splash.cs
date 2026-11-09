using Godot;

public class Splash : CPUParticles2D
{

    private static readonly PackedScene pack=ResourceLoader.Load<PackedScene>("res://particles/Splash.tscn");

    public static Splash Create()
    {
        Splash particles=pack.Instance<Splash>();
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
            SetPhysicsProcess(false);
            CallDeferred("queue_free");
        }
    }
}
