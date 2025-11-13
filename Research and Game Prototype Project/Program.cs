using System;
using System.Numerics;
using Raylib_cs;

namespace BreakoutCollisionDemo
{
    struct GameObject
    {
        public Vector2 Position;   // top-left
        public Vector2 Size;       // width, height
        public bool Destroyed;
        public bool Solid;
    }

    struct BallObject
    {
        public Vector2 Position;   // top-left of the ball sprite
        public float Radius;
        public Vector2 Velocity;
    }

    class Program
    {
        // Small helper, in case your .NET target does not have Math.Clamp
        static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // AABB - AABB collision (not used heavily, but available for testing)
        static bool CheckCollisionAABB(GameObject a, GameObject b)
        {
            bool collisionX = a.Position.X + a.Size.X >= b.Position.X &&
                              b.Position.X + b.Size.X >= a.Position.X;

            bool collisionY = a.Position.Y + a.Size.Y >= b.Position.Y &&
                              b.Position.Y + b.Size.Y >= a.Position.Y;

            return collisionX && collisionY;
        }

        // Circle - AABB collision
        static bool CheckCollisionCircleAABB(BallObject ball, GameObject box)
        {
            // Circle center
            Vector2 center = new Vector2(
                ball.Position.X + ball.Radius,
                ball.Position.Y + ball.Radius
            );

            // AABB center and half extents
            Vector2 halfExtents = box.Size * 0.5f;
            Vector2 boxCenter = box.Position + halfExtents;

            // Difference between centers
            Vector2 diff = center - boxCenter;

            // Clamp difference to the extents of the box
            float clampedX = Clamp(diff.X, -halfExtents.X, halfExtents.X);
            float clampedY = Clamp(diff.Y, -halfExtents.Y, halfExtents.Y);
            Vector2 clamped = new Vector2(clampedX, clampedY);

            // Closest point on box to circle
            Vector2 closest = boxCenter + clamped;

            // Vector from circle center to closest point
            Vector2 delta = closest - center;

            float distanceSquared = Vector2.Dot(delta, delta);
            return distanceSquared <= ball.Radius * ball.Radius;
        }

