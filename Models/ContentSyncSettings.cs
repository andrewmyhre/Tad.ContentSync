using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Tad.ContentSync.Models
{
    public class ContentSyncSettings
    {
        public virtual int Id { get; set; }
        [Display(Name="Remote Instance Url")]
        public virtual string RemoteUrl { get; set; }
    }
}
