using Godot;

public class HealthLoss : CPUParticles2D
{
    private static Vector2 offset=new Vector2(0f,0.1f);
    public string text;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        
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
