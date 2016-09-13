using Microsoft.ProjectOxford.Linguistics;
using Microsoft.ProjectOxford.Linguistics.Contract;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

namespace Bot_Application1
{
    public class Library1
    {
        public string Id;
        public List<string> words = new List<string>();

    }



    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 

        
        

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

        
            if (activity.Type == ActivityTypes.Message)
            {

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var State = activity.GetStateClient();

                var UserData = await State.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                var x = UserData.GetProperty<string>("persName");
                //        List<Library1> liba = new List<Library1>();
                if ((x == null) || (x == "0"))    //если запускаем первый раз
                {

                    var rep = await Reply(activity.Text);
                    Activity reply = activity.CreateReply(rep);
                    UserData.SetProperty<string>("persName", "1");
                    await State.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, UserData);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (x == "1")
                { //когда получил ответ на вопрос "как зовут"
                    UserData.SetProperty<string>("persName", activity.Text);
                    await State.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, UserData);
                    Activity reply = activity.CreateReply("Done, saved your name, ready to work");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                else
                {
                    //рабочая лошадка
                    var rep = await Reply(activity.Text, x);
                    Activity reply = activity.CreateReply(rep);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }




            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        async Task<string> Reply(string msg)
        {

            string omsg = "";
            var a = msg.ToLower().Split(' ');

            if (a.IsPresent("hello") || a.IsPresent("hi")) { omsg = "Hello, what's your name?"; }
            else { omsg = "Cultured people greet first. Hello, what's your name?"; }
            return omsg;

        }
        async Task<string> Reply(string msg, string msg2)
        {
            string omsg = "";

            var a = msg.ToLower().Split(' ');

            if (a.IsPresent("hello") || a.IsPresent("hi")) { omsg = "Hello, " + msg2; }
            else
             if (a.IsPresent("bye") || a.IsPresent("goodbye")) { omsg = "Goodbye, " + msg2; }
            else
            {

                //гном делает словарь слов
                ///////////////////////////////////////////////////////
                List<Library1> liba = new List<Library1>();
                var Client = new LinguisticsClient("96fc9641c5934292be66b62d51fdde5c");
                var Analyzers = await Client.ListAnalyzersAsync();
                // var f = File.OpenText(@"D:\wap.txt");
                using (StreamReader f = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Properties.Resources.wap))))
                {
                    StringBuilder sb = new StringBuilder();

                    int k = 0;
                    while (!f.EndOfStream)
                    {

                        var s = await f.ReadLineAsync();

                        if (s.Trim() == string.Empty)
                        {
                            if (sb.Length > 5)
                            {
                                var Req = new AnalyzeTextRequest();
                                Req.Language = "en";
                                Req.Text = sb.ToString();
                                Req.AnalyzerIds = new Guid[] { Analyzers[1].Id };
                                var Res = await Client.AnalyzeTextAsync(Req);
                                string s1 = Res[0].Result.ToString();
                                Regex ItemRegex = new Regex(@"\((\w+) (\w+)\)", RegexOptions.Compiled);
                                foreach (Match ItemMatch in ItemRegex.Matches(s1))
                                {
                                    k = -1;
                                    for (int i = 0; i < liba.Count(); i++)
                                    {
                                        if (liba[i].Id == ItemMatch.Groups[1].ToString())
                                        {
                                            k = i;
                                        }
                                    }
                                    if (k == -1)
                                    {
                                        Library1 templib = new Library1();
                                        templib.Id = ItemMatch.Groups[1].ToString();
                                        liba.Add(templib);
                                        k = liba.Count() - 1;
                                    }
                                    liba[k].words.Add(ItemMatch.Groups[2].ToString());
                                }
                                await Task.Delay(1000);
                            }
                            sb.Clear();
                        }
                        else
                        {
                            sb.AppendLine(s);
                        }
                    }
                }
                /*

                    //    using (StreamReader f = new StreamReader(Properties.Resources.wap))
                    //    string foo = global::Bot_Application1.Properties.Resources.wap;
                    StreamReader f = new StreamReader(global::Bot_Application1.Properties.Resources.wap);
                */




                /////////////////////////////////////////////////
                //гном сделал словарь
                var Req1 = new AnalyzeTextRequest();
                Req1.Language = "en";
                Req1.Text = msg;
                Req1.AnalyzerIds = new Guid[] { Analyzers[1].Id };
                var Res1 = await Client.AnalyzeTextAsync(Req1);

                var text = Res1[0].Result.ToString();

                var regex = new Regex(@"\((\w+) (\w+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = regex.Matches(text);

                foreach (Match ItemMatch in matches)
                {
                    for (int i = 0; i < liba.Count; i++)
                    {
                        if (ItemMatch.Groups[1].ToString() == liba[i].Id)
                        {
                            var random = new Random();
                            int index = random.Next(liba[i].words.Count);
                            omsg += $"{liba[i].words[index]} ";
                        }
                    }


                }


            }
            omsg=omsg.ToLower();
            return omsg;

        }
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}