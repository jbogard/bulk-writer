namespace Headspring.BulkWriter.Nhibernate
{
    public interface IPropertyValueGetter
    {
        object Get(object item);
    }
}