using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dewdrop.Models
{
    interface IModModel
    {
        /// <summary>
        /// Basic information in the form of <see cref="ModGenericModel"/> 
        /// </summary>
        /// <returns><see cref="ModGenericModel"/></returns>
        ModGenericModel ModInfo();
    }
}
