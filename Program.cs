using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

const string SchemaFile = "api-schema.json";

var schema = await ReadApiSchema(SchemaFile);

if (schema == null)
{
    Console.WriteLine("Unable to read schema from {0}", SchemaFile);
    return;
}

foreach (var (path, httpMethod, name) in GetAllEndpointsQuery(schema))
{
    app.MapMethods(path, new[] { httpMethod.Key }, (ILogger<Program> logger) =>
    {
        logger.LogInformation("Endpoint Name: {name}", name);
        return httpMethod.Value;
    }).WithName(name);
}

await app.RunAsync();

//Method Definitions

async Task<Dictionary<string, Dictionary<string, JsonNode>>?> ReadApiSchema(string filePath) =>
    await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, JsonNode>>>(File.OpenRead(filePath));

IEnumerable<(string, KeyValuePair<string, JsonNode>, string)> GetAllEndpointsQuery(Dictionary<string, Dictionary<string, JsonNode>> apiSchema) =>
    from path in apiSchema.Keys
    from httpMethod in apiSchema[path]
    let name = $"{httpMethod.Key} {path}"
    select (path, httpMethod, name);
