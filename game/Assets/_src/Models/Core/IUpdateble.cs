using System;

namespace Game.Model
{
    /// <summary>
    /// Интерфейс для объектов, в которых вызывается Update
    /// </summary>
    public interface IUpdateble
    {
        void Update(float delta);
    }
}
