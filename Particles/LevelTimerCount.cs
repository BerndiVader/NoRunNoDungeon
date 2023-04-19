using Godot;
using System;

public class LevelTimerCount : CPUParticles2D
{
    public int chr;
    static Image fontTex;
    static BitmapFont font;

    static LevelTimerCount()
    {
        font=new BitmapFont();
        font.CreateFromFnt("res://fonts/m6x11.fnt");
        fontTex=font.GetTexture(0).GetData();
    }

    public override void _Ready()
    {
        OneShot=true;
        ImageTexture tex=new ImageTexture();
        tex.CreateFromImage(fontTex.GetRect(font.GetCharTxUvRect(48+chr)),1);
        Texture=tex;
        this.Emitting=true;
        Scale=PlayerCamera.instance.Zoom;
        Position*=Scale;
        Position+=PlayerCamera.instance.GetCameraScreenCenter();         
    }

    public override void _Process(float delta)
    {
        if(!Emitting)
        {
            QueueFree();
        }
    }

}
