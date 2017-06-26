﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel;

namespace Model
{
    

    class FileContent
    {
        public IEnumerable<Board> boards { get; private set; }
        public IEnumerable<Menu> menus { get; private set; }

        public bool ParseError { get; private set; }
        public bool CoreError { get; private set; }

        public FileContent(string fileName)
        {
            ParseError = false;

            try
            {
                var lines = readLines(fileName);

                var menuEntries = lines.Where(l => l.key[0] == "menu");
                menus = menuEntries.Select(e => new Menu(e.key[1], e.value));

                var boardEntries = lines.Where(l => l.key[0] != "menu").ToLookup(k => k.key[0]); // entries grouped by boardID
                boards = boardEntries.Select(e => new Board(menus, e));

                ParseError = boards.Any(b => b.ParseError);
            }
            catch
            {
                ParseError = true;
            }
        }

        List<Entry> readLines(string filename)
        {
            var lines = new List<Entry>();
            using (TextReader reader = new StreamReader(filename))
            {
                while (true)
                {
                    String line = reader.ReadLine();
                    if (line == null) break;

                    var cmdPos = line.IndexOf('#');
                    if (cmdPos != -1)
                    {
                        line = line.Substring(0, cmdPos);
                    }

                    if (line == "") continue;
                    lines.Add(new Entry(line));
                }
            }
            return lines;

        }
    }
}
