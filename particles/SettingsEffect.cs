using Godot;
using System;

public class SettingsEffect : CPUParticles2D
{
    public int chr;
    public float scale=5f;
    private readonly ImageTexture tex=new ImageTexture();

    public override void _Ready()
    {
        ScaleAmount=scale;
        OneShot=true;

        tex.CreateFromImage(ResourceUtils.fontTexture.GetRect(ResourceUtils.font.GetCharTxUvRect(chr)),1);
        Texture=tex;
        Scale=PlayerCamera.instance.Zoom;
        Position=PlayerCamera.instance.GetCameraScreenCenter();   
        Emitting=true;
    }

    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            QueueFree();
        }
    }

}
