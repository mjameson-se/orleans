using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.TestingHost;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.StreamingTests
{
    internal class StreamTestUtils
    {
        public const string AZURE_QUEUE_STREAM_PROVIDER_NAME = "AzureQueueProvider";

        internal static void LogStartTest(string testName, Guid streamId, string streamProviderName, ILogger logger, TestCluster siloHost)
        {
            SiloAddress primSilo = siloHost.Primary?.SiloAddress;
            SiloAddress secSilo = siloHost.SecondarySilos.FirstOrDefault()?.SiloAddress;
            logger.Info("\n\n**START********************** {0} ********************************* \n\n"
                        + "Running with initial silos Primary={1} Secondary={2} StreamId={3} StreamType={4} \n\n",
                testName, primSilo, secSilo, streamId, streamProviderName);
        }

        internal static void LogEndTest(string testName, ILogger logger)
        {
            logger.Info("\n\n--END------------------------ {0} --------------------------------- \n\n", testName);
        }

        internal static IStreamPubSub GetStreamPubSub(IInternalClusterClient client)
        {
            var runtime = client.ServiceProvider.GetRequiredService<IStreamProviderRuntime>();
            return runtime.PubSub(StreamPubSubType.ExplicitGrainBasedAndImplicit);
        }

        internal static async Task CheckPubSubCounts(IInternalClusterClient client, ITestOutputHelper output, string when, int expectedPublisherCount, int expectedConsumerCount, Guid streamIdGuid, string streamProviderName, string streamNamespace)
        {
            var pubSub = GetStreamPubSub(client);
            var streamId = new InternalStreamId(streamProviderName, StreamId.Create(streamNamespace, streamIdGuid));

            int consumerCount = await pubSub.ConsumerCount(streamId);

            Assert_AreEqual(output, expectedConsumerCount, consumerCount, "{0} - ConsumerCount for stream {1} = {2}",
                when, streamId, consumerCount);

            int publisherCount = await pubSub.ProducerCount(streamId);

            Assert_AreEqual(output, expectedPublisherCount, publisherCount, "{0} - PublisherCount for stream {1} = {2}",
                when, streamId, publisherCount);
        }

        internal static void Assert_AreEqual(ITestOutputHelper output, int expected, int actual, string msg, params object[] args)
        {
            // expected == -1 means don't care / don't assert check value.
            string prefix = expected == -1 ? "Not-checked" : actual == expected ? "True" : "FALSE";
            string fmtMsg = String.Format("--> {0}: ", prefix) + String.Format(msg, args);
            output.WriteLine(fmtMsg);
            if (expected != -1)
            {
                Assert.Equal(expected, actual);
            }
        }
    }
}