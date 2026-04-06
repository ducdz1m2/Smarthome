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

            // Sau này có thêm Service mới, bạn chỉ cần thêm vào đây
            // services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
