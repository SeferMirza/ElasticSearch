using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace ElasticSearch.Api;

public class Client
{
    readonly ElasticsearchClient _client;

    public Client(IConfiguration configuration)
    {
        Uri connectionString = new(configuration.GetConnectionString("ElasticSearch") ?? throw new ArgumentNullException());
        ElasticsearchClientSettings settings = new(connectionString);
        settings.DefaultIndex("api");

        _client = new(settings);
        _client.IndexAsync("api").GetAwaiter().GetResult();
    }

    public async Task Ping(CancellationToken cancellationToken)
    {
        var pingResponse = await _client.PingAsync(cancellationToken);
        if (!pingResponse.IsValidResponse)
        {
            throw new Exception("Elasticsearch connection failed");
        }
    }

    public async Task Add(Documentation documentation, CancellationToken cancellationToken)
    {
        CreateRequest<Documentation> createRequest = new(documentation.Id)
        {
            Document = documentation,
        };

        await _client.CreateAsync(createRequest, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Documentation>> All(CancellationToken cancellationToken)
    {
        var result = await _client.SearchAsync<Documentation>(s => s.Index("api"), cancellationToken: cancellationToken);

        return result.Documents;
    }

    public async Task<bool> Update(Documentation documentation, CancellationToken cancellationToken)
    {
        var response = await _client.UpdateAsync<Documentation, Documentation>(
            "my-tweet-index",
            documentation.Id,
            u => u
                .Doc(documentation)
        );

        if (!response.IsValidResponse)
        {
            throw new Exception("Something goes wrong!");
        }

        return response.IsSuccess();
    }

    public async Task<bool> Delete(int index, CancellationToken cancellationToken)
    {
        var response = await _client.DeleteAsync("api", index, cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new Exception("Something goes wrong!");
        }

        return response.IsSuccess();
    }

    public async Task<IReadOnlyCollection<Documentation>> SearchWithFuzzy(string text, string field, CancellationToken cancellationToken, int? from = 0, int? size = 10)
    {
        Action<QueryDescriptor<Documentation>> query = q => q.Fuzzy(f => f.Field(new Field(field)).Value(text));
        var response = await Search(query, cancellationToken, from, size);

        return response;
    }

    public async Task<IReadOnlyCollection<Documentation>> SearchWithMatch(string text, string field, CancellationToken cancellationToken, int? from = 0, int? size = 10)
    {
        Action<QueryDescriptor<Documentation>> query = q => q.Match(f => f.Field(new Field(field)).Suffix(text));
        var response = await Search(query, cancellationToken, from, size);

        return response;
    }

    public async Task<IReadOnlyCollection<Documentation>> SearchWithWildcard(string text, string field, CancellationToken cancellationToken, int? from = 0, int? size = 10)
    {
        Action<QueryDescriptor<Documentation>> query = q => q.Wildcard(w => w.Field(new Field(field)).Value(text));
        var response = await Search(query, cancellationToken, from, size);

        return response;
    }

    async Task<IReadOnlyCollection<Documentation>> Search(Action<QueryDescriptor<Documentation>> query, CancellationToken cancellationToken, int? from = 0, int? size = 10)
    {
        var response = await _client.SearchAsync<Documentation>(s => s
            .Index("api")
            .From(from)
            .Size(size)
            .Query(query)
        );

        if (!response.IsValidResponse)
        {
            throw new Exception("Something goes wrong!");
        }

        return response.Documents;
    }

    public async Task Reset(CancellationToken cancellationToken)
    {
        await _client.Indices.DeleteAsync("api", cancellationToken);
        await _client.Indices.CreateAsync("api", cancellationToken);
    }
}
