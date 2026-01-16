using Godot;

public class ItemTaken : Particles
{
    private static Vector2 offset=new Vector2(0f,0.5f);
    public static AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PowerUp/Retro PowerUP StereoUP 05.wav");
    public SfxPlayer audio=new SfxPlayer();

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
