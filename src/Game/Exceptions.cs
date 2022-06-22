using System;
using System.IO;

namespace cliAppleWorm
{

    public class InvalidLevelFile : Exception
    {
        public FileInfo File;

        public InvalidLevelFile() : base()
        {

        }

        public InvalidLevelFile(string message) : base(message)
        {

        }

        public InvalidLevelFile(FileInfo file) : base(file.FullName)
        {
            this.File = file;
        }

        public InvalidLevelFile(string message, FileInfo file) : this(file)
        {
            this.File = file;
        }
    }
}
