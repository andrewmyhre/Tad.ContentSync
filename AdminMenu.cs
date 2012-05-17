using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Tad.ContentSync {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("contentsync")
                .Add(root => root
                        .Caption(T("Content Sync"))
                        .Position("1")
                        .LinkToFirstChild(true)
                        .Add(child=>child.Caption(T("Overview"))
                                .Position("0")
                                .Action("Overview", "Admin", new {area = "Tad.ContentSync"})
                                .LocalNav())
                        .Add(child => child.Caption(T("Local Only"))
                                .Position("1")
                                .Action("Comparison", "Admin", new { area = "Tad.ContentSync", type = "LocalOnly" })
                                .LocalNav())
                        .Add(child => child.Caption(T("Remote Only"))
                                .Position("1")
                                .Action("Comparison", "Admin", new { area = "Tad.ContentSync", type = "RemoteOnly" })
                                .LocalNav())
                        .Add(child => child.Caption(T("Differences"))
                                .Position("1")
                                .Action("Comparison", "Admin", new { area = "Tad.ContentSync", type = "Differences" })
                                .LocalNav())
                        .Add(child => child.Caption(T("Mismatches"))
                                .Position("2")
                                .Action("Comparison", "Admin", new { area = "Tad.ContentSync", type = "Mismatches" })
                                .LocalNav()))
                .Add(T("Settings"), menu => menu
                    .Add(T("Content Sync"), "99", item => item.Action("Settings", "Admin", new {area = "Tad.ContentSync"})));
        }
    }
}