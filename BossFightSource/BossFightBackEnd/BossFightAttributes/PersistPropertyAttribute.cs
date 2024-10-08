namespace BossFight
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public sealed class PersistPropertyAttribute : System.Attribute
    {
        public bool IsIdProperty { get; set; }

        public PersistPropertyAttribute(bool pIsIdProperty = false)
        {
            IsIdProperty = pIsIdProperty;
        }
    }
}
