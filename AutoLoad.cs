using Godot;

public class AutoLoad : Node
{
    public override void _Ready()
    {
        GameSettings.Init();
        GameSettings.current.SetAll(this);

        Worker.Start();
        ResourceUtils.Init();
        World.Init(GetTree().Root);
        QueueFree();
    }

}
