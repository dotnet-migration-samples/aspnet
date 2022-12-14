using NHibernate.Cfg.Loquacious;

namespace Orchard.Data {
    public interface IDatabaseCacheConfiguration : IDependency {
        void Configure(CacheConfigurationProperties cache);
    }
}