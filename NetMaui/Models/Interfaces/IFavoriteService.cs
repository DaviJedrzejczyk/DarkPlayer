using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interfaces
{
    public interface IFavoriteService
    {
        void AddFavorite(string path);
        void RemoveFavorite(string path);
        List<string> LoadFavorites();
        bool IsFavorite(string path);
    }
}
