using System;
using ReactiveUI;
using Newtonsoft.Json;

namespace ReactiveX
{
	public class OrderViewModel : ReactiveObject
	{
		[JsonProperty("orderNumber")]
		public string OrderNumber { get; set; }
	}
}

