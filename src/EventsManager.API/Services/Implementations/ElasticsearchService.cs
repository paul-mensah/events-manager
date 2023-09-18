using EventsManager.API.Services.Interfaces;
using Nest;
using Newtonsoft.Json;

namespace EventsManager.API.Services.Implementations;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(ILogger<ElasticsearchService> logger,
        IElasticClient elasticClient)
    {
        _logger = logger;
        _elasticClient = elasticClient;
    }

    public async Task<T> GetByIdAsync<T>(string id) where T : class
    {
        var getResponse = await _elasticClient.GetAsync<T>(id);

        if (!getResponse.IsValid)
        {
            _logger.LogError(getResponse.OriginalException,
                "An error occured getting document by id:{eventId}\n{debugInformation}",
                id, getResponse.DebugInformation);

            return null;
        }

        return getResponse.Source;
    }

    public async Task<bool> AddAsync<T>(T doc) where T : class
    {
        IndexResponse indexResponse = await _elasticClient.IndexDocumentAsync(doc);

        if (!indexResponse.IsValid)
            _logger.LogError(indexResponse.OriginalException,
                "An error occured indexing document\n{debugInformation}\n{document}",
                indexResponse.DebugInformation, JsonConvert.SerializeObject(doc, Formatting.Indented));

        return indexResponse.IsValid;
    }

    public async Task<bool> DeleteAsync<T>(string id) where T : class
    {
        DeleteResponse deleteResponse = await _elasticClient.DeleteAsync<T>(id);

        if (!deleteResponse.IsValid)
            _logger.LogError(deleteResponse.OriginalException,
                "An error occured deleting document by id:{eventId}\n{debugInformation}",
                id, deleteResponse.DebugInformation);

        return deleteResponse.IsValid;
    }

    public async Task<bool> UpdateAsync<T>(string id, T doc) where T : class
    {
        var updateResponse = await _elasticClient.UpdateAsync<T>(id, selector => selector.Doc(doc));

        if (!updateResponse.IsValid)
            _logger.LogError(updateResponse.OriginalException,
                "An error occured updating document by id:{eventId}\n{debugInformation}",
                id, updateResponse.DebugInformation);

        return updateResponse.IsValid;
    }

    public async Task<ISearchResponse<T>> SearchAsync<T>(SearchRequest<T> searchRequest) where T : class
    {
        var searchResponse = await _elasticClient.SearchAsync<T>(searchRequest);

        if (!searchResponse.IsValid)
            _logger.LogError(searchResponse.OriginalException,
                "An error occured searching for documents\n{debugInformation}", searchResponse.DebugInformation);

        return searchResponse;
    }
}