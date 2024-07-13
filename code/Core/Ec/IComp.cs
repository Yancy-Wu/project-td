namespace Game.Core.Ec
{
    public interface IComp
    {
        static readonly int CompType;
        static readonly int[] DependCompTypes = Array.Empty<int>();
        internal RuntimeCompInfo CompInfo { get; }
        void InitFromDict(Dictionary<string, object> data);
        void OnActive();
        void OnInactive(bool willDestroy = false);
        void Destroy();
    }

    public class RuntimeCompInfo
    {
        public bool IsActive;
    }
}
