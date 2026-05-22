namespace cancrops.src.genetics
{
    public enum StatTraitLogic
    {
        MIN,
        MEAN,
        MAX
    }

    public static class StatTraitLogicHelper
    {
        public static int Apply(StatTraitLogic logic, int dominant, int recessive)
        {
            switch (logic)
            {
                case StatTraitLogic.MIN:  return dominant < recessive ? dominant : recessive;
                case StatTraitLogic.MEAN: return (dominant + recessive) / 2;
                case StatTraitLogic.MAX:  return dominant > recessive ? dominant : recessive;
                default: return dominant;
            }
        }

        public static StatTraitLogic Parse(string value)
        {
            if (string.IsNullOrEmpty(value)) return StatTraitLogic.MAX;
            switch (value.ToUpperInvariant())
            {
                case "MIN":  return StatTraitLogic.MIN;
                case "MEAN": return StatTraitLogic.MEAN;
                case "MAX":  return StatTraitLogic.MAX;
                default:     return StatTraitLogic.MAX;
            }
        }
    }
}
