namespace MeetupBot.Data
{
	public class Event
	{
		public string id { get; set; }
		public string name { get; set; }
		public int no_rsvp_count { get; set; }
		public long time { get; set; }
	}
}