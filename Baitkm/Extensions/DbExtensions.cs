using Baitkm.DAL.Context;
using Baitkm.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Extensions
{
    internal static class DbExtensions
    {
        internal static void ConfigureDb(this IServiceCollection services, IConfiguration configuration)
        {
            ConstValues.ConnectionString = configuration["AppSettings:ConnectionString"];
            services.AddDbContext<BaitkmDbContext>(options =>
            {
                options.UseSqlServer(configuration["AppSettings:ConnectionString"]);
            });
        }
    }
}