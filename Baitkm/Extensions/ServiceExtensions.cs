using Baitkm.Application.Middlewares.AuthorizationHandler;
using Baitkm.Application.Middlewares.IpAddress;
using Baitkm.Application.Middlewares.Managers;
using Baitkm.Application.Middlewares.Statistics;
using Baitkm.BLL.Services.Activities;
using Baitkm.BLL.Services.Announcements;
using Baitkm.BLL.Services.Cites;
using Baitkm.BLL.Services.Configurations;
using Baitkm.BLL.Services.Conversations;
using Baitkm.BLL.Services.Conversations.Messages;
using Baitkm.BLL.Services.Conversations.SupportConversations;
using Baitkm.BLL.Services.Conversations.SupportConversations.SupportMessages;
using Baitkm.BLL.Services.Currencies;
using Baitkm.BLL.Services.Emails;
using Baitkm.BLL.Services.Exchanges;
using Baitkm.BLL.Services.FAQs;
using Baitkm.BLL.Services.Geographic;
using Baitkm.BLL.Services.Notifications;
using Baitkm.BLL.Services.PhoneCodes;
using Baitkm.BLL.Services.SaveFilters;
using Baitkm.BLL.Services.Scheduler.Jobs;
using Baitkm.BLL.Services.Scheduler.Jobs.PushNotificationRelated;
using Baitkm.BLL.Services.Scheduler.Service;
using Baitkm.BLL.Services.Subscribes;
using Baitkm.BLL.Services.Token;
using Baitkm.BLL.Services.Users;
using Baitkm.BLL.Services.Users.Guests;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DAL.Repository.Firebase;
using Baitkm.Infrastructure.Helpers.Binders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.Extensions
{
    internal static class ServiceExtensions
    {
        internal static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IEntityRepository), typeof(EntityRepository));

            services.AddScoped<IAuthorizationHandler, ApiAuthorize>();//?

            services.AddScoped<IFirebaseRepository, FirebaseRepository>();
            services.AddScoped<ISchedulerService, SchedulerService>();

            #region Job
            services.AddScoped<IJob, AnnouncementDeactivationDayJob>();
            services.AddScoped<IJob, AnnouncementTopDeactivationDayJob>();
            services.AddScoped<IJob, PushNotificationSendingJob>();
            services.AddScoped<IRoleManager, RoleManager>();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<ILocateByIpAddress, LocateByIpAddress>();
            services.AddScoped<IJob, ExpiredAnnouncementJob>();
            services.AddScoped<IJob, AnnoucmentPricesJob>();
            #endregion

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IFAQService, FAQService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<ISaveFilterService, SaveFilterService>();
            services.AddScoped<IGuestService, GuestService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<ISupportConversationService, SupportConversationService>();
            services.AddScoped<ISupportMessageService, SupportMessageService>();
            services.AddScoped<IGeographicService, GeographicService>();
            services.AddScoped<IPhoneCodeService, PhoneCodeService>();
            services.AddScoped<IOptionsBinder, OptionsBinder>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IExchangeService, ExchangeService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ISubscribeService, SubscribeService>();
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}