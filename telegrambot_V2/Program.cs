using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using telegrambot_V2;

namespace telegram_bot_k
{

    class Program
    {

        public static void Main()
        {
            var botClient = new TelegramBotClient("1837727564:AAGSxuvOWeicP26hQgy0e94wxQnwpIxhugQ");
            using var cts = new CancellationTokenSource();
            WordItem msg = new();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            int iterator_for_meaning = 0;
            int iterator_for_synonyms = 0;
            string chatId = default;
            WorkWithApi ApiInAction = new();
            InlineKeyboardMarkup keyboardMarkup = new(new[]
            {
             new[]
            {
              InlineKeyboardButton.WithCallbackData("another definition", $"go to definition"),
              InlineKeyboardButton.WithCallbackData("pronunciation", "go to pronunciation"),
              InlineKeyboardButton.WithCallbackData($"words like this", "go to synonyms"),
              InlineKeyboardButton.WithCallbackData($"examples", "go to examples"),
              InlineKeyboardButton.WithCallbackData($"add to collection", "collection")
            }
            });

            


            botClient.StartReceiving
            (
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions,
                cts.Token
            );

         

            var me = botClient.GetMeAsync();
            Console.WriteLine($"Hello World, I am user {me.Id}");
            Console.ReadLine();
            cts.Cancel();


            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            { 
                if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                {
                    if (update.Message.Text.Contains("/") && update?.Message?.Text != null)
                    {
                     await HandleCommandMessage(botClient, update, cancellationToken);
                        return;
                    }

                    msg =  await HandleMessage(botClient, update, update.Message, keyboardMarkup, cancellationToken,iterator_for_meaning);
                    return;
                }
                if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQuery(botClient, update.CallbackQuery, msg, chatId);
                    return;
                }
                
            }

            static string GetJustWord(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                return update.Message.Text;
            }

            async Task<WordItem> HandleMessage (ITelegramBotClient botClient, Update update,  Message message, InlineKeyboardMarkup keyboardMarkup, CancellationToken cancellationToken, int iterator)
            {
                string messageText;
                try
                {
                    chatId = update.Message.Chat.Id.ToString();
                    messageText = update.Message.Text;
                    string message_for_api = update.Message.Text;
                    var item = GetFromApiWord(message_for_api);
                    try
                    {
                        if (item.meanings.Count < iterator_for_meaning)
                        {
                            iterator_for_meaning = 0;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"{item.meanings[iterator][1]}\n{item.meanings[iterator][0]}\n{item.text_phonetic}",
                    replyMarkup: keyboardMarkup,
                    cancellationToken: cancellationToken);
                    return item;
                }
                catch
                {
                    return null;
                }
            }

            async Task HandleCommandMessage (ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                chatId = update.Message.Chat.Id.ToString();
                WorkWithApi workWithApi = new();
                switch (update.Message.Text)
                {
                    case "/help":
                        {
                            _ = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"test",                                  //допиши інструкцію до бота
                            cancellationToken: cancellationToken);
                        }
                        break;
                    case "/see_collection":
                        {
                            see_collection(botClient, update, cancellationToken);
                        }
                        break;
                    case "/play_game":
                        {
                            //SlovkoGame slovkoGame = new SlovkoGame();
                            //slovkoGame.SlovkoEngine(workWithApi.GetCollection(chatId), botClient, update, cancellationToken);
                        }
                        break;
                    case "/dev_by":
                        {
                            _ = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"test",                                      // вставь свій тг user name 
                            cancellationToken: cancellationToken);
                        }
                        break;
                }
            }
            

            async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, WordItem item, string ChatID)
            {
                try
                {
                    if (callbackQuery.Data.Contains("definition"))
                    {
                        iterator_for_meaning += 1;
                        await botClient.SendTextMessageAsync(
                                callbackQuery.Message.Chat.Id,
                                $"{item.meanings[iterator_for_meaning][1]}\n{item.meanings[iterator_for_meaning][0]}"
                                );

                    }
                    if (callbackQuery.Data.Contains("pronunciation"))
                    {
                        await botClient.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $"{item.audio}"
                            );
                    }
                    if (callbackQuery.Data.Contains("synonyms"))
                    {
                        await botClient.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $"{item.synonyms[iterator_for_synonyms]}"
                            );
                        if (item.synonyms.Count == iterator_for_synonyms)
                            iterator_for_synonyms = 0;
                        iterator_for_synonyms++;
                    }
                    if (callbackQuery.Data.Contains("example"))
                    {
                        await botClient.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $"{GetExample(item.word)}"
                            );
                    }
                    if (callbackQuery.Data.Contains("collection"))
                    {
                        ApiInAction.ToCollection(item.word, ChatID);
                    }
                }
                catch
                {
                    return;
                }
            }

            async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message},{apiRequestException.HttpStatusCode}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
            }



        }

        static WordItem GetFromApiWord(string messageText)
        {
            string[] word_id = messageText.Split(" ", 2);
            string WEBSERVICE_URL = "https://localhost:44326/api/ItemContoller/";
            var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL + word_id[0]);
            webRequest.Method = "GET";
            webRequest.Timeout = 12000;
            try
            {
                using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        var jsonResponse = sr.ReadToEnd();
                        var item = JsonConvert.DeserializeObject<WordItem>(jsonResponse);
                        return item;
                    }
                }
            }
            catch 
            {
                Console.Write("not today");
                return null;
            }
        }

        static void see_collection( ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chatId = update.Message.Chat.Id.ToString();
            WorkWithApi workWithApi = new WorkWithApi();
            var list =  workWithApi.GetCollection(chatId);
            
            foreach (var tmp in list)
            {
                _ = botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"{tmp}",
                        cancellationToken: cancellationToken);
            }
        }

        static string GetExample (string messageText)
        {
            string[] word_id = messageText.Split(" ", 2);
            string WEBSERVICE_URL = "https://localhost:44326/api/ItemContoller/getExample";
            var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL + word_id[0]);
            webRequest.Method = "GET";
            webRequest.Timeout = 12000;
            try
            {
                using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        var item = sr.ReadToEnd();
                                               
                        return item;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("not today");
                return null;
            }


           
        }
    }
}





