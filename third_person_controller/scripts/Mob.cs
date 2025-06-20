using Godot;
using System;

public partial class Mob : CharacterBody3D
{
    [Export]
    public int MinSpeed { get; set; } = 5;
    [Export]
    public int MaxSpeed { get; set; } = 15;

    [Signal]
    public delegate void SquashedEventHandler();

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    public void Initialize(Vector3 startPosition, Vector3 playerPosition)
    {
        // Need to zero Y value, or mobs will rotate Y
        LookAtFromPosition(startPosition, new Vector3(playerPosition.X, 0, playerPosition.Z));
        RotateY((float)GD.RandRange(-(Mathf.Pi) / 4.0, Mathf.Pi / 4.0));

        int randomSpeed = GD.RandRange(MinSpeed, MaxSpeed);
        Velocity = Vector3.Forward * randomSpeed;
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);
    }

    private void OnVisibilityNotifierScreenExited()
    {
        QueueFree();
    }

    public void Squash()
    {
        EmitSignal(SignalName.Squashed);
        QueueFree();
    }
}
