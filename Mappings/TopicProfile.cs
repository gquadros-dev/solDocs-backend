using AutoMapper;
using solDocs.Models;
using solDocs.Dtos;

namespace solDocs.Mappings
{
    public class TopicProfile : Profile
    {
        public TopicProfile()
        {
            CreateMap<CreateUpdateTopicDto, TopicModel>();
            CreateMap<TopicModel, CreateUpdateTopicDto>();
        }
    }
}