using EventStore.Core.Bus;
using EventStore.Core.Index.Hashes;
using EventStore.Core.Services.PersistentSubscription.ConsumerStrategy;

namespace EventStore.ConsumerStrategy.PinnedPerPartitionKey
{
    public class ConsumerStrategyFactory : IPersistentSubscriptionConsumerStrategyFactory
    {
        public ConsumerStrategyFactory(string strategyName)
        {
            StrategyName = strategyName;
        }

        public string StrategyName { get; private set; }

        public IPersistentSubscriptionConsumerStrategy Create(string subscriptionId, IPublisher mainQueue, ISubscriber mainBus)
        {
            return new PinnedPerPartitionKeyPersistentSubscriptionConsumerStrategy(new XXHashUnsafe());
        }
    }
}