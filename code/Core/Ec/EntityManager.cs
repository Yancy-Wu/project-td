using System.Diagnostics;

namespace Game.Core.Ec {
    internal class EntityManager {
        private readonly Dictionary<string, Entity> _entities = new();

        internal void AddEntity(Entity entity) {
            Debug.Assert(!_entities.ContainsKey(entity.Id!), $"Cannot add duplicate entity: {entity.Id}");
            _entities[entity.Id!] = entity;
        }

        internal void DestroyEntity(Entity entity) {
            Debug.Assert(_entities.ContainsKey(entity.Id!), $"Cannot destroy not exist entity: {entity.Id}");
            _entities.Remove(entity.Id!);
            entity.Destroy(false);
        }

        public Entity? GetEntity(string Id) {
            if (_entities.TryGetValue(Id, out Entity? val)) return (Entity?)val;
            return null;
        }
    }
}
