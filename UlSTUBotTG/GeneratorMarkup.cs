using Telegram.Bot.Types.ReplyMarkups;

namespace UlSTUBotTG
{
    internal static class GeneratorMarkup
    {
        public static ReplyKeyboardMarkup GetMenuStart()
        {
            var rkm = new ReplyKeyboardMarkup(new KeyboardButton[][]
                {
            new KeyboardButton[]
            {
                new KeyboardButton("Моё расписание"),
                new KeyboardButton("Изменить группу")
            }
                });

            rkm.ResizeKeyboard = true;

            return rkm;
        }

        public static ReplyKeyboardMarkup GetMenuTimeTable()
        {
            var rkm = new ReplyKeyboardMarkup(new KeyboardButton[][]
                {
            new KeyboardButton[]
            {
                new KeyboardButton("Сессии")
            },
            new KeyboardButton[]
            {
                new KeyboardButton("Текущая неделя"),
                new KeyboardButton("Следующая неделя")
            },
            new KeyboardButton[]{
                new KeyboardButton("Назад")
            }
                });

            rkm.ResizeKeyboard = true;

            return rkm;
        }

        public static ReplyKeyboardMarkup GetMenuReturn()
        {
            var rkm = new ReplyKeyboardMarkup(
                new KeyboardButton("Назад")
            );

            rkm.ResizeKeyboard = true;

            return rkm;
        }

        public static ReplyKeyboardMarkup GetMenuFaculty()
        {
            var rkm = new ReplyKeyboardMarkup(new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("СФ"),
                    new KeyboardButton("ФИСТ"),
                    new KeyboardButton("ЭФ")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("ИФМИ"),
                    new KeyboardButton("МФ"),
                    new KeyboardButton("РФ"),
                    new KeyboardButton("ИАТУ")
                },
                new KeyboardButton[]{
                    new KeyboardButton("ЗВФ"),
                    new KeyboardButton("ИЭФ"),
                    new KeyboardButton("КЭИ"),
                    new KeyboardButton("ГФ")
                },
                new KeyboardButton[]{
                    new KeyboardButton("Назад")
                }
            });

            rkm.ResizeKeyboard = true;

            return rkm;
        }
    }
}
