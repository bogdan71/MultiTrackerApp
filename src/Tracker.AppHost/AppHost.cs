var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Tracker_Api>("tracker-api");

builder.AddNpmApp("tracker-web", "../tracker-web", "dev")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
