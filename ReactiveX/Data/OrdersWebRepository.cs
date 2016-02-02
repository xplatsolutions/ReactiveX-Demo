using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Concurrency;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ReactiveX
{
	public class OrdersWebRepository
	{
		protected const string BaseUrlAddress = @"https://api.parse.com/1/classes";

		protected virtual HttpClient GetHttpClient()
		{
			HttpClient httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(BaseUrlAddress);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			return httpClient;
		}

		public async Task<List<OrderViewModel>> GetAsync()
		{
			// Create a cold observable with Defer, 
			// everytime someone subscribes it calls the internal method.
			// IObservable IS AWAITABLE!
			return await Observable.Defer(() => GetAsyncInternal().ToObservable()) 
				.Timeout(TimeSpan.FromSeconds(5))
				.Retry(4) // Retry couple of times, it's fine
				.Catch<List<OrderViewModel>, Exception>((ex) => {
					Debug.WriteLine("Report exception to insights:" + ex); // Catch the exception and do something
					return Observable.Return(new List<OrderViewModel>()); // Return a hot observable
				});
		}

		/// <summary>
		/// Boring http client code.
		/// </summary>
		/// <returns>List of orders.</returns>
		private async Task<List<OrderViewModel>> GetAsyncInternal()
		{
			using (HttpClient client = GetHttpClient ())
			{
				HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress + "/Order");
				requestMessage.Headers.Add("X-Parse-Application-Id", "fwpMhK1Ot1hM9ZA4iVRj49VFzDePwILBPjY7wVFy");
				requestMessage.Headers.Add("X-Parse-REST-API-Key", "egeLQVTC7IsQJGd8GtRj3ttJVRECIZgFgR2uvmsr");
				HttpResponseMessage response = await client.SendAsync(requestMessage);
				response.EnsureSuccessStatusCode ();
				string ordersJson = await response.Content.ReadAsStringAsync();
				JObject jsonObj = JObject.Parse (ordersJson);
				JArray ordersResults = (JArray)jsonObj ["results"];
				//throw new NotSupportedException("boom");
				return JsonConvert.DeserializeObject<List<OrderViewModel>> (ordersResults.ToString ());
			}
		}
	}
}

