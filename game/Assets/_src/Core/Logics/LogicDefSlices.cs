using System;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial class LogicDef
        {
            [SerializeReference, ReferenceSelect(typeof(ISlice))]
            private ISlice[] slices;
            
            public void ExecuteCodeLogic(ref SliceContext context)
            {
                if (slices == null || slices.Length == 0) return;
                foreach (var iter in slices)
                {
                    if (iter.IsConditionHit(ref context))
                        iter.Execute(ref context);
                }
            }
        }
    }
}