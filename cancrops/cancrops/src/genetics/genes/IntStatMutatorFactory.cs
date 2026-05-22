using cancrops.src.genetics.abstractions;

namespace cancrops.src.genetics.genes
{
    public static class IntStatMutatorFactory
    {
        public const string DEFAULT       = "default";
        public const string NONE          = "none";
        public const string HILL_CLIMB    = "hill_climb";
        public const string GAUSSIAN      = "gaussian";
        public const string RESET_ON_LOW  = "reset_on_low";

        // Resolves a mutator by name. Unknown names fall back to default.
        public static IMutator<int> Create(string name)
        {
            switch ((name ?? "").ToLowerInvariant())
            {
                case NONE:         return new NoneIntStatMutator();
                case HILL_CLIMB:   return new HillClimbIntStatMutator();
                case GAUSSIAN:     return new GaussianIntStatMutator();
                case RESET_ON_LOW: return new ResetOnLowIntStatMutator();
                case DEFAULT:
                default:           return new IntStatMutator();
            }
        }
    }
}
