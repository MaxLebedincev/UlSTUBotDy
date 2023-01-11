using GemBox.Spreadsheet;
using HandlerUlSTU.DTO.Timetable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HandlerUlSTU
{
    public static class DataParser
    {
        /// <summary>
        /// Неконтралируемая функция парсинга, возможны непредсказуемые ошибки!
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Timetable ParserJsonTimetable(string? json)
        {
            Timetable uncertainData;

            try
            {
                uncertainData = JsonConvert.DeserializeObject<Timetable>(json);

                if (uncertainData.response.weeks == null)
                {
                    return new Timetable();
                }

                Regex re = new Regex(@"(?<="")[0-9]{2}(?=""\:\{)");

                var weeks = re.Matches(json ?? throw new Exception(""));
                json = re.Replace(json ?? throw new Exception(""), "week");
                json = json.Replace("[]", @"[{""group"":""""}]");
                json = new Regex(@"(\[(?={""group""))|(\](?=,|\]))").Replace(json, "");

                #pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
                uncertainData = JsonConvert.DeserializeObject<Timetable>(json);
                #pragma warning restore CS8600

                List<int> numberWeek = new List<int>();

                foreach (var week in weeks)
                {
                    #pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                    numberWeek.Add(Int32.Parse(week.ToString()));
                    #pragma warning restore CS8604 
                }

                #pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
                uncertainData.response.weeks.numberWeek = numberWeek;
                #pragma warning restore CS8602 

            }
            catch (Exception) { throw new Exception("ОШИБКА ПАРСЕРА!"); }

            return uncertainData;
        }

        public static List<string> ParserHtmlPageWithAllFiles(string data)
        {
            Regex filterUrl = new Regex(@"(?<=href="").+xls(?="")");

            return filterUrl.Matches(data).Cast<Match>().Select(m => m.Value).ToList();
        }

        public static byte[] GenerateTable(Timetable timetable, string groupName)
        {
            if (timetable == null || timetable.response == null || timetable.response.weeks == null || timetable.response.weeks.week == null || timetable.response.weeks.week.days == null || timetable.response.weeks.numberWeek == null)
                throw new Exception("Некорректные данные");

            var weeks = timetable.response.weeks;

            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            ExcelFile workbook = new ExcelFile();
            ExcelWorksheet ws = workbook.Worksheets.Add("Timetable");

            #region Настройка глобальных стилей

            CellStyle MainStyle = new CellStyle();
            MainStyle.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            MainStyle.VerticalAlignment = VerticalAlignmentStyle.Center;
            MainStyle.WrapText = true;
            CellRange rangeAll = ws.Cells.GetSubrangeAbsolute(0, 0, 10, 8);
            rangeAll.Style = MainStyle;

            #endregion Настройка глобальных стилей

            #region Создание шапки

            ws.Cells[0, 0].Value = $"Расписание занятий группы: {groupName}   Неделя: {weeks.numberWeek.First()}";
            //ws.Cells[0, 0].Style.Borders.SetBorders(MultipleBorders.All, SpreadsheetColor.FromName(ColorName.Black), LineStyle.Thin);
            ws.Cells[0, 0].Style.Font.Size = 20 * 16;
            ws.Cells[0, 0].Style.Font.Name = "Arial";
            CellRange rangeTitle = ws.Cells.GetSubrangeAbsolute(0, 0, 1, 8);
            rangeTitle.Merged = true;

            #endregion Создание шапки

            #region Создание таблицы

            ws.Columns[0].Width = 256 * 5;

            for (int i = 1; i < 9; i++)
            {
                ws.Columns[i].Width = 256 * 10;
            }

            for (int i = 4; i < 10; i++)
            {
                ws.Rows[i].Height = 256 * 3;
            }

            for (int horizontalIndex = 2; horizontalIndex < 10; horizontalIndex++)
            {
                for (int verticalIndex = 0; verticalIndex < 9; verticalIndex++)
                {
                    ws.Cells[horizontalIndex, verticalIndex].Style.Borders.SetBorders(MultipleBorders.All, SpreadsheetColor.FromName(ColorName.Black), LineStyle.Thin);
                    ws.Cells[horizontalIndex, verticalIndex].Style.Font.Size = 20 * 5;
                    ws.Cells[horizontalIndex, verticalIndex].Style.Font.Name = "Arial";
                    if (verticalIndex == 0)
                    {
                        ws.Cells[horizontalIndex, verticalIndex].Style.Font.Size = 20 * 7;
                        ws.Cells[horizontalIndex, verticalIndex].Value = FirstColumnTimeTableName[horizontalIndex-2];
                    }
                    else if (horizontalIndex == 2)
                    {
                        ws.Cells[horizontalIndex, verticalIndex].Style.Font.Size = 20 * 7;
                        ws.Cells[horizontalIndex, verticalIndex].Value = $"{verticalIndex}-я";
                    }
                    else if (horizontalIndex == 3)
                    {
                        ws.Cells[horizontalIndex, verticalIndex].Style.Font.Size = 20 * 7;
                        ws.Cells[horizontalIndex, verticalIndex].Value = SecondLineTimeTableDateTime[verticalIndex-1];
                    }
                    else
                    {
                        try
                        {
                            #pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
                            if (weeks.week.days[verticalIndex-1].lessons[horizontalIndex-4].group == "")
                            {
                                ws.Cells[horizontalIndex, verticalIndex].Value = "";
                            }
                            else
                            {
                                var lesson = weeks.week.days[verticalIndex-1].lessons[horizontalIndex-4];

                                ws.Cells[horizontalIndex, verticalIndex].Value = $"{lesson.nameOfLesson}, {lesson.teacher}, аудитория {lesson.room}";
                            }
                            #pragma warning restore CS8602
                        }
                        catch (Exception) { throw new Exception("Ошибка формирования EXCEL"); }
                    }
                }
            }

            #endregion Создание таблицы

            #region Создание border

            ws.Cells[10, 0].Value = $"VK: | TG: @UlSTUMainBot | Dis:";
            ws.Cells[10, 0].Style.Font.Size = 20 * 7;
            ws.Cells[10, 0].Style.Font.Name = "Arial";
            CellRange rangeBorder = ws.Cells.GetSubrangeAbsolute(10,0,10,8);
            rangeBorder.Merged = true;

            #endregion Создание border

            var imageOptions = new ImageSaveOptions(ImageSaveFormat.Jpeg)
            {
                PageNumber = 0, 
                CropToContent = true
            };

            MemoryStream ms = new MemoryStream();

            workbook.Save(ms, imageOptions);

            return ms.ToArray();
        }

        private static readonly string[] FirstColumnTimeTableName = {"Пара", "Время", "Пнд", "Втр", "Срд", "Чтв", "Птн", "Сбт"};

        private static readonly string[] SecondLineTimeTableDateTime = { "08:30-09:50", "10:00-11:20", "11:30-12:50", "13:30-14:50", "15:00-16:20", "16:30-17:50", "18:00-19:20", "19:30-20:50" };

        public static readonly Dictionary<string, string> AbbreviationFaculty = new Dictionary<string, string>() {
            {"ГФ","Гуманитарный факультет"},
            {"ЗВФ","Заочно-вечерний факультет"},
            {"ИЭФ","Инженерно-экономический факультет"},
            {"ИФМИ","Инженерный факультет международного института"},
            {"МФ","Машиностроительный факультет"},
            {"РФ","Радиотехнический факультет"},
            {"ИАТУ","Самолетостроительный факультет"},
            {"СФ","Строительный факультет"},
            {"ФИСТ","Факультет информационных систем и технологий"},
            {"КЭИ","Факультет среднего профессионального образования"},
            {"ЭФ","Энергетический факультет"}
        };

        public static bool isFaculty(string mes)
        {
            return AbbreviationFaculty.ContainsKey(mes.ToUpper());
        }

        public static bool isFileFromFaculty(string file, string faculty)
        {
            var fullFaculty = AbbreviationFaculty[faculty.ToUpper()];

            if (file.Contains(fullFaculty))
            {
                return true;
            }

            return false;
        }
    }
}
