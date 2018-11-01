using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using MeetupBot.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace MeetupBot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class MeetupWithLuisDialog : LuisDialog<object>
    {
	    private const string MeetupKey = "3d25732b25943171a533f721f3830";

        public MeetupWithLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
	        await context.PostAsync("Sorry, not sure how to help you with that.");
	        context.Wait(MessageReceived);
        }

	    [LuisIntent("ForgetMe")]
	    public async Task ForgetMeIntent(IDialogContext context, LuisResult result)
	    {
		    context.UserData.RemoveValue("zip");

		    await context.PostAsync("Ok, I just forgot where you live.");
		    context.Wait(MessageReceived);
	    }

        [LuisIntent("MyZipCode")]
        public async Task MyZipCodeIntent(IDialogContext context, LuisResult result)
        {
	        var userData = context.UserData;
	        var zip = result.Entities.FirstOrDefault(i => i.Type == "ZipCode")?.Entity ?? "";
	        userData.SetValue("zip", zip);

	        await context.PostAsync($"Thanks! I'll remember that you live at {zip}");
	        context.Wait(MessageReceived);
        }

	    [LuisIntent("MeetupsNearMe")]
	    public async Task MeetupsNearMeIntent(IDialogContext context, LuisResult result)
	    {
		    string zip;
		    var userData = context.UserData;

		    if (result.Entities.Any(i => i.Type == "ZipCode"))
		    {
				zip = result.Entities.FirstOrDefault(i => i.Type == "ZipCode")?.Entity ?? "";
			    userData.SetValue("zip", zip);
		    }

		    zip = userData.ContainsKey("zip") ? userData.GetValue<string>("zip") : null;

		    if (zip == null)
			    await context.PostAsync("Sorry, I don't know where you are. Tell me your zip code.");
		    else
		    {
			    var client = new HttpClient();
			    var respStream = await client.GetStreamAsync($"https://api.meetup.com/recommended/groups?key={MeetupKey}&zip={zip}");
			    var items = (IEnumerable<Meetup>)new DataContractJsonSerializer(typeof(IEnumerable<Meetup>)).ReadObject(respStream);
			    
			    var sb = new StringBuilder($"Here are some upcoming meetups near {zip}:{Environment.NewLine}{Environment.NewLine}");
			    foreach (var item in items.OrderBy(i => i.next_event?.time ?? long.MaxValue).Take(10)) 
			    {
					var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(item.next_event.time / 1000);
					sb.AppendLine($"**{dateTime:ddd MMM dd}** {item.name}{Environment.NewLine}");
			    }

			    await context.PostAsync(sb.ToString());
		    }

		    context.Wait(MessageReceived);
	    }
    }
}