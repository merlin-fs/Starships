using Game.Core.Prefabs;
using Game.Model.Logics;

using Unity.Entities;

namespace Game.Views
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class ViewComponentSystem : SystemBase
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAspect<Logic.Aspect>()
                .WithAll<PrefabInfo.ContextReference, Logic.ChangeTag>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic.ChangeTag>());
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            foreach (var (logic, context) in SystemAPI.Query<Logic.Aspect, PrefabInfo.ContextReference>()
                         .WithChangeFilter<Logic.ChangeTag>())
            {
                var view = context.Value.Resolve<IView>();
                foreach (var component in view.GetComponents<IViewLogicComponent>())
                {
                    component.ChangeAction(logic.Action);
                }
            }
        }
    }
}
