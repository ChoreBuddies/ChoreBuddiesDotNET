var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ChoreBuddies_Backend>("chorebuddies-backend");

builder.Build().Run();
