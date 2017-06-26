using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Entry
    {
        public Entry(string line)
        {
            line = line.Trim();
            int idx = line.IndexOf('=');          
            key = line.Substring(0, idx).Split(new char[] { '.' });
            value = line.Substring(idx + 1, line.Length - idx - 1);
        }

        public string[] key { get; private set; }
        public string value { get; private set; }
    }

    public class Option
    {
        public string optionID { get; set; } = null;
        public List<BuildEntry> paramList { get; }

        public Option(IEnumerable<Entry> optionEntries)
        {
            var titleEntry = optionEntries.FirstOrDefault(m => m.key.Length == 4);
            if (titleEntry != null)
            {
                optionID = titleEntry != null ? titleEntry.value : optionEntries.FirstOrDefault().key[2];

                var paramEntries = optionEntries.Where(m => m.key.Length >= 6 && m.key[4] == "build");
                paramList = paramEntries.Select(e => new BuildEntry(e)).ToList();
            }
        }
    }

    public class BuildEntry
    {
        public BuildEntry(Entry e)
        {
            var startIndex = Array.IndexOf(e.key, "build") + 1;
            name = "build";
            for (int i = startIndex; i < e.key.Length; i++)
            {
                name += "." + e.key[i];
            }
            value = e.value;
        }
        public String name { get; private set; }
        public String value { get; private set; }
    }
    
    public class Board
    {
        public String name { get; private set; }

        public List<BuildEntry> fixedOptions { get; private set; }

        public Dictionary<string, List<Option>> optionSets { get; } = new Dictionary<string, List<Option>>();

        public string core { get; private set; }      

        public bool ParseError { get; private set; }


        public Board(IEnumerable<Menu> menus, IEnumerable<Entry> entries)
        {
            ParseError = false;
            try
            {
                foreach (var menu in menus)
                {
                    optionSets.Add(menu.OptionSetID, new List<Option>());
                }
                parse(entries);

            }
            catch { ParseError = true; }
        }
        
        void parse(IEnumerable<Entry> entries)
        {
            name = entries.FirstOrDefault(e => e.key[1] == "name").value;

            // parse menu definitions
            foreach (var optionSet in optionSets)
            {
                var options = entries.Where(e => e.key[1] == "menu" && e.key[2] == optionSet.Key);  // all options from a menu
                if (options != null)
                {
                    foreach (var optionEntries in options.GroupBy(o => o.key[3]))                   // option entries grouped by optionId (e.g. serial, keyboard...)
                    {
                        var option = new Option(optionEntries);                                     // construct an option from the entries
                        if (option.optionID != null) optionSet.Value.Add(new Option(optionEntries));
                    }
                }
            }

            // parse board-fixed options
            fixedOptions = entries.Where(e => e.key[1] == "build").Select(e => new BuildEntry(e)).ToList();

            // get the core defintion of the board
            core = fixedOptions?.FirstOrDefault(o => o.name == "build.core")?.value ?? "unknown";            
        }
    }
}
