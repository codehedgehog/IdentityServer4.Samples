// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.



namespace Client
{

	using IdentityModel.Client;
	using Newtonsoft.Json.Linq;
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;

	public class Program
	{
		public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

		private static async Task MainAsync()
		{
			// discover endpoints from metadata
			var disco = await DiscoveryClient.GetAsync("https://localhost:44337/");
			if (disco.IsError)
			{
				Console.WriteLine(disco.Error);
				return;
			}

			// request token
			var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
			var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

			if (tokenResponse.IsError)
			{
				Console.WriteLine(tokenResponse.Error);
				return;
			}

			Console.WriteLine(tokenResponse.Json);
			Console.WriteLine("\n\nPress any key to continue...\n\n");
			Console.ReadLine();

			// call api
			var client = new HttpClient();
			client.SetBearerToken(tokenResponse.AccessToken);

			var response = await client.GetAsync("https://localhost:44368/identity");
			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine(response.StatusCode);
				Console.WriteLine("\n\nPress any key to continue...\n\n");
				Console.ReadLine();
			}
			else
			{
				var content = await response.Content.ReadAsStringAsync();
				Console.WriteLine(JArray.Parse(content));
				Console.WriteLine("\n\nPress any key to continue...\n\n");
				Console.ReadLine();
			}
		}
	}
}