using System.Runtime.CompilerServices;
using Unity.Animation;



[assembly: InternalsVisibleTo("Unity.Animation.Graph.Hybrid")]
[assembly: InternalsVisibleTo("Unity.Animation.Hybrid")]
[assembly: InternalsVisibleTo("Unity.Animation.Authoring")]
[assembly: InternalsVisibleTo("Unity.Animation.Hybrid.Tests")]
[assembly: InternalsVisibleTo("Unity.Animation.Editor")]
[assembly: InternalsVisibleTo("Unity.Animation.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.Animation.Tests")]
[assembly: InternalsVisibleTo("Unity.Animation.PerformanceTests")]
[assembly: InternalsVisibleTo("Unity.Animation.Graph")]
[assembly: InternalsVisibleTo("Unity.Animation.DefaultGraphPipeline")]
[assembly: InternalsVisibleTo("BurstCompatibilityTests")]

[assembly: Unity.Jobs.RegisterGenericJobType(typeof(SortReadTransformComponentJob<NotSupportedTransformHandle>))]
[assembly: Unity.Jobs.RegisterGenericJobType(typeof(ReadTransformComponentJob<NotSupportedTransformHandle>))]
[assembly: Unity.Jobs.RegisterGenericJobType(typeof(ReadRootTransformJob<NotSupportedRootMotion>))]
[assembly: Unity.Jobs.RegisterGenericJobType(typeof(UpdateRootRemapMatrixJob<NotSupportedRootMotion>))]
[assembly: Unity.Jobs.RegisterGenericJobType(typeof(WriteRootTransformJob<NotSupportedRootMotion>))]
[assembly: Unity.Jobs.RegisterGenericJobType(typeof(AccumulateRootTransformJob<NotSupportedRootMotion>))]
[assembly: Unity.Jobs.RegisterGenericJobType(typeof(WriteTransformComponentJob<NotSupportedTransformHandle>))]
