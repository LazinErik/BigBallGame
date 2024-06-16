using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public abstract class Ball
{
    public float Radius { get; set; }
    public PointF Position { get; set; }
    public Color Color { get; set; }
    public PointF Direction { get; set; }

    protected Ball(float radius, PointF position, Color color, PointF direction)
    {
        Radius = radius;
        Position = position;
        Color = color;
        Direction = direction;
    }

    public abstract void OnCollision(Ball other, Canvas canvas);
}

public class RegularBall : Ball
{
    public RegularBall(float radius, PointF position, Color color, PointF direction)
        : base(radius, position, color, direction) { }

    public override void OnCollision(Ball other, Canvas canvas)
    {
        if (other is RegularBall)
        {
            RegularBall otherRegular = (RegularBall)other;
            if (this.Radius > otherRegular.Radius)
            {
                this.Radius += otherRegular.Radius;
                this.Color = CombineColors(this.Color, this.Radius, otherRegular.Color, otherRegular.Radius);
                canvas.RemoveBall(other);
            }
        }
        else if (other is MonsterBall)
        {
            other.Radius += this.Radius;
            canvas.RemoveBall(this);
        }
        else if (other is RepelentBall)
        {
            other.Color = this.Color;
            this.Direction = new PointF(-this.Direction.X, -this.Direction.Y);
        }
    }

    private Color CombineColors(Color color1, float radius1, Color color2, float radius2)
    {
        float totalRadius = radius1 + radius2;
        int r = (int)((color1.R * radius1 + color2.R * radius2) / totalRadius);
        int g = (int)((color1.G * radius1 + color2.G * radius2) / totalRadius);
        int b = (int)((color1.B * radius1 + color2.B * radius2) / totalRadius);
        return Color.FromArgb(r, g, b);
    }
}

public class MonsterBall : Ball
{
    public MonsterBall(float radius, PointF position, Color color)
        : base(radius, position, color, new PointF(0, 0)) { }

    public override void OnCollision(Ball other, Canvas canvas)
    {
        if (other is RegularBall || other is RepelentBall)
        {
            this.Radius += other.Radius;
            canvas.RemoveBall(other);
        }
    }
}

public class RepelentBall : Ball
{
    public RepelentBall(float radius, PointF position, Color color, PointF direction)
        : base(radius, position, color, direction) { }

    public override void OnCollision(Ball other, Canvas canvas)
    {
        if (other is RegularBall)
        {
            this.Color = other.Color;
            other.Direction = new PointF(-other.Direction.X, -other.Direction.Y);
        }
        else if (other is RepelentBall)
        {
            Color temp = this.Color;
            this.Color = other.Color;
            other.Color = temp;
        }
        else if (other is MonsterBall)
        {
            this.Radius /= 2;
        }
    }
}

public class Canvas
{
    public float Width { get; }
    public float Height { get; }
    public List<Ball> Balls { get; }

    public Canvas(float width, float height)
    {
        Width = width;
        Height = height;
        Balls = new List<Ball>();
    }

    public void AddBall(Ball ball)
    {
        Balls.Add(ball);
    }

    public void RemoveBall(Ball ball)
    {
        Balls.Remove(ball);
    }

    public void Update()
    {
        foreach (var ball in Balls)
        {
            UpdateBallPosition(ball);
            CheckCollisions(ball);
        }
    }

    private void UpdateBallPosition(Ball ball)
    {
        ball.Position = new PointF(ball.Position.X + ball.Direction.X, ball.Position.Y + ball.Direction.Y);
        if (ball.Position.X - ball.Radius < 0 || ball.Position.X + ball.Radius > Width)
        {
            ball.Direction = new PointF(-ball.Direction.X, ball.Direction.Y);
        }
        if (ball.Position.Y - ball.Radius < 0 || ball.Position.Y + ball.Radius > Height)
        {
            ball.Direction = new PointF(ball.Direction.X, -ball.Direction.Y);
        }
    }

    private void CheckCollisions(Ball ball)
    {
        foreach (var other in Balls)
        {
            if (ball != other && AreColliding(ball, other))
            {
                ball.OnCollision(other, this);
            }
        }
    }

    private bool AreColliding(Ball ball1, Ball ball2)
    {
        float dx = ball1.Position.X - ball2.Position.X;
        float dy = ball1.Position.Y - ball2.Position.Y;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);
        return distance < ball1.Radius + ball2.Radius;
    }
}

public class Simulation
{
    private Canvas _canvas;

    public Simulation(float canvasWidth, float canvasHeight, int regularBalls, int monsterBalls, int repelentBalls)
    {
        _canvas = new Canvas(canvasWidth, canvasHeight);
        InitializeBalls(regularBalls, monsterBalls, repelentBalls);
    }

    private void InitializeBalls(int regularBalls, int monsterBalls, int repelentBalls)
    {
        Random rand = new Random();
        for (int i = 0; i < regularBalls; i++)
        {
            _canvas.AddBall(new RegularBall(rand.Next(5, 15), RandomPosition(), RandomColor(), RandomDirection()));
        }
        for (int i = 0; i < monsterBalls; i++)
        {
            _canvas.AddBall(new MonsterBall(rand.Next(15, 25), RandomPosition(), RandomColor()));
        }
        for (int i = 0; i < repelentBalls; i++)
        {
            _canvas.AddBall(new RepelentBall(rand.Next(5, 15), RandomPosition(), RandomColor(), RandomDirection()));
        }
    }

    private PointF RandomPosition()
    {
        Random rand = new Random();
        return new PointF(rand.Next(0, (int)_canvas.Width), rand.Next(0, (int)_canvas.Height));
    }

    private Color RandomColor()
    {
        Random rand = new Random();
        return Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
    }

    private PointF RandomDirection()
    {
        Random rand = new Random();
        return new PointF((float)(rand.NextDouble() * 2 - 1), (float)(rand.NextDouble() * 2 - 1));
    }

    public void Run()
    {
        while (_canvas.Balls.Any(ball => ball is RegularBall))
        {
            _canvas.Update();
            System.Threading.Thread.Sleep(100); 
        }
    }
}
