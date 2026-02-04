using Godot;

public class HealthLoss : Particles
{
    private static Vector2 offset=new Vector2(0f,0.1f);
    public string text;

    public override void _Ready()
    {
        base._Ready();

        Texture=ResourceUtils.CreateTextureFromText(text,ResourceUtils.font,ResourceUtils.fontTexture);
        OneShot=true;
        Emitting=true;
    }

    public override void _PhysicsProcess(float delta) 
    {
        if(!Emitting)
        {
            SetPhysicsProcess(false);
            QueueFree();
        }
        Position-=offset;
    }

}
