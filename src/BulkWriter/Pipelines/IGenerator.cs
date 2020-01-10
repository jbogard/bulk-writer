using System.Collections.Generic;

namespace BulkWriter.Pipelines
{
    public interface IGenerator<out T>
    {
        IEnumerable<T> Select();
    }
}