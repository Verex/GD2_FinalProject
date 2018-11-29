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

        Right.AddDefaultBinding(Key.D);
        Right.AddDefaultBinding(InputControlType.LeftStickRight);

        Accelerate.AddDefaultBinding(Key.W);
        Accelerate.AddDefaultBinding(InputControlType.LeftStickUp);

        Decelerate.AddDefaultBinding(Key.S);
        Decelerate.AddDefaultBinding(InputControlType.LeftStickDown);

        Fire.AddDefaultBinding(Key.Space);
        Fire.AddDefaultBinding(InputControlType.Action1);
    }
}