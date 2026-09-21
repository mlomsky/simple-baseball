using Godot;

namespace SimpleBaseball;

public enum BallFlightMode
{
    ToBatter,
    Batted
}

/// <summary>
/// Simulates ball flight on a virtual X/Y/Z axis (Z = depth away from the camera,
/// which sits at Z=0 — behind the batter/home plate, the classic Baseball-Stars
/// "watch the pitch come at you" angle) and projects it onto the 2D screen each
/// frame to fake perspective on a fixed camera.
/// </summary>
public partial class Ball : Node2D
{
    // Tuned in M1: how far scale/vertical offset shift between near (Z=0, at the
    // camera/plate) and far (Z=MaxZ, out at the mound).
    [Export] public float MaxZ = 400f;
    [Export] public float NearScale = 0.9f;
    [Export] public float FarScale = 0.45f;
    [Export] public float DepthYOffsetPixels = 105f;
    [Export] public float Gravity = -60f;

    public BallFlightMode Mode = BallFlightMode.ToBatter;

    private Vector2 _originScreenPos;
    private Vector3 _simPosition;
    private Vector3 _velocity;
    private bool _active;

    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public void Launch(Vector2 originScreenPos, Vector3 startPosition, Vector3 startVelocity, BallFlightMode mode)
    {
        Mode = mode;
        _originScreenPos = originScreenPos;
        _simPosition = startPosition;
        _velocity = startVelocity;
        _active = true;
        Visible = true;
        UpdateProjection();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active)
        {
            return;
        }

        float dt = (float)delta;
        _velocity += new Vector3(0f, Gravity, 0f) * dt;
        _simPosition += _velocity * dt;

        UpdateProjection();

        // Deactivate once the ball exits the [0, MaxZ] depth range in either
        // direction — a pitch travels MaxZ -> 0 (mound to plate), a batted ball
        // (added later) travels 0 -> MaxZ (plate out into the outfield).
        if (_simPosition.Z <= 0f || _simPosition.Z >= MaxZ)
        {
            _active = false;
        }
    }

    private void UpdateProjection()
    {
        float depthT = Mathf.Clamp(_simPosition.Z / MaxZ, 0f, 1f);
        float scale = Mathf.Lerp(NearScale, FarScale, depthT);

        Position = new Vector2(
            _originScreenPos.X + _simPosition.X * scale,
            _originScreenPos.Y - _simPosition.Y * scale - depthT * DepthYOffsetPixels);

        Scale = Vector2.One * scale;
    }
}
