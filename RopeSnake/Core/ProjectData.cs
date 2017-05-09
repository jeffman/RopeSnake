using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public abstract class ProjectData
    {
        /// <summary>
        /// A strong, unique hash of this ProjectData instance.
        /// </summary>
        /// <returns>a hash in a format to be defined later</returns>
        public virtual string GetProjectHash()
        {
            throw new NotImplementedException();
        }
    }
}
