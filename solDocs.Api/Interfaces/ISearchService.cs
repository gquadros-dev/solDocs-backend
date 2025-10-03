using solDocs.Dtos;

namespace solDocs.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<SearchResultDto>> SearchAsync(string searchText, string tenantId, bool isAuthenticated);
    }   
}