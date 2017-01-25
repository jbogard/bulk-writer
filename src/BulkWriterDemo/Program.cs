using System.Collections.Generic;
using BulkWriter;

namespace BulkWriterDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mapping = MapBuilder
                .MapAllProperties<MyDomainEntity>()
                .DestinationTable("MyDomainEntities")       // Optional
                .MapProperty(x => x.Id, x => x.DoNotMap())  // Required for ID properties where you want to use the DBs auto-increment feature.
                .Build();

            using (var bulkWriter = mapping.CreateBulkWriter("Data Source=(local);Initial Catalog=BulkWriterTest;Integrated Security=SSPI"))
            {
                var items = GetDomainEntities();
                bulkWriter.WriteToDatabase(items);
            }
        }

        private static IEnumerable<MyDomainEntity> GetDomainEntities()
        {
            for (int i = 0; i < 1000; i++)
            {
                yield return new MyDomainEntity();
            }
        } 
    }
}