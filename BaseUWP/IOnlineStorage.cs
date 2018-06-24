using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base
{
    public interface IOnlineStorage : IStorage
    {
        bool LogIn();
        bool IsConnected();
        bool LogOut();
    }
}
