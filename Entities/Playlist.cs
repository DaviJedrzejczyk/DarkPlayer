using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Playlist
    {
        public string Name { get; set; }
        public List<string> Musics { get; set; }

        public Playlist()
        {
            
        }

        public Playlist(string name, List<string> musics)
        {
            this.Name = name;
            this.Musics = musics;
        }
    }
}
