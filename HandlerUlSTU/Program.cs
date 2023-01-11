using HandlerUlSTU;
using HandlerUlSTU.DTO;
using Newtonsoft.Json;

await MainAsync();

async Task MainAsync()
{
    var inter = new InteractionAPI(new EntryPoint()
    {
        Authorization = "https://lk.ulstu.ru/?q=auth/login",
        TimetablePrivate = "https://time.ulstu.ru/api/1.0/timetable",
        AllFilesPrivate = "https://lk.ulstu.ru/timetable/"
    });

    var contents = await inter.GetJsonTimetableWithAuth("m.lebedintcev", "ЬнТуцЗфы616", "ИВТАПбд-31");

    var contents2 = await inter.GetHtmlPageWithAllFilesWithAuth("m.lebedintcev", "ЬнТуцЗфы616");

    //var contents3 = await inter.GetBytesFileWithAuth("m.lebedintcev", "ЬнТуцЗфы616", @"shared/session/Факультет информационных систем и технологий/Расписание зачетов для групп программы Искусственного интеллекта (магистр).xls");

    //File.WriteAllBytes(@"E:\Project\UlSTUBotDy\Main.xls", contents3);

    var b = DataParser.ParserHtmlPageWithAllFiles(contents2);

    var a = DataParser.GenerateTable(DataParser.ParserJsonTimetable(contents), "ИВТАПбд-31");

    File.WriteAllBytes("1.jpg", a);

    //var answer = JsonConvert.DeserializeObject(contents);
}