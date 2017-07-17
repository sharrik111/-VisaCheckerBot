using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Storage
{
    public interface IStorageService
    {
        void Save(string content);

        string Load();
    }
}
