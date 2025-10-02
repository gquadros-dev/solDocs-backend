using solDocs.Dtos;
using solDocs.Models;
using MongoDB.Driver;
using solDocs.Interfaces;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Net;

namespace solDocs.Services
{
    public class SearchService : ISearchService
    {
        private readonly IMongoCollection<TopicModel> _topicsCollection;
        private readonly IMongoCollection<ArticleModel> _articlesCollection;

        public SearchService(IMongoDatabase database)
        {
            _topicsCollection = database.GetCollection<TopicModel>("Topics");
            _articlesCollection = database.GetCollection<ArticleModel>("Articles");
        }

        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string searchText, string tenantId, bool isAuthenticated)
        {
            var topicFilterBuilder = Builders<TopicModel>.Filter;
            var topicFilter = topicFilterBuilder.Eq(t => t.TenantId, tenantId) &
                              topicFilterBuilder.Eq(t => t.DeletedAt, null) &
                              topicFilterBuilder.Regex(t => t.Name, new BsonRegularExpression(searchText, "i"));

            if (!isAuthenticated)
            {
                topicFilter &= topicFilterBuilder.Eq(t => t.Type, "public");
            }
            var topicsTask = _topicsCollection.Find(topicFilter).ToListAsync();

            var articleFilterBuilder = Builders<ArticleModel>.Filter;
            var articleFilter = articleFilterBuilder.Eq(a => a.TenantId, tenantId) &
                                articleFilterBuilder.Eq(a => a.DeletedAt, null) &
                                articleFilterBuilder.Text(searchText, new TextSearchOptions { Language = "pt" });

            var articlesTask = _articlesCollection.Find(articleFilter).ToListAsync();
            
            await Task.WhenAll(topicsTask, articlesTask);

            var topics = topicsTask.Result;
            var articles = articlesTask.Result;

            if (!isAuthenticated)
            {
                var publicTopicIds = topics.Select(t => t.Id).ToHashSet();
                articles = articles.Where(a => publicTopicIds.Contains(a.TopicId)).ToList();
            }

            var topicResults = topics.Select(t => new SearchResultDto
            {
                Id = t.Id, Name = t.Name, Type = "topic"
            });

            var articleResults = articles.Select(a => new SearchResultDto
            {
                Id = a.Id, Name = a.Title, Type = "article", Snippet = GenerateSnippet(a.Content, searchText) 
            });

            return topicResults.Concat(articleResults);
        }
        
        private static string? GenerateSnippet(string content, string searchText, int wordsAround = 5)
        {
            var plainText = Regex.Replace(content, "<.*?>", " ");
            plainText = WebUtility.HtmlDecode(plainText);

            var matchIndex = plainText.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase);

            if (matchIndex == -1)
            {
                return plainText.Length > 150 ? plainText.Substring(0, 150) + "..." : plainText;
            }

            var words = plainText.Split(new[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            var textToSearchWords = searchText.Split(new[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            int matchWordIndex = -1;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].IndexOf(textToSearchWords[0], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matchWordIndex = i;
                    break;
                }
            }

            if (matchWordIndex == -1) return null;

            var startIndex = Math.Max(0, matchWordIndex - wordsAround);
            var endIndex = Math.Min(words.Length - 1, matchWordIndex + textToSearchWords.Length - 1 + wordsAround);

            var snippetWords = words.ToList().GetRange(startIndex, endIndex - startIndex + 1);
            var snippet = string.Join(" ", snippetWords);

            var highlightedSnippet = Regex.Replace(snippet, searchText, $"<strong>{searchText}</strong>", RegexOptions.IgnoreCase);

            return $"...{highlightedSnippet}...";
        }
    }
}