using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly OrderRepository _orderRepository;
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionCheckOut;
        private readonly string checkoutMessageTopic;
        private readonly string orderPaymentProcessTopic;
        private readonly string orderUpdatePaymentResultTopic;

        private ServiceBusProcessor checkoutProcessor;
        private ServiceBusProcessor orderUpdatePaymentProcessor;

        private readonly IConfiguration _configuration;

        private readonly IMessageBus _messageBus;
        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            subscriptionCheckOut = _configuration.GetValue<string>("SubscriptionCheckoutName");
            checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");

            var client = new ServiceBusClient(serviceBusConnectionString);
            checkoutProcessor = client.CreateProcessor(checkoutMessageTopic, subscriptionCheckOut);
            orderUpdatePaymentProcessor = client.CreateProcessor(orderUpdatePaymentResultTopic, subscriptionCheckOut);
            _messageBus = messageBus;
        }

        public async Task Start()
        {
            checkoutProcessor.ProcessMessageAsync += OnCheckOutMessageReceviced;
            checkoutProcessor.ProcessErrorAsync += ErrorHandler;
            await checkoutProcessor.StartProcessingAsync();

            orderUpdatePaymentProcessor.ProcessMessageAsync += OnPaymentUpdateReceviced;
            orderUpdatePaymentProcessor.ProcessErrorAsync += ErrorHandler;
            await orderUpdatePaymentProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
            await checkoutProcessor.StartProcessingAsync();
            await checkoutProcessor.DisposeAsync();

            await orderUpdatePaymentProcessor.StartProcessingAsync();
            await orderUpdatePaymentProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }
        private async Task OnCheckOutMessageReceviced(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);
            var orderHeader = new OrderHeader
            {
                UserId = checkoutHeaderDto.UserId,
                CardNumber = checkoutHeaderDto.CardNumber,
                CouponCode = checkoutHeaderDto.CouponCode,
                CVV = checkoutHeaderDto.CVV,
                DiscountTotal = checkoutHeaderDto.DiscountTotal,
                Email = checkoutHeaderDto.Email,
                ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
                FirstName = checkoutHeaderDto.FirstName,
                LastName = checkoutHeaderDto.LastName,
                OrderTime = DateTime.UtcNow,
                PaymentStatus = false,
                Phone = checkoutHeaderDto.Phone,
                OrderTotal = checkoutHeaderDto.OrderTotal,
                PickupDateTime = checkoutHeaderDto.PickupDateTime,
                OrderDetails = new()
            };

            foreach (var details in checkoutHeaderDto.CartDetails)
            {
                OrderDetails orderDetails = new()
                {
                    ProductId = details.ProductId,
                    ProductName = details.Product.Name,
                    Price = details.Product.Price,
                    Count = details.Count
                };

                orderHeader.CartTotalItems += details.Count;
                orderHeader.OrderDetails.Add(orderDetails);

            }

            await _orderRepository.AddOrder(orderHeader);

            PaymentRequestMessage paymentRequestMessage = new()
            {
                Name = orderHeader.FirstName + " " + orderHeader.LastName,
                CardNumber = orderHeader.CardNumber,
                CVV = orderHeader.CVV,
                ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                OrderId = orderHeader.OrderHeaderId,
                OrderTotal = orderHeader.OrderTotal,
                Email =orderHeader.Email
            };

            try
            {
                await _messageBus.PublishMessage(paymentRequestMessage, orderPaymentProcessTopic);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private async Task OnPaymentUpdateReceviced(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            UpdatePaymentResultMessage payemntResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            await _orderRepository.UpdateOrderPaymentStatus(payemntResultMessage.OrderId, payemntResultMessage.Status);
            await arg.CompleteMessageAsync(arg.Message);
        }
     }
}
