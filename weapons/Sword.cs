using Godot;
using System;

public class Sword : Weapon
{
    Sprite sprite,shaderSprite;
    ShaderMaterial shader;
    public override void _Ready()
    {
        base._Ready();

        sprite=GetNode<Sprite>("Sprite");
        shaderSprite=GetNode<Sprite>("ShaderSprite");
        shader=(ShaderMaterial)shaderSprite.Material;

        shaderSprite.Visible=false;

        Connect("body_entered", this, nameof(onHitSomething));
        Connect("area_entered", this, nameof(onHitSomething));
    }

    public override void _PhysicsProcess(float delta)
    {
        switch(state)
        {
            case WEAPONSTATE.IDLE:
                if(!animationPlayer.IsPlaying()&&AnimationNames.SETUP+getStringDirection()!=animationPlayer.CurrentAnimation)
                {
                    animationPlayer.Play(AnimationNames.SETUP+getStringDirection());
                }
                break;
            case WEAPONSTATE.ATTACK:
                if(!animationPlayer.IsPlaying())
                {
                    shader.SetShaderParam("swing",false);
                    sprite.Visible=true;
                    shaderSprite.Visible=false;
                    state=WEAPONSTATE.IDLE;
                    hit=false;
                }
                break;
        }
    }

    public override bool attack()
    {
        if (state == WEAPONSTATE.IDLE)
        {
            sprite.Visible=false;
            shaderSprite.Visible=true;
            shader.SetShaderParam("flip_h",!Player.instance.animationController.FlipH);
            shader.SetShaderParam("swing",true);
            playSfx(sfxSwing);
            animationPlayer.Play(AnimationNames.SWING + getStringDirection());
            state = WEAPONSTATE.ATTACK;
            return true;
        }
        return false;
    }

}
