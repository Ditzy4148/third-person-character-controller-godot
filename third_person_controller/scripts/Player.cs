using Godot;
using System;

public partial class Player : CharacterBody3D
{

    // Move speed in meters per second
    [Export]
    public int Speed { get; set; } = 14;

    // Downward acceleration when in air, in meters per second squared
    [Export]
    public int FallAcceleration { get; set; } = 95;

    [Export]
    public int JumpImpulse { get; set; } = 30;
    [Export]
    public double JumpBuffer { get; set; } = 0.2f;
    [Export]
    public double JumpBufferTimer { get; set; }

    [Export]
    public int BounceImpulse { get; set; } = 16;

    private Vector3 _targetVelocity = Vector3.Zero;
    private Area3D _mobDetector;

    [Signal]
    public delegate void MobHitEventHandler();

    public override void _Ready()
    {
        _mobDetector = GetNode<Area3D>("MobDetector");
        _mobDetector.BodyEntered += MobContact;
    }

    // _PhysicsProcess is used instead of _Process for physical calculations

    public override void _PhysicsProcess(double delta)
    {
        var direction = Vector3.Zero;

        direction = GetDir(direction);

        _targetVelocity.X = direction.X * Speed;
        _targetVelocity.Z = direction.Z * Speed;

        Jump(delta);

        Velocity = _targetVelocity;

        MoveAndSlide();

        // Iterate through all collisions that occurred in this frame.
        for (int index = 0; index < GetSlideCollisionCount(); index++)
        {
            // Get one of the collisions with the player.
            KinematicCollision3D collision = GetSlideCollision(index);

            // If the collision is with a mob.
            // With C# we leverage typing and pattern-matching
            // instead of checking for the group we created.
            if (collision.GetCollider() is Mob mob)
            {
                // We check that we are hitting it from above.
                // Vector3.UP.dot(collision.get_normal()) > 0.1. The collision normal is a 3D vector that
                // is perpendicular to the plane where the collision occurred. The dot product allows us
                // to compare it to the up direction. With dot products, when the result is greater than
                // 0, the two vectors are at an angle of fewer than 90 degrees. A value higher than 0.1
                // tells us that we are roughly above the monster.
                if (Vector3.Up.Dot(collision.GetNormal()) > 0.75f)
                {
                    // If so, we squash it and bounce.
                    mob.Squash();
                    _targetVelocity.Y = BounceImpulse;
                    // Prevent further duplicate calls.
                    break;
                }
            }
        }
    }

    private Vector3 GetDir(Vector3 direction)
    {
        if (Input.IsActionPressed("move_right"))
        {
            direction.X += 1.0f;
        }
        if (Input.IsActionPressed("move_left"))
        {
            direction.X -= 1.0f;
        }
        if (Input.IsActionPressed("move_back"))
        {
            direction.Z += 1.0f;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            direction.Z -= 1.0f;
        }

        if (direction != Vector3.Zero)
        {
            direction = direction.Normalized();

            GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);
        }

        return direction;
    }

    private void Jump(double delta)
    {
        if (Input.IsActionJustPressed("jump"))
        {
            // Add a buffer in case button is pressed just before landing
            JumpBufferTimer = JumpBuffer;
        }

        if (IsOnFloor())
        {
            // Reset velocity after landing
            _targetVelocity.Y = 0;

            // Player jumps as long as "jump" input is pressed when approximately on the floor
            if (JumpBufferTimer > 0)
            {
                JumpBufferTimer = 0;
                _targetVelocity.Y += JumpImpulse;
            }
        }

        if (JumpBufferTimer > 0)
        {
            // JumpBufferTimer = Mathf.Clamp(JumpBufferTimer, 0, JumpBufferTimer - (delta * 0.1));
            JumpBufferTimer -= delta;
        }

        if (!IsOnFloor())
        {
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }
    }

    private void MobContact(Node3D body)
    {
        Die();
    }

    private void Die()
    {
        EmitSignal(SignalName.MobHit);
        QueueFree();
    }
}
