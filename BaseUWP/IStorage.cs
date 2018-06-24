using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base
{
    public interface IStorage
    {
        string[] GetFolderContent(string folder);
        bool CopyFile(string origin, string destination, bool move = false);
        bool MakeFolder(string path);
        bool IsFolder(string path);
        bool IsFile(string path);
    }
}
