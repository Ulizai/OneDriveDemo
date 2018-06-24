using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Base;

namespace OneDriveFacadeUWP
{
    class OneDriveStorage : IOnlineStorage
    {
        OneDriveSimpleSample.OneDriveService driveLink;
        public OneDriveStorage()
        {
            driveLink = new OneDriveSimpleSample.OneDriveService(DateTime.Now.ToString());
        }

        public bool CopyFile(string origin, string destination, bool move = false)
        {
            throw new NotImplementedException();
        }

        public string[] GetFolderContent(string folder)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool IsFolder(string path)
        {
            throw new NotImplementedException();
        }

        public bool LogIn()
        {
            return true;
        }

        public bool LogOut()
        {
            throw new NotImplementedException();
        }

        public bool MakeFolder(string path)
        {
            throw new NotImplementedException();
        }
    }
}
