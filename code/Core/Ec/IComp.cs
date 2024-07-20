using Game.Core.Meta;

namespace Game.Core.Ec {
    public interface IComp {
        static readonly int CompType;
        static readonly CompMeta Meta;
        internal RuntimeCompInfo CompInfo { get; }
        void OnActive();
        void OnInactive(bool willDestroy = false);
        void Destroy();
    }

    public class RuntimeCompInfo {
        public bool IsActive = false;
    }
}
