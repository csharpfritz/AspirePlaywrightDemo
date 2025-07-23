var builder = DistributedApplication.CreateBuilder(args);

var nws = builder.AddExternalService("nws", "https://api.weather.gov");

var cache = builder.AddRedis("cache")
									.WithLifetime(ContainerLifetime.Persistent)
									.WithRedisInsight();

var aiModel = builder.AddGitHubModel("ai-model", "openai/gpt-4o-mini");

var api = builder.AddProject<Projects.Api>("api")//;
									.WithReference(cache)
									.WithReference(nws);

var postgres = builder.AddPostgres("postgres")
								.WithLifetime(ContainerLifetime.Persistent)
								.WithDataVolume(isReadOnly: false);

var weatherDb = postgres.AddDatabase("weatherdb");

var web = builder.AddProject<Projects.MyWeatherHub>("myweatherhub")
								 .WithReference(api)
								 .WithReference(weatherDb)
								 .WithReference(aiModel)
								 .WaitFor(postgres)
								 .WithExternalHttpEndpoints();

builder.Build().Run();
