using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Extensions
{
    internal static class ValidationExtensions
    {
        internal static void ConfigureValidation(this IServiceCollection services)
        {
            services.AddMvc()
                .AddFluentValidation(options =>
                {
                    options.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                    options.LocalizationEnabled = false;
                    options.RegisterValidatorsFromAssemblyContaining(typeof(Baitkm.Validators.AssemblyReference));
                });
        }
    }
}