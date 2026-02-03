using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using FIAP.CloudGames.Users.Application.Dtos;
using FIAP.CloudGames.Users.Application.Interfaces; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FIAP.CloudGames.Users.Infrastructure.Messaging
{
    public interface IAuthenticationResponsePublisher
    {
        Task PublishAuthenticationResponseAsync(AuthenticationResponseEvent response);
    }

    public class AuthenticationResponsePublisher(
        IAmazonSimpleNotificationService sns,
        IConfiguration configuration,
        ILogger<AuthenticationResponsePublisher> logger) : IAuthenticationResponsePublisher
    {
        private readonly IAmazonSimpleNotificationService _sns = sns;
        private readonly ILogger<AuthenticationResponsePublisher> _logger = logger;
        private readonly string _topicArn = configuration["AWS:SNS:AuthResponsesTopicArn"]
            ?? throw new ArgumentNullException("AWS:SNS:AuthResponsesTopicArn not configured");

        public async Task PublishAuthenticationResponseAsync(AuthenticationResponseEvent response)
        {
            try
            {
                var messageJson = JsonSerializer.Serialize(response);

                var request = new PublishRequest
                {
                    TopicArn = _topicArn,
                    Message = messageJson,
                    MessageAttributes = new Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue>
                    {
                        {
                            "MessageType",
                            new Amazon.SimpleNotificationService.Model.MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = "AuthenticationResponse"
                            }
                        },
                        {
                            "RequestId",
                            new Amazon.SimpleNotificationService.Model.MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = response.RequestId.ToString()
                            }
                        }
                    }
                };

                var publishResponse = await _sns.PublishAsync(request);

                _logger.LogInformation(
                    "[AUTH-RESPONSE] Published authentication response for RequestId={RequestId}, Success={Success}, MessageId={MessageId}",
                    response.RequestId,
                    response.Success,
                    publishResponse.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[AUTH-RESPONSE] Error publishing authentication response for RequestId={RequestId}",
                    response.RequestId);
                throw;
            }
        }
    }

    public class AuthenticationRequestConsumer(
        IAmazonSQS sqs,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<AuthenticationRequestConsumer> logger) : BackgroundService
    {
        private readonly IAmazonSQS _sqs = sqs;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<AuthenticationRequestConsumer> _logger = logger;
        private readonly string _queueUrl = configuration["AWS:SQS:AuthRequestsQueueUrl"]
            ?? throw new ArgumentNullException("AWS:SQS:AuthRequestsQueueUrl not configured");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[AUTH-CONSUMER] Starting authentication request consumer");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20,
                        MessageAttributeNames = ["All"],
                        AttributeNames = ["All"]
                    };

                    var response = await _sqs.ReceiveMessageAsync(request, stoppingToken);

                    foreach (var message in response.Messages)
                    {
                        await ProcessMessageAsync(message, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AUTH-CONSUMER] Error receiving messages from SQS");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("[AUTH-CONSUMER] Stopping authentication request consumer");
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("[AUTH-CONSUMER] Processing message: {MessageId}", message.MessageId);

                // Parse SNS wrapper
                var snsWrapper = JsonSerializer.Deserialize<SnsMessageWrapper>(message.Body);
                if (snsWrapper == null)
                {
                    _logger.LogWarning("[AUTH-CONSUMER] Failed to deserialize SNS wrapper. MessageId={MessageId}", message.MessageId);
                    await DeleteMessageAsync(message);
                    return;
                }

                // Parse authentication request
                var authRequest = JsonSerializer.Deserialize<AuthenticationRequestEvent>(snsWrapper.Message);
                if (authRequest == null)
                {
                    _logger.LogWarning("[AUTH-CONSUMER] Failed to deserialize AuthenticationRequestEvent. MessageId={MessageId}", message.MessageId);
                    await DeleteMessageAsync(message);
                    return;
                }

                // Resolve serviços dentro de um scope
                using var scope = _serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var responsePublisher = scope.ServiceProvider.GetRequiredService<IAuthenticationResponsePublisher>();

                // Autentica usando o AuthService que já existe no projeto
                var authResponse = await authService.AuthenticateAsync(authRequest, cancellationToken);

                // Publica a resposta no SNS
                await responsePublisher.PublishAuthenticationResponseAsync(authResponse);

                // Remove mensagem da fila após processar com sucesso
                await DeleteMessageAsync(message);

                _logger.LogInformation(
                    "[AUTH-CONSUMER] Successfully processed authentication request: RequestId={RequestId}, Success={Success}",
                    authRequest.RequestId,
                    authResponse.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[AUTH-CONSUMER] Error processing message: {MessageId}. Message will return to queue for retry.",
                    message.MessageId);
            }
        }

        private async Task DeleteMessageAsync(Message message)
        {
            try
            {
                await _sqs.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUTH-CONSUMER] Error deleting message: {MessageId}", message.MessageId);
            }
        }
    }

    public class SnsMessageWrapper
    {
        public string Message { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
}