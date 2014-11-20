using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Settings;
using NHibernate;

namespace MrCMS.Services.CloneSite
{
    [CloneSitePart(-70)]
    public class UpdateWebpages : ICloneSiteParts
    {
        private readonly ICloneWebpageSiteParts _cloneWebpageSiteParts;
        private readonly ISession _session;

        public UpdateWebpages(ICloneWebpageSiteParts cloneWebpageSiteParts, ISession session)
        {
            _cloneWebpageSiteParts = cloneWebpageSiteParts;
            _session = session;
        }

        public void Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            var webpages = _session.QueryOver<Webpage>().Where(webpage => webpage.Site.Id == to.Id).List();
            foreach (var webpage in webpages)
            {
                var original = siteCloneContext.GetOriginal(webpage);
                if (original != null)
                    _cloneWebpageSiteParts.Clone(original, webpage, siteCloneContext);
            }
        }
    }

    [CloneSitePart(-65)]
    public class UpdateErrorPages : ICloneSiteParts
    {
        private readonly ILegacySettingsProvider _legacySettingsProvider;

        public UpdateErrorPages(ILegacySettingsProvider legacySettingsProvider)
        {
            _legacySettingsProvider = legacySettingsProvider;
        }

        public void Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            var fromProvider = new ConfigurationProvider(@from, _legacySettingsProvider);
            var fromSiteSettings = fromProvider.GetSiteSettings<SiteSettings>();
            var toProvider = new ConfigurationProvider(@to, _legacySettingsProvider);
            var toSiteSettings = toProvider.GetSiteSettings<SiteSettings>();

            var error403 = siteCloneContext.Find<Webpage>(fromSiteSettings.Error403PageId);
            if (error403 != null) toSiteSettings.Error403PageId = error403.Id;

            var error404 = siteCloneContext.Find<Webpage>(fromSiteSettings.Error404PageId);
            if (error404 != null) toSiteSettings.Error404PageId = error404.Id;

            var error500 = siteCloneContext.Find<Webpage>(fromSiteSettings.Error500PageId);
            if (error500 != null) toSiteSettings.Error500PageId = error500.Id;

            toProvider.SaveSettings(toSiteSettings);
        }
    }
}