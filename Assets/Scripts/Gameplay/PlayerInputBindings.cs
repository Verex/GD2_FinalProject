using System;
using InControl;

public class PlayerInputBindings : PlayerActionSet
{
    public PlayerAction Left;
    public PlayerAction Right;
    public PlayerAction Accelerate;
    public PlayerAction Deccelerate;
    public PlayerAction Fire;

    public PlayerInputBindings()
    {
        Left = CreatePlayerAction("Strafe Left");
        Right = CreatePlayerAction("Strafe Right");
        Accelerate = CreatePlayerAction("Accelerate");
        Deccelerate = CreatePlayerAction("Deccelerate");
        Fire = CreatePlayerAction("Fire");

        Left.AddDefaultBinding(Key.A);
        Right.AddDefaultBinding(Key.D);
        Accelerate.AddDefaultBinding(Key.W);
        Deccelerate.AddDefaultBinding(Key.S);

        Fire.AddDefaultBinding(Key.Space);
    }
}