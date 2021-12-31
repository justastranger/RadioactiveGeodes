using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace RadioactiveGeodes
{
    public interface IGenericModConfigMenuApi
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);
        void UnregisterModConfig(IManifest mod);

        void StartNewPage(IManifest mod, string pageName);
        void OverridePageDisplayName(IManifest mod, string pageName, string displayName);
        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage);
        void RegisterParagraph(IManifest mod, string paragraph);


        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet,
            Action<int> optionSet);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet,
            Action<int> optionSet, int min, int max);

        void OpenModMenu(IManifest mod);

        void SetDefaultIngameOptinValue(IManifest mod, bool optedIn);
    }
}
