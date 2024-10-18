using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Interfaces
{
    public interface IFileSaverService
    {
        Task SaveFileAsync(string fileName, byte[] data);
    }
}
