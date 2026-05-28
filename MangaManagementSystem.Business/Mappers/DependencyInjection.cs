using AutoMapper;
using MangaManagement.DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Mappers
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterInfrastructure(
                this IServiceCollection services)
        {

            services.AddAutoMapper(typeof(DependencyInjection));
            return services;
        }
    }
}
