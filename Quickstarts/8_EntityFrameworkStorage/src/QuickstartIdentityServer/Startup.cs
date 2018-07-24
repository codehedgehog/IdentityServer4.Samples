﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace QuickstartIdentityServer
{
	using IdentityServer4;
	using IdentityServer4.EntityFramework.DbContexts;
	using IdentityServer4.EntityFramework.Mappers;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore.Internal;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.IdentityModel.Tokens;
	using System.Reflection;
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			const string connectionString = @"Data Source=ITDEVSQL;database=WIRAWANTEST;trusted_connection=yes;";
			var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

			// configure identity server with in-memory stores, keys, clients and scopes
			services.AddIdentityServer()
					.AddDeveloperSigningCredential()
					.AddTestUsers(Config.GetUsers())
					// this adds the config data from DB (clients, resources)
					.AddConfigurationStore(options =>
					{
						options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
						options.DefaultSchema = "idsrv";
					})
					// this adds the operational data from DB (codes, tokens, consents)
					.AddOperationalStore(options =>
					{
						options.ConfigureDbContext = builder =>
											builder.UseSqlServer(connectionString,
													sql => sql.MigrationsAssembly(migrationsAssembly));

									// this enables automatic token cleanup. this is optional.
									options.EnableTokenCleanup = true;
						options.TokenCleanupInterval = 30;
						options.DefaultSchema = "idsrv";
					});

			services.AddAuthentication()
					.AddGoogle("Google", options =>
					{
						options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

						options.ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com";
						options.ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo";
					})
					.AddOpenIdConnect("oidc", "OpenID Connect", options =>
					{
						options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
						options.SignOutScheme = IdentityServerConstants.SignoutScheme;

						options.Authority = "https://demo.identityserver.io/";
						options.ClientId = "implicit";

						options.TokenValidationParameters = new TokenValidationParameters
						{
							NameClaimType = "name",
							RoleClaimType = "role"
						};
					});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			InitializeDatabase(app);
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();

			}

			app.UseIdentityServer();

			app.UseStaticFiles();
			app.UseMvcWithDefaultRoute();
		}

		private void InitializeDatabase(IApplicationBuilder app)
		{
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

				var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				context.Database.Migrate();
				if (!context.Clients.Any())
				{
					foreach (var client in Config.GetClients())
					{
						context.Clients.Add(client.ToEntity());
					}
					context.SaveChanges();
				}

				if (!context.IdentityResources.Any())
				{
					foreach (var resource in Config.GetIdentityResources())
					{
						context.IdentityResources.Add(resource.ToEntity());
					}
					context.SaveChanges();
				}

				if (!context.ApiResources.Any())
				{
					foreach (var resource in Config.GetApiResources())
					{
						context.ApiResources.Add(resource.ToEntity());
					}
					context.SaveChanges();
				}
			}
		}

	}
}