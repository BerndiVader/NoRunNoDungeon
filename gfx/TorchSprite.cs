using Godot;
using Godot.Collections;

public class TorchSprite : Sprite
{
    private const int DELAY=5;
    
    private Vector2 defaultScale;
    private int delayCount=0;

    private ShaderMaterial shader;
    private Node2D parent;

    public override void _Ready()
    {
        defaultScale=Scale;
        shader=(ShaderMaterial)Material;
        parent=GetParent<Node2D>();

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
            Scale=defaultScale*(float)MathUtils.RandomRange(0.8f,1.2f);
            shader.SetShaderParam("amount", MathUtils.RandomRange(40f,60f));
        }
    }

}
