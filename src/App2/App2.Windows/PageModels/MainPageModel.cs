
namespace App2.PageModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TankardDB.Core;

    public class MainPageModel
    {
        public async void Load()
        {
            var db = new Tankard(null);
        }
    }
}
