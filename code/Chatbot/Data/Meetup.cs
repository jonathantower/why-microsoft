namespace MeetupBot.Data
{
	public class Meetup
	{

		public float score { get; set; }
		public string id { get; set; }
		public string name { get; set; }
		public string status { get; set; }
		public string link { get; set; }
		public string urlname { get; set; }
		public string description { get; set; }
		public long created { get; set; }
		public string city { get; set; }
		public string country { get; set; }
		public string localized_country_name { get; set; }
		public string localized_location { get; set; }
		public string state { get; set; }
		public string join_mode { get; set; }
		public string visibility { get; set; }
		public float lat { get; set; }
		public float lon { get; set; }
		public int members { get; set; }
		public Event next_event { get; set; }
	}
}
