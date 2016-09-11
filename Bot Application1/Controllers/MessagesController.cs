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


namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {


     
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var State = activity.GetStateClient();

                var UserData = await State.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                var x = UserData.GetProperty<string>("persName");
                if (x==null)
                { var rep = await Reply(activity.Text);
                    Activity reply = activity.CreateReply(rep);
                    UserData.SetProperty<string>("persName", "1");
                    await State.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, UserData);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (x == "1") {
                    UserData.SetProperty<string>("persName", activity.Text);
                    await State.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, UserData);
                    Activity reply = activity.CreateReply("Done, saved your name");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                    else { var rep = await Reply(activity.Text,x);
                      Activity reply = activity.CreateReply(rep);
                      await connector.Conversations.ReplyToActivityAsync(reply);
                }
               
                // return our reply to the user
               
                
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

            string omsg="";
            var a = msg.ToLower().Split(' ');
         
            if (a.IsPresent("hello")|| a.IsPresent("hi")) {omsg = "Hello, what's your name?";}
            else { omsg = "Cultured people greet first. Hello, what's your name?"; }
            return omsg;
            
        }
        async Task<string> Reply(string msg, string msg2)
        {
            string omsg = "";
           
                var a = msg.ToLower().Split(' ');

                if (a.IsPresent("hello") || a.IsPresent("hi")) { omsg = "Hello, " + msg2; }
                else
            {
                var Client = new LinguisticsClient("96fc9641c5934292be66b62d51fdde5c");
                var Analyzers = await Client.ListAnalyzersAsync();
                var Req = new AnalyzeTextRequest();
                Req.Language = "en";
                Req.Text = msg;
                Req.AnalyzerIds = new Guid[] { Analyzers[0].Id };
                var Res = await Client.AnalyzeTextAsync(Req);
                omsg =msg2+", I found this"+ $"{Res[0].Result.ToString()}";
            }
             
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