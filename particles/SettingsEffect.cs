using Godot;
using System;

public class SettingsEffect : CPUParticles2D
{
    public int chr;
    public float scale=5f;
    private static Image fontTex;
    private static BitmapFont font;
    private ImageTexture tex=new ImageTexture();

    static SettingsEffect()
    {
        font=new BitmapFont();
        font.CreateFromFnt("res://fonts/m6x11.fnt");
        fontTex=font.GetTexture(0).GetData();
    }

    public override void _Ready()
    {
        ScaleAmount=scale;
        OneShot=true;

        tex.CreateFromImage(fontTex.GetRect(font.GetCharTxUvRect(chr)),1);
        Texture=tex;
        Emitting=true;
        Scale=PlayerCamera.instance.Zoom;
        Position=PlayerCamera.instance.GetCameraScreenCenter();         
    }

    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            QueueFree();
        }
    }

}