        static void Main(string[] args)
        {
            const int screenWidth = 800;
            const int screenHeight = 600;

            Raylib.InitWindow(screenWidth, screenHeight, "2D Collision - Breakout Prototype (C# + Raylib_cs)");
            Raylib.SetTargetFPS(60);

            // GAME OVER Flag
            bool gameOver = false;

            // Paddle
            GameObject paddle = new GameObject();
            paddle.Size = new Vector2(100, 20);
            paddle.Position = new Vector2(
                (screenWidth - paddle.Size.X) * 0.5f,
                screenHeight - 60
            );
            paddle.Destroyed = false;
            paddle.Solid = true;

            // Ball
            BallObject ball = new BallObject();
            ball.Radius = 10.0f;
            ball.Position = new Vector2(
                screenWidth * 0.5f - ball.Radius,
                screenHeight * 0.5f - ball.Radius
            );
            ball.Velocity = new Vector2(200.0f, -250.0f);

            // Bricks (level)
            const int bricksCols = 10;
            const int bricksRows = 5;
            GameObject[] bricks = new GameObject[bricksRows * bricksCols];

            float brickWidth = 60.0f;
            float brickHeight = 20.0f;
            float offsetX = 50.0f;
            float offsetY = 50.0f;
            float spacing = 8.0f;

            for (int row = 0; row < bricksRows; row++)
            {
                for (int col = 0; col < bricksCols; col++)
                {
                    GameObject brick = new GameObject();
                    brick.Size = new Vector2(brickWidth, brickHeight);
                    brick.Position = new Vector2(
                        offsetX + col * (brickWidth + spacing),
                        offsetY + row * (brickHeight + spacing)
                    );
                    brick.Destroyed = false;
                    brick.Solid = false;
                    bricks[row * bricksCols + col] = brick;
                }
            }

            // Game loop
            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                // Not GameOver
                if (!gameOver)
                {
                    // Input: paddle movement
                    float paddleSpeed = 400.0f;
                    if (Raylib.IsKeyDown(KeyboardKey.Left))
                    {
                        paddle.Position.X -= paddleSpeed * dt;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.Right))
                    {
                        paddle.Position.X += paddleSpeed * dt;
                    }

                    // Clamp paddle within screen
                    if (paddle.Position.X < 0) paddle.Position.X = 0;
                    if (paddle.Position.X + paddle.Size.X > screenWidth)
                    {
                        paddle.Position.X = screenWidth - paddle.Size.X;
                    }

                    // Update ball
                    ball.Position += ball.Velocity * dt;

                    // Screen bounds collision (left/right)
                    if (ball.Position.X <= 0)
                    {
                        ball.Position.X = 0;
                        ball.Velocity.X *= -1.0f;
                    }
                    if (ball.Position.X + ball.Radius * 2 >= screenWidth)
                    {
                        ball.Position.X = screenWidth - ball.Radius * 2;
                        ball.Velocity.X *= -1.0f;
                    }

                    // Top
                    if (ball.Position.Y <= 0)
                    {
                        ball.Position.Y = 0;
                        ball.Velocity.Y *= -1.0f;
                    }

                    // Bottom → GAME OVER
                    if (ball.Position.Y >= screenHeight)
                    {
                        gameOver = true;
                    }

                    // Collision: ball vs paddle                
                    if (CheckCollisionCircleAABB(ball, paddle))
                    {
                        // Bounce upward
                        ball.Velocity.Y = -MathF.Abs(ball.Velocity.Y);

                        float paddleCenter = paddle.Position.X + paddle.Size.X * 0.5f;
                        float ballCenterX = ball.Position.X + ball.Radius;
                        float diff = (ballCenterX - paddleCenter) / (paddle.Size.X * 0.5f);
                        ball.Velocity.X = diff * 300.0f;
                    }

                    // Collision: ball vs bricks
                    for (int i = 0; i < bricks.Length; i++)
                    {
                        if (!bricks[i].Destroyed)
                        {
                            if (CheckCollisionCircleAABB(ball, bricks[i]))
                            {
                                if (!bricks[i].Solid)
                                {
                                    bricks[i].Destroyed = true;
                                }
                                // Simple bounce: invert vertical velocity
                                ball.Velocity.Y *= -1.0f;
                                break; // only handle one brick per frame
                            }
                        }
                    }
                }

                // --------------------------
                // DRAWING
                // --------------------------
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                // Paddle
                Raylib.DrawRectangleV(paddle.Position, paddle.Size, Color.Blue);

                // Ball
                Vector2 ballCenter = new Vector2(
                    ball.Position.X + ball.Radius,
                    ball.Position.Y + ball.Radius
                );
                Raylib.DrawCircleV(ballCenter, ball.Radius, Color.Yellow);

                // Bricks
                for (int i = 0; i < bricks.Length; i++)
                {
                    if (!bricks[i].Destroyed)
                    {
                        Color col = bricks[i].Solid ? Color.Gray : Color.Red;
                        Raylib.DrawRectangleV(bricks[i].Position, bricks[i].Size, col);
                    }
                }

                // UI
                Raylib.DrawText("2D Collision: AABB & Circle-AABB Demo", 10, 10, 20, Color.White);
                Raylib.DrawText("Use LEFT/RIGHT to move the paddle", 10, 40, 18, Color.White);

                if (gameOver)
                {
                    string msg = "GAME OVER";
                    int fontSize = 50;
                    int textWidth = Raylib.MeasureText(msg, fontSize);
                    int x = (screenWidth - textWidth) / 2;
                    int y = screenHeight / 2 - fontSize;

                    Raylib.DrawText(msg, x, y, fontSize, Color.Red);

                    string sub = "Close the window to exit";
                    int subSize = 20;
                    int subWidth = Raylib.MeasureText(sub, subSize);
                    int sx = (screenWidth - subWidth) / 2;
                    int sy = y + fontSize + 20;
                    Raylib.DrawText(sub, sx, sy, subSize, Color.RayWhite);
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
