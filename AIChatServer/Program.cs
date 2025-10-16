using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AIChatServer.Config.Implementations;
using AIChatServer.Entities.Connection.Implementations;
using AIChatServer.Factories.Implementations;
using AIChatServer.Integrations.Email.Implementations;
using AIChatServer.Managers;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Implementations;
using AIChatServer.Utils.Implementations.Mappers;
using AIChatServer.Managers.Implementations;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["aichat"].ConnectionString;
            var compositeLoggerFactory = new CompositeLoggerFactory(connectionString);

            var appConfigManager = new AppConfigManager(new ConfigFileReader("appsettings.json"));
            var configData = appConfigManager.GetConfigData();

            var jsonHelper = new JsonHelper();
            var random = new Random();
            var mapperFactory = new MapperFactory();
            var tokenManagerFactory = new TokenManagerFactory();
            var repositoryFactory = new RepositoryFactory(compositeLoggerFactory, connectionString);
            var serviceFactory = new ServiceFactory(compositeLoggerFactory);
            var httpService = new HttpService(new HttpClient());
            var managerFactory = new ManagerFactory(jsonHelper, compositeLoggerFactory, httpService);
            var emailSender = new EmailSender(
                configData.EmailConfigData.SmtpServer,
                configData.EmailConfigData.SmtpPort,
                configData.EmailConfigData.SenderEmail,
                configData.EmailConfigData.Password,
                new HtmlContentBuilder(new StringChanger()),
                compositeLoggerFactory.Create<EmailSender>()
            );
            var verificationCodeSender = new VerificationCodeSender(emailSender, new EmailTextGetter());

            var userFactoryFactory = new UserFactoryFactory(jsonHelper, verificationCodeSender, compositeLoggerFactory, random);
            var commandHandlerFactory = new CommandHandlerFactory(jsonHelper);
            var chatStrategyFactory = new ChatStrategyFactory(compositeLoggerFactory);

            var webSocketConnectionFactory = new WebSocketConnectionFactory(jsonHelper, new CommandRequestMapper(),
                compositeLoggerFactory.Create<WebSocketConnection>());

            var mainManagerFactory = new MainManagerFactory(
                mapperFactory,
                tokenManagerFactory,
                repositoryFactory,
                serviceFactory,
                managerFactory,
                userFactoryFactory,
                commandHandlerFactory,
                chatStrategyFactory,
                compositeLoggerFactory
            );

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            services.AddSingleton(mapperFactory);
            services.AddSingleton(tokenManagerFactory);
            services.AddSingleton(repositoryFactory);
            services.AddSingleton(serviceFactory);
            services.AddSingleton(managerFactory);
            services.AddSingleton(userFactoryFactory);
            services.AddSingleton(commandHandlerFactory);
            services.AddSingleton(chatStrategyFactory);

            services.AddSingleton(jsonHelper);
            services.AddSingleton(compositeLoggerFactory);
            services.AddSingleton(httpService);
            services.AddSingleton(random);
            services.AddSingleton(configData);

            var mainDeps = await mainManagerFactory.Create(configData, webSocketConnectionFactory);
            services.AddSingleton(typeof(IChatManager), mainDeps.ChatManager);
            services.AddSingleton(typeof(IUserManager), mainDeps.UserManager);
            services.AddSingleton(typeof(IAIManager), mainDeps.AIManager);
            services.AddSingleton(typeof(INotificationManager), mainDeps.NotificationManager);
            services.AddSingleton(typeof(INotificationService), mainDeps.NotificationService);
            services.AddSingleton(typeof(IChatService), mainDeps.ChatService);
            services.AddSingleton(typeof(IUserService), mainDeps.UserService);

            services.AddSingleton(typeof(ISendCommandMapper), mainDeps.SendCommandMapper);
            services.AddSingleton(typeof(IResponseMapper<AIChatServer.Entities.DTO.Response.ChatResponse, AIChatServer.Entities.Chats.ChatWithUserContext>), mainDeps.ChatResponseMapper);
            services.AddSingleton(typeof(IMapper<AIChatServer.Entities.DTO.Request.MessageRequest, AIChatServer.Entities.Chats.Message, AIChatServer.Entities.DTO.Response.MessageResponse>), mainDeps.MessageMapper);

            services.AddSingleton(mainDeps.CommandHandlers);
            services.AddSingleton(mainDeps.Logger);
            services.AddSingleton(mainDeps.UserEvents);
            services.AddSingleton(mainDeps.ConnectionListener);

            services.AddSingleton(mainDeps.ChatManager);
            services.AddSingleton(mainDeps.UserManager);

            services.AddSingleton<NotificationServiceFacade>();

            services.AddSingleton<ConnectionEventHandler>();
            services.AddSingleton<CommandEventHandler>();
            services.AddSingleton<ChatEventHandler>();
            services.AddSingleton<AIMessageEventHandler>();

            services.AddSingleton<MainManager>(sp =>
            {
                var connectionListener = sp.GetRequiredService<IConnectionListener>();
                var userEvents = sp.GetRequiredService<IUserEvents>();
                var connHandler = sp.GetRequiredService<ConnectionEventHandler>();
                var cmdHandler = sp.GetRequiredService<CommandEventHandler>();
                var chatHandler = sp.GetRequiredService<ChatEventHandler>();
                var aiHandler = sp.GetRequiredService<AIMessageEventHandler>();

                return new MainManager(connectionListener, userEvents, connHandler, cmdHandler, chatHandler, aiHandler);
            });

            var provider = services.BuildServiceProvider(validateScopes: true);

            var logger = provider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("DI container built, resolving MainManager...");

            var mainManager = provider.GetRequiredService<MainManager>();

            logger.LogInformation("MainManager initialized. Server is running. Press Enter to shutdown.");
            Console.ReadLine();
        }
    }
}
