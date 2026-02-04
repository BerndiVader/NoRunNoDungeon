using Godot;

public class ItemTaken : Particles
{
    public static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PowerUp/Retro PowerUP StereoUP 05.wav");
    public readonly SfxPlayer audio=new SfxPlayer();
    private static Vector2 offset=new Vector2(0f,0.05f);

    public override void _Ready()
    {
        base._Ready();
        audio.Stream=sfx;
        audio.Position=Position;
        audio.VolumeDb=-10f;
        World.level.AddChild(audio);

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
        Position-=offset;
    }

}
