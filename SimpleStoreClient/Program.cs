using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.Fabric;
using Common;

namespace SimpleStoreClient
{
    internal static class Program
    {
        private static NetTcpBinding CreateClientConnectionBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxReceivedMessageSize = 1024 * 1024
            };
            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            return binding;
        }

        private static void PrintPartition(Client client)
        {
            ResolvedServicePartition partition;
            if (client.TryGetLastResolvedServicePartition(out partition))
            {
                Console.WriteLine("Partition ID: " + partition.Info.Id);
            }
        }


        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        static void Main(string[] args)
        {
            Uri ServiceName = new Uri("fabric:/SimpleStoreApplication/ShoppingCartService");
            ServicePartitionResolver serviceResolver = new ServicePartitionResolver(() =>
            new FabricClient());
            NetTcpBinding binding = CreateClientConnectionBinding();
            Client shoppingClient = new Client(new WcfCommunicationClientFactory<IShoppingCartService>(serviceResolver, binding, null), ServiceName);
            for (int i = 0; i < 10; i++)
            {
                Client shoppingClient = new Client(new WcfCommunicationClientFactory<IShoppingCartService>
                (serviceResolver, binding, null),
                 ServiceName, i);
                shoppingClient.AddItem(new ShoppingCartItem
                {
                    ProductName = "XBOX ONE (" + i.ToString() + ")",
                    UnitPrice = 329.0,
                    Amount = 2
                }).Wait();
                shoppingClient.AddItem(new ShoppingCartItem
                {
                    ProductName = "Halo 5 (" + i.ToString() + ")",
                    UnitPrice = 59.99,
                    Amount = 1
                }).Wait();
                PrintPartition(shoppingClient);
                var list = shoppingClient.GetItems().Result;
                foreach (var item in list)
                {
                    Console.WriteLine(string.Format("{0}: {1:C2} X {2} = {3:C2}",
                    item.ProductName,
                    item.UnitPrice,
                    item.Amount,
                    item.LineTotal));
                }
            }
        }

    }
}
