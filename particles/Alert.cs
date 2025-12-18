using Godot;
using System;

public class Alert : CPUParticles2D
{
    public int chr;
    public float scale=1f;
    private readonly ImageTexture texture=new ImageTexture();

    public override void _Ready()
    {
        texture.CreateFromImage(ResourceUtils.fontTexture.GetRect(ResourceUtils.font.GetCharTxUvRect(chr)),1);
        Texture=texture;

        ScaleAmount=scale;
        Scale*=0.5f;
        OneShot=false;
        Emitting=true;
    }

}
