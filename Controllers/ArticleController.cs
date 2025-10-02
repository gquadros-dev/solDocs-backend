using System.Security.Claims;
using AutoMapper;
using solDocs.Interfaces;
using solDocs.Models;
using solDocs.Dtos.Article;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/{tenantSlug}/articles")]
    public class ArticlesController: BaseTenantController
    {
        private readonly IArticleService _articleService;
        private readonly ITopicService _topicService;
        private readonly IMapper _mapper;

        public ArticlesController(IArticleService articleService, ITopicService topicService, IMapper mapper, ITenantService tenantService)
            : base(tenantService) 
        {
            _articleService = articleService;
            _topicService = topicService;
            _mapper = mapper;
        }

        /// <summary>
        /// Busca um artigo público específico pelo seu ID.
        /// </summary>
        [HttpGet("public/{articleId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ArticleModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleModel>> GetPublicArticleById(string tenantSlug, string articleId)
        {
            var tenant = await _tenantService.GetBySlugAsync(tenantSlug);
            if (tenant == null) return NotFound(new { message = "Tenant não encontrado." });

            var article = await _articleService.GetArticleByIdAsync(articleId, tenant.Id);
            if (article == null) return NotFound(new { message = "Artigo não encontrado." });

            var topic = await _topicService.GetTopicByIdAsync(article.TopicId, tenant.Id);
            if (topic == null || topic.Type == "private")
            {
                return Unauthorized(new { message = "Este conteúdo é privado ou não existe." });
            }

            return Ok(article);
        }

        /// <summary>
        /// Busca um artigo específico pelo seu ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ArticleModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleModel>> GetArticleById(string id)
        {
            if (TenantId == null) return Unauthorized();

            var article = await _articleService.GetArticleByIdAsync(id, TenantId);
            if (article == null) return NotFound();
            
            return Ok(article);
        }

        /// <summary>
        /// Cria um novo artigo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ArticleModel>> CreateArticle([FromBody] CreateArticleDto articleDto)
        {
            if (TenantId == null) return Unauthorized();
            
            var articleToCreate = _mapper.Map<ArticleModel>(articleDto);
            articleToCreate.TenantId = TenantId;

            var createdArticle = await _articleService.CreateArticleAsync(articleToCreate);
            
            
            var tenantSlug = (await GetTenantAsync())?.Slug;
            return CreatedAtAction(
                nameof(GetArticleById),
                new { tenantSlug = tenantSlug, id = createdArticle.Id }, 
                createdArticle
            );
        }

        /// <summary>
        /// Atualiza um artigo existente.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(string id, [FromBody] UpdateArticleDto articleDto)
        {
            if (TenantId == null) return Unauthorized();

            var articleToUpdate = _mapper.Map<ArticleModel>(articleDto);
            var updatedArticle = await _articleService.UpdateArticleAsync(id, TenantId, articleToUpdate);

            if (updatedArticle == null) return NotFound(); 

            return Ok(updatedArticle);
        }

        /// <summary>
        /// Busca todos os artigos pelo ID do tópico.
        /// </summary>
        [HttpGet("topic/{topicId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ArticleModel>>> GetArticlesByTopicId(string topicId)
        {
            if (TenantId == null) return Unauthorized();

            var articles = await _articleService.GetArticlesByTopicIdAsync(topicId, TenantId);
            return Ok(articles);
        }

        /// <summary>
        /// Busca todos os artigos.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<ArticleModel>> GetAllArticles()
        {
            if (TenantId == null) return new List<ArticleModel>();

            return await _articleService.GetAllArticlesAsync(TenantId);
        }

        /// <summary>
        /// Exclui um artigo pelo seu ID.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            if (TenantId == null) return Unauthorized();

            var success = await _articleService.DeleteArticleAsync(id, TenantId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}