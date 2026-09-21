using Godot;

namespace SimpleBaseball;

public partial class Field : Node2D
{
    // Mound's screen position (the ball's near/Z=0 origin) — matches the
    // MoundMarker drawn in Background.tscn.
    [Export] public Vector2 MoundScreenPosition = new(640, 560);
    [Export] public float PitchFlightSeconds = 1.0f;
    [Export] public float PitchReleaseHeight = 40f;
    [Export] public float PitchEndHeight = 10f;
    [Export] public float RespawnDelaySeconds = 1.0f;

    private PackedScene _ballScene;
    private Ball _ball;

    public override void _Ready()
    {
        _ballScene = GD.Load<PackedScene>("res://Scenes/Ball/Ball.tscn");

        var spawnTimer = new Timer
        {
            WaitTime = PitchFlightSeconds + RespawnDelaySeconds,
            Autostart = true
        };
        AddChild(spawnTimer);
        spawnTimer.Timeout += SpawnPitch;

        SpawnPitch();
    }

    private void SpawnPitch()
    {
        _ball?.QueueFree();

        _ball = _ballScene.Instantiate<Ball>();
        AddChild(_ball);

        float vz = _ball.MaxZ / PitchFlightSeconds;

        // Solve the initial vertical velocity so the ball travels from
        // PitchReleaseHeight down to PitchEndHeight over PitchFlightSeconds,
        // under Ball's own gravity: y(t) = y0 + vy0*t + 0.5*g*t^2.
        float g = _ball.Gravity;
        float t = PitchFlightSeconds;
        float vy0 = (PitchEndHeight - PitchReleaseHeight - 0.5f * g * t * t) / t;

        _ball.Launch(
            MoundScreenPosition,
            new Vector3(0f, PitchReleaseHeight, 0f),
            new Vector3(0f, vy0, vz),
            BallFlightMode.ToBatter);
    }
}
