using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Đăng ký AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Đăng ký các Services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IWarehouseService, WarehouseService>();

            return services;
        }
    }
}
