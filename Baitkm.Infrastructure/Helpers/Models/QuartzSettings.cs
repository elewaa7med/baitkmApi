using System.ComponentModel;

namespace Baitkm.Infrastructure.Helpers.Models
{
    public class QuartzSettings
    {
        [DisplayName("quartz.jobStore.type")]
        public static string JobStoreType { get; set; }
        [DisplayName("quartz.jobStore.useProperties")]
        public static bool JobStoreUseProperties { get; set; }
        [DisplayName("quartz.jobStore.dataSource")]
        public static string JobStoreDataSource { get; set; }
        [DisplayName("quartz.jobStore.tablePrefix")]
        public static string JobStoreTablePrefix { get; set; }
        [DisplayName("quartz.jobStore.driverDelegateType")]
        public static string JobStoreDriverDelegateType { get; set; }
        [DisplayName("quartz.dataSource.default.connectionString")]
        public static string ConnectionString { get; set; }
        [DisplayName("quartz.dataSource.default.provider")]
        public static string Provider { get; set; }
        [DisplayName("quartz.serializer.type")]
        public static string SerializerType { get; set; }
    }
}
