using System;
using InControl;

public class PlayerInputBindings : PlayerActionSet
{
    public PlayerAction Left;
    public PlayerAction Right;
    public PlayerAction Accelerate;
    public PlayerAction Decelerate;
    public PlayerAction Fire;

    public PlayerInputBindings()
    {
        Left = CreatePlayerAction("Strafe Left");
        Right = CreatePlayerAction("Strafe Right");
        Accelerate = CreatePlayerAction("Accelerate");
        Decelerate = CreatePlayerAction("Decelerate");
        Fire = CreatePlayerAction("Fire");
    }

    public void InitializeBindings() 
    {
        
        Left.AddDefaultBinding(Key.A);
        Left.AddDefaultBinding(InputControlType.LeftStickLeft);
        Left.AddDefaultBinding(InputControlType.DPadLeft);

        Right.AddDefaultBinding(Key.D);
        Right.AddDefaultBinding(InputControlType.LeftStickRight);
        Right.AddDefaultBinding(InputControlType.DPadRight);

        Accelerate.AddDefaultBinding(Key.W);
        Accelerate.AddDefaultBinding(InputControlType.LeftStickUp);
        Accelerate.AddDefaultBinding(InputControlType.DPadUp);

        Decelerate.AddDefaultBinding(Key.S);
        Decelerate.AddDefaultBinding(InputControlType.LeftStickDown);
        Decelerate.AddDefaultBinding(InputControlType.DPadDown);

        Fire.AddDefaultBinding(Key.Space);
        Fire.AddDefaultBinding(InputControlType.Action1);
        Fire.AddDefaultBinding(InputControlType.RightTrigger);
    }
}