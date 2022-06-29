using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using telegram_bot_for_Valera;
namespace telegrambot_V2
{
     class SlovkoGame
    {
        //static string finish_word = apple;
        public void SlovkoEngine(List<string> words, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
           
            var finish_word = RandomChooseWord(words);
            string checkword = default;
            
            //var tmp = botClient.GetUpdatesAsync();
            //string checkword = update.Message.Text;
            //var t = tmp.Result;
            // checkword = t[t.Length-1].Message.Text ;
            for (int i = 0; i < 6; i++)
            {
                string tmp_word = update.Message.Text;
                var tmp = botClient.GetUpdatesAsync();
                //botClient.U
                //tmp.AsyncState
                
                var t = tmp.Result;
                checkword = t[t.Length - 1].Message.Text;
                if(tmp_word == checkword || checkword == "/play_game")
                {
                    i--;
                    continue;
                }
                if (tmp_word == "exit")
                    return;
                GiveResponseToUser($"Ваша спроба №{i + 1}", update, botClient, cancellationToken);
                if (checkword.Length != finish_word.Length)
                {
                    GiveResponseToUser($"{checkword} не відповідає довжині слова", update, botClient, cancellationToken);
                    i--;
                    //Thread.Sleep(10000);
                    continue;
                }
                if (checkword == finish_word)
                {
                    GiveResponseToUser($"Ура, ви вгадали слово: {checkword} ", update, botClient, cancellationToken);
                    return ;
                }
                
                sloveykofunc(finish_word, checkword, update, botClient, cancellationToken);
            }
            GiveResponseToUser($"Загадане слово було: {checkword} ", update, botClient, cancellationToken);
            return ;
        }

       public string RandomChooseWord(List<string> words)
        {
            Random random = new Random();
            int iterator = random.Next(0, words.Count - 2);
            return words[iterator];
        }

        static public void sloveykofunc(string word, string checkword, Update update, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == checkword[i])
                {
                    GiveResponseToUser($"буква {checkword[i]} на своєму місці", update, botClient, cancellationToken);
                    continue;
                }
                if (word.Contains(checkword[i]))
                {
                    GiveResponseToUser($"буква {checkword[i]} є у слові, але не на своєму місці", update, botClient, cancellationToken);
                    continue;
                }
                GiveResponseToUser($"буква {checkword[i]} не присутня у слові", update, botClient, cancellationToken);
            }
        }

       static public void GiveResponseToUser(string result, Update update, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var chatId = update.Message.Chat.Id.ToString();
            _ = botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: $"{result}",
                       cancellationToken: cancellationToken);
        }
    }
}