using Godot;
using System;

public class SettingsEffect : CPUParticles2D
{
    public int chr;
    public float scale=5f;
    private readonly ImageTexture texture=new ImageTexture();

    public override void _Ready()
    {
        ScaleAmount=scale;

        texture.CreateFromImage(ResourceUtils.fontTexture.GetRect(ResourceUtils.font.GetCharTxUvRect(chr)),1);
        Texture=texture;
        Scale=PlayerCamera.instance.Zoom;
        Position=PlayerCamera.instance.GetCameraScreenCenter();   
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
    }

}
