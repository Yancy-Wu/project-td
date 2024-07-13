using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Ec {
    public static class IdManager {
        public static string GenUUID() {
            return Guid.NewGuid().ToString();
        }
    }
}
