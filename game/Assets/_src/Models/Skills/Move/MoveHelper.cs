using Game.Model.Worlds;
using Unity.Mathematics;

namespace Game.Model
{
    public partial struct Move
    {
        private static float3 UP = new float3(0f, 1f, 0f);
        
        /*
        public static bool MoveToPoint(float delta, ref Move moving, Aspect aspect)
        {
            if (moving.Travel >= 1f)
            {
                return true;
            }
            
            float speed = moving.Speed * aspect.PathInfo.DeltaTime;
            moving.Travel += delta * speed;

            //float time = moving.PathPrecent;
            float time = Map.Path.ConvertToConstantPathTime(moving.Travel, aspect.PathInfo.Length, aspect.PathTimes.AsNativeArray());
            float3 position = Map.Path.GetPosition(time, false, aspect.PathPoints.AsNativeArray(), aspect.PathInfo.DeltaTime);

            //var look = math.normalize(position - aspect.LocalTransformRO.Position);
            var look = position - aspect.LocalTransformRO.Position;
            //look.y = -0.5f;

            var rotation  = quaternion.LookRotation(look, UP);
            var transform = aspect.LocalTransformRO;
            //rotation.Value = quaternion.LookRotation(position - translation.Value, UP);
            //aspect.LocalTransformRW.Position = position;
            //aspect.LocalTransformRW.Rotation = rotation;
            transform.Position = position;
            transform.Rotation = rotation;
            aspect.LocalTransformRW = transform;
            return false;
        }
        */
    }
}