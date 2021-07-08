using FilometroWorker.Controllers;
using FilometroWorker.Database;
using FilometroWorker.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace FilometroWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TelegramBotClient telegramBot = new TelegramBotClient("YOUTELEGRAMTOKENHERE");
        private readonly VacinacaoController vacinacaoController = new();

        private static List<PostoVacinacao> postosVacinacao = new();
        private static List<PostoVacinacao> postosVacinacaoNew = new();


        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            telegramBot.OnMessage += TelegramBot_OnMessage;
            telegramBot.StartReceiving();
           
        }

        private void TelegramBot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            telegramBot.SendTextMessageAsync(e.Message.Chat.Id, vacinacaoController.HandleMessage(e.Message));
        }

        private async Task<List<PostoVacinacao>> BuscarPostos()
        {
            RestClient restClient = new ("https://deolhonafila.prefeitura.sp.gov.br/");

            IRestRequest restRequest = new RestRequest("/processadores/dados.php", Method.POST);
            restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            restRequest.AddParameter("text/xml", "dados=dados", ParameterType.RequestBody);

            IRestResponse response = await restClient.ExecuteAsync(restRequest);
            return JsonConvert.DeserializeObject<List<PostoVacinacao>>(response.Content);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<PostoVacinacao> postosChanged;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Executando às: {time}", DateTimeOffset.Now);
                postosVacinacaoNew = await BuscarPostos();

                if (postosVacinacaoNew is null)
                    _logger.LogError("Sem informação de postos: {time}", DateTimeOffset.Now);
                else
                {

                    vacinacaoController.Data = postosVacinacaoNew;
                    postosChanged = PostosAtualizados();

                    List<ReplyMessage> replies = vacinacaoController.GetNotifications(postosChanged);
                    foreach (ReplyMessage reply in replies)
                        await telegramBot.SendTextMessageAsync(reply.chatid, reply.body);

                    postosVacinacao = postosVacinacaoNew;
                }

                await Task.Delay(60000, stoppingToken);
            }
        }

        private List<PostoVacinacao> PostosAtualizados()
        {
            List<PostoVacinacao> postosChanged = new();

    
            foreach(PostoVacinacao postoNew in postosVacinacaoNew)
            {
                PostoVacinacao postoOld = postosVacinacao.Where(p => p.equipamento.Equals(postoNew.equipamento)).FirstOrDefault();
                if(postoOld is null)
                {
                    postosChanged.Add(postoNew);
                }
                else
                {
                    if (postoOld?.data_hora?.Equals(postoNew?.data_hora) == false)
                        postosChanged.Add(postoNew);
                }
            }

            return postosChanged;
        }
    }
}
