using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    internal interface IRandomGenerator
    {
        int RandomNumber(int min, int max);
        string RandomString(int size, bool lowerCase = false);
        string RandomPassword();
    }
}
