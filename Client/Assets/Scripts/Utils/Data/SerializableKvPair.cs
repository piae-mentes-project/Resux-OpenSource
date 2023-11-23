using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resux
{
    [Serializable]
    public class SerializableKvPair<TK, TV>
    {
        public TK Key;
        public TV Value;
    }
}
