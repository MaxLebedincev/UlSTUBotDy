using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types.ReplyMarkups;
using UlSTUBotTG;
using HandlerUlSTU;
using HandlerUlSTU.DTO;
using Telegram.Bot.Types.InputFiles;
using HandlerUlSTU.DTO.Timetable;
using Newtonsoft.Json;
using UlSTUBotTG.DTO;

ITelegramBotClient bot;
ConfigurationBot config;
InteractionAPI interactionAPI;

await Main();

async Task Main()
{
    using var services = ConfigureUlSTU();

    config = services.GetRequiredService<ConfigurationBot>();

    if (config.CredentialUlSTU == null)
    {
        throw new Exception("В конфигурации не предсавлены доступы к сайту!");
    }
    else if (config.EntryPointUlSTU == null)
    {
        throw new Exception("В конфигурации не предсавлены точки входа к сайту!");
    }

    bot = new TelegramBotClient(config.Token);
    interactionAPI = new InteractionAPI(
        new Credential()
        {
            Login = config.CredentialUlSTU.Login,
            Password = config.CredentialUlSTU.Password
        }, 
        new EntryPoint()
        {
            Authorization = config.EntryPointUlSTU.Authorization,
            TimetablePrivate = config.EntryPointUlSTU.TimetablePrivate,
            AllFilesPrivate = config.EntryPointUlSTU.AllFilesPrivate,
        }
    );


    Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

    var cts = new CancellationTokenSource();
    var cancellationToken = cts.Token;
    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = { }, // receive all update types
    };
    bot.StartReceiving(
        HandleUpdateAsync,
        HandleErrorAsync,
        receiverOptions,
        cancellationToken
    );
    Console.ReadLine();
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Некоторые действия
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
    
    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
    {

        if (update.Message == null || update.Message.Text == null)
            return;

        var message = update.Message;

        switch (message.Text.ToLower())
        {
            case "/start":
            case "назад":
                await botClient.SendTextMessageAsync(message.Chat, "1", replyMarkup: GeneratorMarkup.GetMenuStart());
                return;
            case "моё расписание":
                await botClient.SendTextMessageAsync(message.Chat, "2", replyMarkup: GeneratorMarkup.GetMenuTimeTable());
                return;
            case "изменить группу":
                await botClient.SendTextMessageAsync(message.Chat, "3", replyMarkup: GeneratorMarkup.GetMenuReturn());
                return;
            case "сессии":
                await botClient.SendTextMessageAsync(message.Chat, "4", replyMarkup: GeneratorMarkup.GetMenuFaculty());
                return;
            case "текущая неделя":

                PersonInfo person = GetCurrentPerson(message.Chat.Id);

                if(person == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Сначала укажите группу", replyMarkup: GeneratorMarkup.GetMenuReturn());
                }
                else
                {
                    var resourceJson = await interactionAPI.GetJsonTimetableWithAuth(person.NameGroup);
                    Timetable? resource = DataParser.ParserJsonTimetable(resourceJson);
                    Stream stream = new MemoryStream(DataParser.GenerateTable(resource, person.NameGroup));
                    InputOnlineFile iof = new InputOnlineFile(stream);
                    iof.FileName = "Расписание";
                    await botClient.SendPhotoAsync(message.Chat, iof);
                }
                return;
            case "следующая неделя":
                await botClient.SendTextMessageAsync(message.Chat, "Функция ещё не доделана!", replyMarkup: GeneratorMarkup.GetMenuTimeTable());
                return;
            default:

                if (DataParser.isFaculty(message.Text))
                {
                    try
                    {
                        string htmlPage = await interactionAPI.GetHtmlPageWithAllFilesWithAuth();

                        List<string> allFiles = DataParser.ParserHtmlPageWithAllFiles(htmlPage);

                        foreach (string file in allFiles)
                        {
                            if (DataParser.isFileFromFaculty(file, message.Text))
                            {
                                Stream stream = new MemoryStream(await interactionAPI.GetBytesFileWithAuth(file));

                                InputOnlineFile iof = new InputOnlineFile(stream);
                                var section = file.Split("/");
                                iof.FileName = section[section.Length - 1];

                                await botClient.SendDocumentAsync(message.Chat, iof, "Сообщение");
                            }
                        }
                    }
                    catch (Exception) { return; }
                }
                else
                {
                    string resourceJson = await interactionAPI.GetJsonTimetableWithAuth(message.Text);

                    Timetable? resource = DataParser.ParserJsonTimetable(resourceJson);

                    if (resource.response.weeks.week == null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Нет данных о группе, проверьте корректность наименования.");
                        return;
                    }
                    else
                    {
                        if (!System.IO.File.Exists(@"DataUsers.json"))
                            throw new Exception("Нет файла с хранилищем данных пользоватлей");

                        PersonInfo currentPerson = new PersonInfo();
                        bool flagUser = false;

                        List<PersonInfo> persons = JsonConvert.DeserializeObject<List<PersonInfo>>(System.IO.File.ReadAllText(@"DataUsers.json"));

                        if (persons == null)
                            persons = new List<PersonInfo>();

                        for(int i = 0; i < persons.Count; i++)
                        {
                            if (persons[i].Id == message.Chat.Id)
                            {
                                flagUser = true;

                                if (persons[i].NameGroup != message.Text)
                                {
                                    persons[i].NameGroup = message.Text;
                                    await botClient.SendTextMessageAsync(message.Chat, "Группа обновлена!");
                                }

                                currentPerson = persons[i];

                                break;
                            }
                        }

                        if (!flagUser)
                        {
                            currentPerson.Id = message.Chat.Id;
                            currentPerson.Chat = message.Chat;
                            currentPerson.NameGroup = message.Text;
                            persons.Add(currentPerson);
                        }

                        System.IO.File.WriteAllText(@"DataUsers.json", JsonConvert.SerializeObject(persons));


                        Stream stream = new MemoryStream(DataParser.GenerateTable(resource, currentPerson.NameGroup));
                        InputOnlineFile iof = new InputOnlineFile(stream);
                        iof.FileName = "Расписание";
                        await botClient.SendPhotoAsync(message.Chat, iof);
                        await botClient.SendTextMessageAsync(message.Chat, "2", replyMarkup: GeneratorMarkup.GetMenuTimeTable());
                    }

                }

                return;
        }
    }
}

PersonInfo GetCurrentPerson(long id)
{
    if (!System.IO.File.Exists(@"DataUsers.json"))
        throw new Exception("Нет файла с хранилищем данных пользоватлей");

    PersonInfo cPerson = new PersonInfo();
    bool flag = false;

    List<PersonInfo> persons = JsonConvert.DeserializeObject<List<PersonInfo>>(System.IO.File.ReadAllText(@"DataUsers.json"));

    foreach (var person in persons)
    {
        if (person.Id == id)
        {
           return person;
        }
    }

    return null;
}

async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    // Некоторые действия
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
}

ServiceProvider ConfigureUlSTU()
{
    return new ServiceCollection()
        .AddSingleton(new ConfigurationBuilder()
            .SetBasePath(@"E:\Project\UlSTUBotDy\UlSTUBotTG")
            .AddJsonFile("appsettings.json").Build()
            .GetSection(nameof(ConfigurationBot))
            .Get<ConfigurationBot>()
        )
        .BuildServiceProvider();
}