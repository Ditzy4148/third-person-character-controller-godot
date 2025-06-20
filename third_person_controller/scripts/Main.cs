using Godot;
using System;

public partial class Main : Node3D
{
    [Export]
    public PackedScene MobScene { get; set; }
    private Timer _myTimer;
    private Player _player;

    public override void _Ready()
    {
        GetNode<ColorRect>("UserInterface/Retry").Hide();
        _player = GetNode<Player>("SubViewportContainer/SubViewport/Level/Player");
        _player.MobHit += () => OnPlayerHit();

        _myTimer = GetNode<Timer>("MobTimer");
        _myTimer.Timeout += () => SpawnMob();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") && GetNode<ColorRect>("UserInterface/Retry").Visible)
        {
            GetTree().ReloadCurrentScene();
        }
    }


    private void SpawnMob()
    {
        Mob mob = MobScene.Instantiate<Mob>();

        // Choose a random location on the path and store it
        var mobSpawnLocation = GetNode<PathFollow3D>("SubViewportContainer/SubViewport/Level/SpawnPath/SpawnLocation");
        // Give the spawn a random offset
        mobSpawnLocation.ProgressRatio = GD.Randf();
        // Randf() returns a random number between 0 and 1
        // ProgressRatio expects 0 = start of path and 1 = end of path

        Vector3 playerPosition = _player.Position;
        mob.Initialize(mobSpawnLocation.Position, playerPosition);

        AddChild(mob);

        mob.Squashed += GetNode<ScoreLabel>("UserInterface/ScoreLabel").OnMobSquashed;
    }

    private void OnPlayerHit()
    {
        GD.Print("Player hit - detected by Main.cs");
        GetNode<ColorRect>("UserInterface/Retry").Show();
        _myTimer.Stop();
    }
}
