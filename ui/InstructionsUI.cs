using Godot;

public class InstructionsUI : BaseUI
{
    private Button back;

    public override void _Ready()
    {
        back=GetNode<Button>("Back");

        back.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));
        back.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));
        back.Connect("button_up",this,nameof(OnMouseSelected),new Godot.Collections.Array(0));
        selected=0;

        base._Ready();
    }

    private void OnMouseSelected(int selected)
    {
        switch(selected)
        {
            case 0:
                QueueFree();
                break;
        }
    }

}
