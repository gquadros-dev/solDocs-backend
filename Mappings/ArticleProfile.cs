using AutoMapper;
using solDocs.Models;
using solDocs.Dtos;

public class ArticleProfile : Profile
{
    public ArticleProfile()
    {
        CreateMap<CreateArticleDto, ArticleModel>();
        CreateMap<UpdateArticleDto, ArticleModel>();
    }
}