using System;
using System.Collections.Generic;
using EventStore.Common.Utils;
using EventStore.Core.Data;
using EventStore.Core.Index.Hashes;
using EventStore.Core.Services.PersistentSubscription;
using EventStore.Core.Services.PersistentSubscription.ConsumerStrategy;

namespace EventStore.ConsumerStrategy.PinnedPerPartitionKey
{
    public class PinnedPerPartitionKeyPersistentSubscriptionConsumerStrategy : IPersistentSubscriptionConsumerStrategy
    {
        private readonly IHasher _hash;
        private readonly PinnedConsumerState _state = new PinnedConsumerState();
        private readonly object _stateLock = new object();

        public PinnedPerPartitionKeyPersistentSubscriptionConsumerStrategy(IHasher hasher)
        {
            _hash = hasher;
        }

        public string Name => "PinnedPerPartitionKey";

        public int AvailableCapacity => _state?.AvailableCapacity ?? 0;

        public void ClientAdded(PersistentSubscriptionClient client)
        {
            lock (_stateLock)
            {
                var newNode = new Node(client);

                _state.AddNode(newNode);

                client.EventConfirmed += OnEventRemoved;
            }
        }

        public void ClientRemoved(PersistentSubscriptionClient client)
        {
            var nodeId = client.CorrelationId;

            client.EventConfirmed -= OnEventRemoved;

            _state.DisconnectNode(nodeId);
        }

        public ConsumerPushResult PushMessageToClient(ResolvedEvent ev)
        {
            if (_state == null)
            {
                return ConsumerPushResult.NoMoreCapacity;
            }

            if (_state.AvailableCapacity == 0)
            {
                return ConsumerPushResult.NoMoreCapacity;
            }

            var bucket = GetAssignmentId(ev);

            if (_state.Assignments[bucket].State != BucketAssignment.BucketState.Assigned)
            {
                _state.AssignBucket(bucket);
            }

            if (!_state.Assignments[bucket].Node.Client.Push(ev))
            {
                return ConsumerPushResult.Skipped;
            }

            _state.RecordEventSent(bucket);
            return ConsumerPushResult.Sent;
        }

        private void OnEventRemoved(PersistentSubscriptionClient client, ResolvedEvent ev)
        {
            var assignmentId = GetAssignmentId(ev);
            _state.EventRemoved(client.CorrelationId, assignmentId);
        }

        private uint GetAssignmentId(ResolvedEvent ev)
        {
            var metaData = ev.OriginalEvent.Metadata.ParseJson<IDictionary<string, object>>();

            var partitionKey = Guid.NewGuid().ToString();

            if (metaData.ContainsKey("PartitionKey"))
                partitionKey = (metaData["PartitionKey"] ?? "").ToString();

            return _hash.Hash(partitionKey) % (uint)_state.Assignments.Length;
        }
    }
}