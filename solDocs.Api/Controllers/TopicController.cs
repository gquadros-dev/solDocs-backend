using AutoMapper;
using solDocs.Interfaces;
using solDocs.Models;
using solDocs.Dtos.Topic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/{tenantSlug}/topics")]
    public class TopicsController : BaseTenantController
    {
        private readonly ITopicService _topicService;
        private readonly IMapper _mapper;

        public TopicsController(ITopicService topicService, IMapper mapper, ITenantService tenantService): base(tenantService)
        {
            _topicService = topicService;
            _mapper = mapper;
        }

        /// <summary>
        /// Busca a árvore completa de tópicos de um determinado tipo (público ou privado).
        /// </summary>
        [HttpGet("public/tree")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicTopicTree(string tenantSlug)
        {
            var tenant = await _tenantService.GetBySlugAsync(tenantSlug);
            if (tenant == null) return NotFound(new { message = "Tenant não encontrado." });
            
            var tree = await _topicService.GetTopicTreeAsync(tenant.Id, "public", false);
            return Ok(tree);
        }
        
        /// <summary>
        /// Busca a árvore completa de tópicos de um determinado tipo (público ou privado).
        /// </summary>
        [HttpGet("private/tree")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrivateTopicTree()
        {
            if (TenantId == null) return Unauthorized();
            var tree = await _topicService.GetTopicTreeAsync(TenantId, "private", true);
            return Ok(tree);
        }

        /// <summary>
        /// Busca um tópico específico pelo seu ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TopicModel>> GetTopicById(string id)
        {
            if (TenantId == null) return Unauthorized();
            var topic = await _topicService.GetTopicByIdAsync(id, TenantId);
            if (topic == null) return NotFound();
            return Ok(topic);
        }

        /// <summary>
        /// Cria um novo tópico.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin, user")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TopicModel>> CreateTopic([FromBody] CreateUpdateTopicDto topicDto)
        {
            if (TenantId == null) return Unauthorized();

            var topicToCreate = _mapper.Map<TopicModel>(topicDto);
            topicToCreate.TenantId = TenantId;
            topicToCreate.ParentId = string.IsNullOrEmpty(topicDto.ParentId) ? null : topicDto.ParentId;

            var createdTopic = await _topicService.CreateTopicAsync(topicToCreate);
            
            var tenantSlug = (await GetTenantAsync())?.Slug;
            return CreatedAtAction(
                nameof(GetTopicById), 
                new { tenantSlug = tenantSlug, id = createdTopic.Id }, 
                createdTopic
            );
        }

        /// <summary>
        /// Atualiza um tópico existente.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin, user")]
        [ProducesResponseType(typeof(TopicModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTopic(string id, [FromBody] CreateUpdateTopicDto topicDto)
        {
            if (TenantId == null) return Unauthorized();
            
            var topicToUpdate = _mapper.Map<TopicModel>(topicDto);
            topicToUpdate.ParentId = string.IsNullOrEmpty(topicDto.ParentId) ? null : topicDto.ParentId;

            var updatedTopic = await _topicService.UpdateTopicAsync(id, TenantId, topicToUpdate);
            if (updatedTopic == null) return NotFound();

            return Ok(updatedTopic);
        }

        /// <summary>
        /// Busca todos os tópicos (em formato de lista, não árvore).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<TopicModel>> GetAllTopics()
        {
            if (TenantId == null) return new List<TopicModel>();
            return await _topicService.GetAllTopicsAsync(TenantId, string.Empty);
        }

        /// <summary>
        /// Exclui um tópico pelo seu ID.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin, user")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTopic(string id)
        {
            if (TenantId == null) return Unauthorized();
            var success = await _topicService.DeleteTopicAsync(id, TenantId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/type")]
        [Authorize(Roles = "admin, user")]
        [ProducesResponseType(typeof(TopicModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTopicType(string id, [FromBody] UpdateTopicTypeDto dto)
        {
            if (TenantId == null) return Unauthorized();
            var updatedTopic = await _topicService.UpdateTopicTypeAsync(id, TenantId, dto.Type);
            if (updatedTopic == null) return NotFound();
            return Ok(updatedTopic);
        }
    }
}