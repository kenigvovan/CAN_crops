using cancrops.src.genetics.abstractions;

namespace cancrops.src.genetics.genes
{
    // GenePair specialisation for int statistics that lets the configured StatTraitLogic
    // (MIN/MEAN/MAX) decide the expressed trait value.
    public class IntGenePair : GenePair<int>
    {
        public IntGenePair(IGene<int> gene, IAllele<int> first, IAllele<int> second)
            : base(gene, first, second) { }

        public override int GetTrait()
        {
            var logic = StatTraitLogicHelper.Parse(cancrops.config?.statTraitLogic);
            return StatTraitLogicHelper.Apply(logic, Dominant.Trait, Recessive.Trait);
        }
    }
}
