using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement.Drivers;
using Orchard.Widgets.Models;

namespace Tad.ContentSync.Drivers
{
    public class LayerPartDriver : ContentPartDriver<LayerPart>
    {
        protected override DriverResult Display(LayerPart layerPart, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Layer",
                                () => shapeHelper.Parts_Layer(Name: layerPart.Name, Description: layerPart.Description));
        }
    }
}
