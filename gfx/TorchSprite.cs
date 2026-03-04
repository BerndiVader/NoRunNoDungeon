using Godot;
using Godot.Collections;

public class TorchSprite : Sprite
{
    private const int DELAY=5;

    [Export] private Vector2 SCALE_RANGE=new Vector2(0.8f,1.2f);
    [Export] private Vector2 GLOW_RANGE=new Vector2(40f,60f);
    
    private Vector2 defaultScale;
    private int delayCount=0;

    private ShaderMaterial shader;
    private Node2D parent;

    public override void _Ready()
    {
        defaultScale=Scale;
        shader=(ShaderMaterial)Material;
        parent=GetParent<Node2D>();
        parent.GetNode<AnimatedSprite>("AnimatedSprite2").Play();

        SetProcess(false);
        SetPhysicsProcess(true);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Array(parent));
        AddChild(notifier2D);
    }

    public override void _PhysicsProcess(float delta)
    {
        delayCount++;
        if(delayCount>=DELAY)
        {
            delayCount=0;
            Scale=defaultScale*(float)MathUtils.RandomRange(SCALE_RANGE);
            shader.SetShaderParam("amount",MathUtils.RandomRange(GLOW_RANGE));
        }
    }

}
