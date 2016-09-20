using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViDi2.Camera
{
    public interface ICameraProvider
    {

        /// <summary>
        /// 
        /// </summary>
        string Name {get;}
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<ICamera> Discover();
        /// <summary>
        /// 
        /// </summary>
        ReadOnlyCollection<ICamera> Cameras { get; }
    }
}
