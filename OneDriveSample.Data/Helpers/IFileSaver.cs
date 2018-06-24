using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneDrive.Info;

namespace OneDriveSample.Helpers
{
    public interface IFileSaver
    {
        Task SaveFile(FileInfo file);
    }
}
