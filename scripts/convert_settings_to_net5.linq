<Query Kind="Program">
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var root = XElement.Load(@"D:\\GitProjects\MoTA\Cyanometer\src\Cyanometer\Cyanometer.Manager\bin\Debug\applicationSettings.config");
	var items = root.Elements("setting");
	var settings = new Settings();
	foreach (var item in items)
	{
		string name = (string)item.Attribute("name");
		string value = item.Element("value").Value;
		var properties = typeof(Settings).GetProperties();
		var property = typeof(Settings).GetProperty(name);
		if (property is not null)
		{
			var propertyType = property.PropertyType;
			if (propertyType == typeof(String))
			{
				property.SetValue(settings, value);
			}
			else if (propertyType == typeof(int))
			{
				property.SetValue(settings, int.Parse(value));
			}
			else if (propertyType == typeof(bool))
			{
				property.SetValue(settings, bool.Parse(value));
			}
			else
			{
				$"Unsupported type {propertyType.Name}".Dump();
			}
		}
		else
		{
			$"Unsupported property {name}".Dump();
		}
	}
	using (var stream = File.OpenWrite(@"D:\\GitProjects\MoTA\Cyanometer\src\Cyanometer\Cyanometer.Manager\bin\Debug\settings.json"))
	{
		await JsonSerializer.SerializeAsync(stream, settings);
	}
}

public record Settings
{
	public string Country { get; set; }
	public string City { get; set; }
	public string Location { get; set; }
	public int LocationId { get; set; }
	public bool ExceptionlessEnabled { get; set; }
	public string ExceptionlessServer { get; set; }
	public string ExceptionlessApiKey { get; set; }
	public bool ProcessImages { get; set; }
	public bool ProcessAirQuality { get; set; }
	public int SleepMinutes { get; set; }
	public int CycleWaitMinutes { get; set; }
	public int InitialDelay { get; set; }
	public string StopCheckUrl { get; set; }
	public string CyanoNotificationsUrl { get; set; }
	public bool SyncWithNntp { get; set; }
	/// <summary>
	/// When true a heartbeat signal is sent to <see cref="HeartbeatAddress"/>.
	/// </summary>
	public bool SendHeartbeat { get; set; }
	/// <summary>
	/// Server address for the heartbeat signal.
	/// </summary>
	public string HeartbeatAddress { get; set; }
	/// <summary>
	/// This is a token used to communicate with Cyanometer web
	/// </summary>
	public string CyanoToken { get; set; }
	/// <summary>
	/// Adds additional arguments to raspistill command line photo utility
	/// </summary>
	public string RaspiStillAdditionalArguments { get; set; }
	public Settings() {}
	public Settings(string country, string city, string location, int locationId, bool processImages, int cycleWaitMinutes, int initialDelay, string cyanoNotificationsUrl, bool syncWithNntp, bool sendHeartbeat, string heartbeatAddress, string cyanoToken, string raspiStillAdditionalArguments)
	{
		Country = country;
		City = city;
		Location = location;
		LocationId = locationId;
		ProcessImages = processImages;
		CycleWaitMinutes = cycleWaitMinutes;
		InitialDelay = initialDelay;
		CyanoNotificationsUrl = cyanoNotificationsUrl;
		SyncWithNntp = syncWithNntp;
		SendHeartbeat = sendHeartbeat;
		HeartbeatAddress = heartbeatAddress;
		CyanoToken = cyanoToken;
		RaspiStillAdditionalArguments = raspiStillAdditionalArguments;
	}
}
