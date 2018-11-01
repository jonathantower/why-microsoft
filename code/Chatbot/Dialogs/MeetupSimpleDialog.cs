using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeetupBot.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace MeetupBot.Dialogs
{
	[Serializable]
	public class MeetupSimpleDialog : IDialog<object>
	{
		private const string MeetupKey = "3d25732b25943171a533f721f3830";

		public async Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);
		}

		public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
		{
			var message = await argument;

			if (Regex.IsMatch(message.Text.ToLower(), "meetup|user group|users group|event"))
			{
				string zip;
				var userData = context.UserData;

				if (Regex.IsMatch(message.Text, "[0-9]{5}"))
				{
					zip = Regex.Match(message.Text, "[0-9]{5}").Value;
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
				context.Wait(MessageReceivedAsync);
			} 
			else if (Regex.IsMatch(message.Text, "[0-9]{5}"))
			{
				var userData = context.UserData;
				var zip = Regex.Match(message.Text, "[0-9]{5}").Value;
				userData.SetValue("zip", zip);

				await context.PostAsync($"Thanks! I'll remember that you live at {zip}");
				context.Wait(MessageReceivedAsync);
			}
			else if (Regex.IsMatch(message.Text.ToLower(), "clear|forget"))
			{
				context.UserData.RemoveValue("zip");
				await context.PostAsync("Ok, I just forgot where you live.");
				context.Wait(MessageReceivedAsync);
			}
			else if (Regex.IsMatch(message.Text.ToLower(), "pay"))
			{
				await DoRichCard(context);
			}
			else
			{
				await context.PostAsync("Sorry, not sure how to help you with that.");
				context.Wait(MessageReceivedAsync);
			}

		}

		public async Task DoRichCard(IDialogContext context)
		{
			var message = context.MakeMessage();

			var attachment = GetReceiptCard();
			message.Attachments.Add(attachment);

			await context.PostAsync(message);

			context.Wait(this.MessageReceivedAsync);
		}

		private static Attachment GetReceiptCard()
		{
			var receiptCard = new ReceiptCard
			{
				Title = "John Doe",
				Facts = new List<Fact> { new Fact("Order Number", "1234"), new Fact("Payment Method", "VISA 5555-****") },
				Items = new List<ReceiptItem>
				{
					new ReceiptItem("Data Transfer", price: "$ 38.45", quantity: "368", image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
					new ReceiptItem("App Service", price: "$ 45.00", quantity: "720", image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
				},
				Tax = "$ 7.50",
				Total = "$ 90.95",
				Buttons = new List<CardAction>
				{
					new CardAction(
						ActionTypes.OpenUrl,
						"More information",
						"https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
						"https://azure.microsoft.com/en-us/pricing/")
				}
			};

			return receiptCard.ToAttachment();
		}
	}
}