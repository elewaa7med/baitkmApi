using Baitkm.Entities;
using Baitkm.Entities.Base;
using Baitkm.Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.DAL.Context
{
    public class BaitkmDbContext : DbContext, IBaitkmDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BaitkmDbContext(DbContextOptions options) : base(options)
        {
            _httpContextAccessor = new HttpContextAccessor();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<Verified> Verifieds { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<PersonSetting> PersonSettings { get; set; }
        public DbSet<PersonOtherSetting> PersonOtherSettings { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Favourite> Favourite { get; set; }
        public DbSet<AnnouncementFeature> Features { get; set; }
        public DbSet<SaveFilter> SaveFilters { get; set; }
        public DbSet<SaveFilterFeature> SaveFilterFeatures { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PushNotification> PushNotifications { get; set; }
        public DbSet<AnnouncementReport> AnnouncementReports { get; set; }
        public DbSet<SupportConversation> SupportConversations { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<HomePageCoverImage> HomePageCoverImages { get; set; }
        //public DbSet<PersonSetting> GuestSubscriptions { get; set; }
        //public DbSet<PersonOtherSetting> GuestSubscriptionAreaAndLanguage { get; set; }
        public DbSet<PhoneCode> PhoneCodes { get; set; }
        public DbSet<Statistic> Statistics { get; set; }
        public DbSet<PersonNotification> PersonNotifications { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Fact> Facts { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<SubscribeAnnouncement> SubscribeAnnouncements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var decimals = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal));
            foreach (var property in decimals)
            {
                property.Relational().ColumnType = "decimal(18, 6)";
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;
            optionsBuilder.EnableSensitiveDataLogging();
            var connectionString = AppSettings.ConnectionString;
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<TEntity> WriterSet<TEntity>() where TEntity : EntityBase
        {
            return base.Set<TEntity>();
        }

        public IQueryable<TEntity> ReaderSet<TEntity>(bool includeDeleted = false) where TEntity : EntityBase
        {
            //if (!includeDeleted)
            //    return base.Set<TEntity>().Where(e => !e.IsDeleted);//.Where(x => !x.IsDeleted);
            return base.Set<TEntity>().AsQueryable();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken token = default)
        {
            //AddTimestamps(_httpContextAccessor);
            return await base.SaveChangesAsync(token);
        }

        public override int SaveChanges()
        {
            //AddTimestamps(_httpContextAccessor);
            return base.SaveChanges();
        }

        #region private
        //private void AddTimestamps(IHttpContextAccessor httpContextAccessor)
        //{
        //    var entities = ChangeTracker.Entries().Where(x => x.Entity is EntityBase && (x.State == EntityState.Added || x.State == EntityState.Modified));
        //    int.TryParse(httpContextAccessor.HttpContext?.User?.Claims?.Where(c => c.Type == "userId")
        //        .Select(c => c.Value).FirstOrDefault(), out var currentUserId);

        //    foreach (var entity in entities)
        //    {
        //        if (entity.State == EntityState.Added)
        //        {
        //            ((EntityBase)entity.Entity).CreatedDt = DateTime.UtcNow;
        //            ((EntityBase)entity.Entity).CreatedBy = currentUserId;
        //        }

        //        ((EntityBase)entity.Entity).UpdatedDt = DateTime.UtcNow;
        //        ((EntityBase)entity.Entity).UpdatedBy = currentUserId;
        //    }
        //}
        #endregion
    }
}
