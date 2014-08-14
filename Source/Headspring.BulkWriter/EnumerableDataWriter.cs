using System;
using System.Collections;
using System.Threading.Tasks;

namespace Headspring.BulkWriter
{
    public static class EnumerableDataWriter
    {
        public static void WriteToDatabase(IEnumerable items, IBulkCopyFactory bulkCopyFactory)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }

            if (null == bulkCopyFactory)
            {
                throw new ArgumentNullException("bulkCopyFactory");
            }

            IEnumerator enumerator = items.GetEnumerator();
            if (enumerator.MoveNext())
            {
                IPropertyToOrdinalMappings mappings;
                using (IBulkCopy bulkCopy = bulkCopyFactory.Create(enumerator.Current, out mappings))
                {
                    using (var dataReader = new EnumeratorDataReader(enumerator.Current, enumerator, mappings))
                    {
                        bulkCopy.WriteToServer(dataReader);
                    }
                }
            }
        }

        public static async Task WriteToDatabaseAsync(IEnumerable items, IBulkCopyFactory bulkCopyFactory)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }

            if (null == bulkCopyFactory)
            {
                throw new ArgumentNullException("bulkCopyFactory");
            }
            
            IEnumerator enumerator = items.GetEnumerator();
            if (enumerator.MoveNext())
            {
                IPropertyToOrdinalMappings mappings;
                using (IBulkCopy bulkCopy = bulkCopyFactory.Create(enumerator.Current, out mappings))
                {
                    using (var dataReader = new EnumeratorDataReader(enumerator.Current, enumerator, mappings))
                    {
                        await bulkCopy.WriteToServerAsync(dataReader);
                    }
                }
            }
        }
    }
}