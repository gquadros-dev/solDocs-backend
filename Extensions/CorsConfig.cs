namespace solDocs.Extensions
{
    public static class CorsConfig
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowNextJsApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000", "http://170.81.43.121:3000") 
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            return services;
        }
    }
}