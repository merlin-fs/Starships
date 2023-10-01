using System;
using Game.Model.Worlds;

using ProjectDawn.Navigation;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine.AI;

namespace Game.Model
{
    public partial struct Move
    {
        public readonly partial struct Aspect: IAspect
        {
            private readonly Entity m_Self;
            //private readonly RefRW<Move> m_Data;
            private readonly RefRW<Target> m_Target;
            private readonly RefRW<Map.Transform> m_MapTransform;
            private readonly RefRW<LocalTransform> m_LocalTransform;
            
            private readonly RefRW<Map.Path.Info> m_PathInfo;
            private readonly DynamicBuffer<Map.Path.Points> m_PathPoints;
            private readonly DynamicBuffer<Map.Path.Times> m_PathTimes;
            private readonly RefRW<AgentBody> m_AgentBody;
            private readonly RefRW<AgentSteering> m_AgentSteering;

            public AgentBody Agent => m_AgentBody.ValueRO;
            public AgentSteering AgentSteering => m_AgentSteering.ValueRO;
            public int2 Target { get => m_Target.ValueRO.Value; set => m_Target.ValueRW.Value = value; }
            public Map.Transform Transform => m_MapTransform.ValueRO;
            public ref readonly LocalTransform LocalTransformRO => ref m_LocalTransform.ValueRO;
            public ref LocalTransform LocalTransformRW => ref m_LocalTransform.ValueRW;
            public ref readonly Map.Path.Info PathInfo => ref m_PathInfo.ValueRO; 
            public DynamicBuffer<Map.Path.Points> PathPoints => m_PathPoints;
            public DynamicBuffer<Map.Path.Times> PathTimes => m_PathTimes;

            public bool SetTarget(float3 value, float speed)
            {
                m_AgentBody.ValueRW.Destination = value;
                m_AgentBody.ValueRW.IsStopped = false;
                m_AgentSteering.ValueRW.Speed = speed;
                return true;
            }
            
            
            public bool SetPath(NativeArray<int2> path, Map.Data map)
            {
                var src = new NavMeshBuildSource() 
                {
                    shape = NavMeshBuildSourceShape.Box,
                    //transform = 
                };
                //new NavMeshSourceData()
                //NavMeshSourceData
                //NavMesh.AddLink(
                //NavMeshBuilder.CollectSources(null, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                //NavMeshBuildSource
                //NavMeshCollectGeometry.RenderMeshes
                //NavMeshBuilder.BuildNavMeshData()

                if (path.Length < 2) return false;
                
                m_PathPoints.ResizeUninitialized(path.Length);
                ref var info = ref m_PathInfo.ValueRW;

                int timeLen = 100;
                float step = 1f / timeLen;
                m_PathTimes.ResizeUninitialized(timeLen);

                for (int j = 0; j < path.Length; j++)
                    m_PathPoints.ElementAt(j).Value = map.MapToWord(path[j]);
                info.DeltaTime = 1f / (m_PathPoints.Length - 1);
                
                var pts = m_PathPoints.AsNativeArray();
                float len = 0f;
                float3 vector = Map.Path.GetPosition(0f, false, pts, info.DeltaTime);
                for (int j = 0; j < timeLen; j++)
                {
                    float pos = step * (j + 1);
                    float3 point = Map.Path.GetPosition(pos, false, pts, info.DeltaTime);
                    len += math.distance(vector, point);
                    vector = point;

                    m_PathTimes.ElementAt(j) = new Map.Path.Times()
                    {
                        Time = pos,
                        Length = len,
                    };
                };
                info.Length = len;
                return true;
            }
        }
    }
}