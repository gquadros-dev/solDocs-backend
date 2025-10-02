using AutoMapper;
using solDocs.Models;
using solDocs.Dtos.Article;

public class ArticleProfile : Profile
{
    public ArticleProfile()
    {
        CreateMap<CreateArticleDto, ArticleModel>();
        CreateMap<UpdateArticleDto, ArticleModel>();
    }
}