using Godot;
using System;

public class SettingsEffect : CPUParticles2D
{
    public int chr;
    public float scale=5f;
    private ImageTexture tex=new ImageTexture();

    public override void _Ready()
    {
        ScaleAmount=scale;

        tex.CreateFromImage(ResourceUtils.fontTexture.GetRect(ResourceUtils.font.GetCharTxUvRect(chr)),1);
        Texture=tex;
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
