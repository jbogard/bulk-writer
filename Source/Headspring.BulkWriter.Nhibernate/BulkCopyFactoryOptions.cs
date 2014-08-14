using System.Data;

namespace Headspring.BulkWriter.Nhibernate
{
    public struct BulkCopyFactoryOptions
    {
        public static readonly BulkCopyFactoryOptions Default = new BulkCopyFactoryOptions
        {
            IsolationLevel = IsolationLevel.Snapshot,
            BatchSize = 4096,
            // This value seemed to provided the best performance in my scenarios, but you may want to change this
            Timeout = int.MaxValue
        };

        public IsolationLevel IsolationLevel { get; set; }

        public int BatchSize { get; set; }

        public int Timeout { get; set; }

        public bool Equals(BulkCopyFactoryOptions other)
        {
            return (this.IsolationLevel == other.IsolationLevel) && (this.BatchSize == other.BatchSize) && (this.Timeout == other.Timeout);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BulkCopyFactoryOptions && this.Equals((BulkCopyFactoryOptions) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) this.IsolationLevel;
                hashCode = (hashCode*397) ^ this.BatchSize;
                hashCode = (hashCode*397) ^ this.Timeout;
                return hashCode;
            }
        }

        public static bool operator ==(BulkCopyFactoryOptions left, BulkCopyFactoryOptions right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BulkCopyFactoryOptions left, BulkCopyFactoryOptions right)
        {
            return !left.Equals(right);
        }
    }
}