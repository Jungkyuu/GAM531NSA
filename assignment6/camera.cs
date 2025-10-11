using OpenTK.Mathematics;

namespace Assignment6_FPSCamera
{
    
    // FPS-style camera with yaw/pitch + FOV zoom.
  
    public class Camera
    {
        public Vector3 Position;

        public float Yaw;      // degrees
        public float Pitch;    // degrees, clamped
        public float Fov = 60f; // degrees (30~90)

        public Vector3 Front { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }

        public Camera(Vector3 startPos, float yawDeg = -90f, float pitchDeg = 0f)
        {
            Position = startPos;
            Yaw = yawDeg;
            Pitch = pitchDeg;
            UpdateBasis();
        }

        public void AddYawPitch(float dYawDeg, float dPitchDeg)
        {
            Yaw += dYawDeg;
            Pitch = MathHelper.Clamp(Pitch + dPitchDeg, -89f, 89f);
            UpdateBasis();
        }

        public void Zoom(float delta) // positive => zoom in (smaller FOV)
        {
            Fov = MathHelper.Clamp(Fov - delta, 30f, 90f);
        }

        public Matrix4 GetViewMatrix()
            => Matrix4.LookAt(Position, Position + Front, Up);

        public Matrix4 GetProjection(float aspect)
            => Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(Fov), aspect, 0.1f, 200f);

        private void UpdateBasis()
        {
            float yawR = MathHelper.DegreesToRadians(Yaw);
            float pitR = MathHelper.DegreesToRadians(Pitch);

            var front = new Vector3(
                MathF.Cos(pitR) * MathF.Cos(yawR),
                MathF.Sin(pitR),
                MathF.Cos(pitR) * MathF.Sin(yawR)
            );
            Front = Vector3.Normalize(front);
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
        public void LookAtTarget(Vector3 target)
        {
            var dir = Vector3.Normalize(target - Position);
            float yawR = MathF.Atan2(dir.Z, dir.X);
            float pitR = MathF.Asin(dir.Y);

            Yaw   = MathHelper.RadiansToDegrees(yawR);
            Pitch = MathHelper.Clamp(MathHelper.RadiansToDegrees(pitR), -89f, 89f);
            UpdateBasis();
        }
    }
}
