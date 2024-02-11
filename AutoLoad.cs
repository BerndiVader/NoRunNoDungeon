using Godot;

public class AutoLoad : Node
{
    public override void _Ready()
    {
        GameSettings.init();
        GameSettings.current.setAll(this);

        new Worker();
        ResourceUtils.Init();
        World.Init(GetTree().Root);
        QueueFree();
    }

}
