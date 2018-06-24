using System;
using System.IO;

namespace OneDriveSample.Helpers
{
    public interface IFilePicker
    {
        void GetFile(Action<string, Stream> callback);
    }
}