using Godot;
using System;

public partial class Bullet : Area2D
{
    [Export] public float Speed = 500f;
    public Vector2 Direction = Vector2.Right; // hướng bắn

    private Sprite2D sprite;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;

        sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        if (Direction.X < 0 && sprite != null)
            sprite.FlipH = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Direction * Speed * (float)delta;

        if (GlobalPosition.X < -2000 || GlobalPosition.X > 2000)
            QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("zombie"))
        {
            if (body is ZombieEnemy zombie)
                zombie.TakeDamage(1);

            QueueFree();
        }
    }
}
