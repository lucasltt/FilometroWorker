using FilometroWorker.Database;
using FilometroWorker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilometroWorker.Controllers
{
    public class VacinacaoController : IChatController<PostoVacinacao, Telegram.Bot.Types.Message, ReplyMessage>
    {
        public List<PostoVacinacao> Data { get; set; }
        private Dictionary<long, string> commandTrack = new();
    

        public string HandleMessage(Telegram.Bot.Types.Message msg)
        {

            string inputMessage = msg.Text.ToUpper().Trim();

            string username = msg.Chat.FirstName;
            long chatid = msg.Chat.Id;
            string response = default(string);
            string command = default(string);
            string parameter = default(string);

            if(inputMessage.StartsWith("/"))
            {
                command = inputMessage;
                if (commandTrack.ContainsKey(chatid))
                    commandTrack[chatid] = command;
                else
                    commandTrack.Add(chatid, command);


                response = command switch
                {
                    "/LISTARZONAS" => CommandListarZonas(),
                    "/LISTARDISTRITOS" => CommandListarDistritos(),
                    "/LISTARPOSTOS" => CommandListarPostos(),
                    "/DETALHEPOSTO" => CommandDetalhePosto(),
                    "/INSCREVERPOSTO" => CommandInscreverPosto(),
                    "/DESINSCREVER" => CommandDesinscrever(),
                    "/BUSCARPOSTO" => CommandBuscarPosto(),
                    "/MINHASINSCRICOES" => CommandMinhasInscricoes(username),
                    _ => CommandNotFound()
                };
            }
            else
            {
                //Não é um comando
                parameter = inputMessage;
                if (commandTrack.ContainsKey(chatid))
                    command = commandTrack[chatid];

                if (string.IsNullOrEmpty(command))
                {
                    response = "Por favor inicie com um comando";
                }
                else
                {
                    response = command switch
                    {
                        "/LISTARDISTRITOS" => CommandListarDistritos(parameter),
                        "/LISTARPOSTOS" => CommandListarPostos(parameter),
                        "/DETALHEPOSTO" => CommandDetalhePosto(parameter),
                        "/INSCREVERPOSTO" => CommandInscreverPosto(parameter, username, chatid),
                        "/DESINSCREVER" => CommandDesinscrever(parameter, username),
                        "/BUSCARPOSTO" => CommandBuscarPosto(parameter),
                        _ => CommandNotFound()
                    };
                }

            }

            return response;
               
        }

        public List<ReplyMessage> GetNotifications(List<PostoVacinacao> modifiedData)
        {
            List<ReplyMessage> replyMessages = new();
            StringBuilder stringBuilder;

            foreach (PostoVacinacao posto in modifiedData)
            {
                List<Subscription> subscriptions = DatabaseConnector.GetSubscriptionsByPosto(posto.equipamento);
                foreach (Subscription subscription in subscriptions)
                {
                    stringBuilder = new();
                    stringBuilder.AppendLine($"Olá {subscription.Username}, o posto {subscription.NomePosto} foi atualizado.");
                    stringBuilder.AppendLine(posto.data_hora);
                    stringBuilder.AppendLine(posto.status_fila);
                    if (posto.astrazeneca?.Equals("1") == true)
                        stringBuilder.AppendLine("Astrazeneca Disponível");
                    if (posto.pfizer?.Equals("1") == true)
                        stringBuilder.AppendLine("Pfizer Disponível");
                    if (posto.coronavac?.Equals("1") == true)
                        stringBuilder.AppendLine("Coronavac Disponível");
                    replyMessages.Add(new ReplyMessage(stringBuilder.ToString(), subscription.ChatId));
                }
            }

            return replyMessages;
        }



        private string CommandListarZonas()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] zonas = Data.Select(k => k.crs).Distinct().ToArray();
            int i = 0;
            foreach (string zona in zonas)
            {
                i++;
                stringBuilder.AppendLine(i.ToString() + " - " + zona);
            }
            return zonas.Length > 0 ? stringBuilder.ToString() : "Sem Resultados!";
        }


        private string CommandListarDistritos(string zona)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] distritos = Data.Where(k => k.crs.ToUpper().Equals(zona)).Select(k => k.distrito).Distinct().ToArray();
            int i = 0;
            foreach (string distrito in distritos)
            {
                i++;
                stringBuilder.AppendLine(i.ToString() + " - " + distrito);
            }
                
            return distritos.Length > 0 ? stringBuilder.ToString() : "Sem Resultados!";
        }


        private string CommandListarDistritos() => "Ok! Digite agora um nome de uma zona para exibir os distritos.";

        private string CommandListarPostos() => "Ok! Digite agora um nome de distrito para listar os postos.";

        private string CommandDetalhePosto() => "Ok! Digite o nome do posto completo para exibir os detalhes.";

        private string CommandInscreverPosto() => "Ok! Digite o nome do posto completo para inscrever nas notificações.";

        private string CommandDesinscrever() => "Digite CONFIRMAR para desinscrever de todas as notificações!";

        private string CommandNotFound() => "Seu comando não foi reconhecido.";

        private string CommandBuscarPosto() => "Digite parte do nome do posto para realizar a busca.";

        private string CommandListarPostos(string distrito)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] postos = Data.Where(k => k.distrito.ToUpper().Equals(distrito)).Select(k => k.equipamento).ToArray();
            int i = 0;
            foreach (string posto in postos)
            {
                i++;
                stringBuilder.AppendLine(i.ToString() + " - " + posto);
            }

            return postos.Length > 0 ? stringBuilder.ToString() : "Sem Resultados!";
        }

        private string CommandDetalhePosto(string nomePosto)
        {
            StringBuilder stringBuilder = new StringBuilder();
            PostoVacinacao posto = Data.Where(k => k.equipamento.ToUpper().Equals(nomePosto)).FirstOrDefault();
            if (posto is not null)
            {
                stringBuilder.AppendLine(posto.equipamento);
                stringBuilder.AppendLine(posto.endereco);
                stringBuilder.AppendLine(posto.data_hora);
                stringBuilder.AppendLine(posto.status_fila);
                if (posto.astrazeneca?.Equals("1") == true)
                    stringBuilder.AppendLine("Astrazeneca Disponível");
                if (posto.pfizer?.Equals("1") == true)
                    stringBuilder.AppendLine("Pfizer Disponível");
                if (posto.coronavac?.Equals("1") == true)
                    stringBuilder.AppendLine("Coronavac Disponível");
            }

            return posto is not null ? stringBuilder.ToString() : "Sem Resultados!";
        }


        private string CommandInscreverPosto(string nomePosto, string username, long chatid)
        {
            
            PostoVacinacao posto = Data.Where(k => k.equipamento.ToUpper().Equals(nomePosto)).FirstOrDefault();
            if (posto is not null)
            {
                DatabaseConnector.InsertData(new Subscription()
                {
                    ChatId = chatid,
                    Username = username,
                    NomePosto = nomePosto
                });
            }

            

            return posto is not null ? $"Tudo certo {username}\nAgora você receberá notificações deste posto!" : "Sem Resultados!";
        }

        private string CommandDesinscrever(string parametro, string username)
        {
            if (parametro.Equals("CONFIRMAR"))
            {

                DatabaseConnector.RemoveByUsername(username);
                return $"Tudo certo {username}\nAgora você receberá notificações deste posto!";
            }
            else return "Valor inválido. Digite CONFIRMAR para seguir com a desinscrição.";
        }

        private string CommandMinhasInscricoes(string parametro)
        {
            List<Subscription> subscriptions = DatabaseConnector.GetSubscriptionsByUsername(parametro);
            StringBuilder stringBuilder = new StringBuilder();
            foreach(Subscription subscription in subscriptions)
                stringBuilder.AppendLine(subscription.NomePosto);

            return stringBuilder.ToString();
          

        }

        private string CommandBuscarPosto(string nomePosto)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] postos = Data.Where(k => k.equipamento.ToUpper().Contains(nomePosto)).Select(k => k.equipamento).ToArray();
            int i = 0;
            foreach (string posto in postos)
            {
                i++;
                stringBuilder.AppendLine(i.ToString() + " - " + posto);
            }

            return postos.Length > 0 ? stringBuilder.ToString() : "Sem Resultados!";
        }


    }
}
