namespace Headspring.BulkWriter.Nhibernate
{
    public class StaticPropertyValueGetter : IPropertyValueGetter
    {
        private readonly object value;

        public StaticPropertyValueGetter(object value)
        {
            this.value = value;
        }

        public object Get(object item)
        {
            return this.value;
        }
    }
}