using Baitkm.Entities;
using Baitkm.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.DAL.Context
{
    public interface IBaitkmDbContext : IDisposable
    {
        DbSet<User> Users { get; set; }
        DbSet<Password> Passwords { get; set; }
        DbSet<Verified> Verifieds { get; set; }
        DbSet<DeviceToken> DeviceTokens { get; set; }
        DbSet<City> Cities { get; set; }
        DbSet<Configuration> Configurations { get; set; }
        DbSet<FAQ> FAQs { get; set; }
        DbSet<PersonSetting> PersonSettings { get; set; }
        DbSet<PersonOtherSetting> PersonOtherSettings { get; set; }
        DbSet<Announcement> Announcements { get; set; }
        DbSet<Attachment> Attachments { get; set; }
        DbSet<Favourite> Favourite { get; set; }
        DbSet<AnnouncementFeature> Features { get; set; }
        DbSet<SaveFilter> SaveFilters { get; set; }
        DbSet<SaveFilterFeature> SaveFilterFeatures { get; set; }
        DbSet<Guest> Guests { get; set; }
        DbSet<Conversation> Conversations { get; set; }
        DbSet<Message> Messages { get; set; }
        DbSet<AnnouncementReport> AnnouncementReports { get; set; }
        DbSet<PushNotification> PushNotifications { get; set; }
        DbSet<SupportConversation> SupportConversations { get; set; }
        DbSet<SupportMessage> SupportMessages { get; set; }
        DbSet<HomePageCoverImage> HomePageCoverImages { get; set; }
        //DbSet<PersonSetting> GuestSubscriptions { get; set; }
        //DbSet<PersonOtherSetting> GuestSubscriptionAreaAndLanguage { get; set; }
        DbSet<PhoneCode> PhoneCodes { get; set; }
        DbSet<Statistic> Statistics { get; set; }
        DbSet<PersonNotification> PersonNotifications { get; set; }
        DbSet<UserLocation> UserLocations { get; set; }
        DbSet<SubscribeAnnouncement> SubscribeAnnouncements { get; set; }
        DbSet<Email> Emails { get; set; }

        DbSet<TEntity> WriterSet<TEntity>() where TEntity : EntityBase;
        IQueryable<TEntity> ReaderSet<TEntity>(bool includeDeleted = false) where TEntity : EntityBase;
        Task<int> SaveChangesAsync(CancellationToken token = default);
        int SaveChanges();
        DatabaseFacade Database { get; }
        EntityEntry Entry(object entity);
    }
}