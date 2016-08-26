using System;

namespace EventStore.ConsumerStrategy.PinnedPerPartitionKey
{
    internal struct BucketAssignment
    {
        internal enum BucketState
        {
            Unassigned,
            Assigned,
            Disconnected
        }

        public Guid NodeId { get; set; }

        public BucketState State { get; set; }

        public int InFlightCount { get; set; }

        public Node Node { get; set; }
    }
}