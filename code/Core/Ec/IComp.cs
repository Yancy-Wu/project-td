namespace Game.Core.Ec
{
    public interface IComp
    {
        static readonly int CompType;
        static readonly CompMeta Meta;
        internal RuntimeCompInfo CompInfo { get; }
        void InitFromDict(Dictionary<string, object> data);
        void OnActive();
        void OnInactive(bool willDestroy = false);
        void Destroy();
    }

    public struct CompMeta
    {
        public int[] DependCompTypes;

        public CompMeta(int[] dependCompTypes)
        {
            DependCompTypes = dependCompTypes;
        }
    }

    public class RuntimeCompInfo
    {
        public bool IsActive;
    }
}
