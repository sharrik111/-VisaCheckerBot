using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Checkers.Storage
{
    public class FileStreamStorageService : IStorageService
    {
        #region Ctors

        public FileStreamStorageService(string filename)
        {
            if(string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Parameter is null or empty.", "filename");
            }
            Filename = filename;
        }

        #endregion

        #region Properties

        private string Filename { get; set; }

        #endregion

        #region IStorageService

        public string Load()
        {
            try
            {
                using (var reader = new StreamReader(Filename))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public void Save(string content)
        {
            try
            {
                using (var writer = new StreamWriter(Filename))
                {
                    writer.Write(content);
                }
            }
            catch { }
        }
        
        #endregion
    }
}
