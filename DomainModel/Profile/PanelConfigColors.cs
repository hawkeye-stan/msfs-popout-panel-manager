using System;
using System.Collections.Generic;
using System.Linq;

namespace MSFSPopoutPanelManager.DomainModel.Profile
{
    public class PanelConfigColors
    {
        public static string GetNextAvailableColor(List<PanelConfig> panelConfigs)
        {
            foreach (var colorName in Enum.GetNames<Colors>())
            {
                if (panelConfigs.Any(p => p.PanelSource.Color == colorName))
                    continue;

                return colorName;
            }

            return "White";
        }
    }

    public enum Colors
    {
        LimeGreen,
        Red,
        Blue,
        Fuchsia,
        Orange,
        Yellow,
        Cyan,
        Ivory,
        Pink,
        Indigo,
        Purple,
        Crimson
    }
}
