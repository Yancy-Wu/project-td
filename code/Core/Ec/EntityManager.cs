using System.Diagnostics;

namespace Game.Core.Ec
{
    internal class EntityManager
    {
        private readonly Dictionary<string, Entity> _entities = new();

        public void AddEntity(Entity entity)
        {
            Debug.Assert(!_entities.ContainsKey(entity.Id!), $"Cannot add duplicate entity: {entity.Id}");
            _entities[entity.Id!] = entity;
        }

        public void DestroyEntity(Entity entity)
        {
            Debug.Assert(_entities.ContainsKey(entity.Id!), $"Cannot destroy not exist entity: {entity.Id}");
            _entities.Remove(entity.Id!);
            entity.Destroy(false);
        }
    }
}
