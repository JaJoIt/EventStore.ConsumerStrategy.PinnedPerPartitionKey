using System.ComponentModel.Composition;
using EventStore.Core.PluginModel;
using EventStore.Core.Services.PersistentSubscription.ConsumerStrategy;

namespace EventStore.ConsumerStrategy.PinnedPerPartitionKey
{
    [Export(typeof(IPersistentSubscriptionConsumerStrategyPlugin))]
    public class ConsumerStrategyPlugin : IPersistentSubscriptionConsumerStrategyPlugin
    {
        public string Name { get { return "PinnedPerPartitionKey"; } }
        public string Version { get { return "1.0.0"; } }

        public IPersistentSubscriptionConsumerStrategyFactory GetConsumerStrategyFactory()
        {
            return new ConsumerStrategyFactory(Name);
        }
    }
}