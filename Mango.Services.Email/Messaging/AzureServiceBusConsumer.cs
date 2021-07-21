using Azure.Messaging.ServiceBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly EmailRepository _emailRepo;
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionEmail;
        private readonly string orderUpdatePaymentResultTopic;

        private ServiceBusProcessor orderUpdatePaymentProcessor;

        private readonly IConfiguration _configuration;
        public AzureServiceBusConsumer(EmailRepository emailRepo, IConfiguration configuration)
        {
            _emailRepo = emailRepo;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            subscriptionEmail = _configuration.GetValue<string>("SubscriptionName");
            orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");

            var client = new ServiceBusClient(serviceBusConnectionString);
            orderUpdatePaymentProcessor = client.CreateProcessor(orderUpdatePaymentResultTopic, subscriptionEmail);
        }

        public async Task Start()
        {
           
            orderUpdatePaymentProcessor.ProcessMessageAsync += OnPaymentUpdateReceviced;
            orderUpdatePaymentProcessor.ProcessErrorAsync += ErrorHandler;
            await orderUpdatePaymentProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
         
            await orderUpdatePaymentProcessor.StartProcessingAsync();
            await orderUpdatePaymentProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnPaymentUpdateReceviced(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var objeMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);



            try
            {
                await _emailRepo.SendAndLogEmail(objeMessage);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
     }
}
