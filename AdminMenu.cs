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
                        .Add(child=>child.Caption(T("All Content"))
                                .Position("0")
                                .Action("Prepare", "Admin", new {area = "Tad.ContentSync", filter="all"})
                                .LocalNav())
                        .Add(child=>child.Caption(T("Same"))
                                .Position("1")
                                .Action("Prepare", "Admin", new {area = "Tad.ContentSync", filter="same"})
                                .LocalNav())
                        .Add(child => child.Caption(T("Different"))
                                .Position("2")
                                .Action("Prepare", "Admin", new { area = "Tad.ContentSync", filter = "different" })
                                .LocalNav())
                        .Add(child => child.Caption(T("Similar"))
                                .Position("3")
                                .Action("Prepare", "Admin", new { area = "Tad.ContentSync", filter = "mismatch" })
                                .LocalNav())
                        .Add(child => child.Caption(T("Local Only"))
                                .Position("4")
                                .Action("Prepare", "Admin", new { area = "Tad.ContentSync", filter = "localonly" })
                                .LocalNav())
                        .Add(child => child.Caption(T("Remote Only"))
                                .Position("5")
                                .Action("Prepare", "Admin", new { area = "Tad.ContentSync", filter = "remoteonly" })
                                .LocalNav()))
                .Add(T("Settings"), menu => menu
                    .Add(T("Content Sync"), "99", item => item.Action("Settings", "Admin", new {area = "Tad.ContentSync"})));
        }
    }
}