namespace cancrops.src.genetics.conditions
{
    public enum EnumConditionResult
    {
        // Condition not met — mutation must NOT happen, overrides chance and FORCE.
        FORBID = 0,

        // Condition met under normal terms — chance roll proceeds as usual.
        PASS = 1,

        // Condition met under "perfect" terms — mutation skips chance roll and is forced.
        FORCE = 2
    }
}
