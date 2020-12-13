using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Class
{
    class LayeredField : Grid2D
    {
        SF2 bedrock;
        SF2 sand;

        LayeredField(SF2 bedrock) : base(bedrock)
        {

        }
        LayeredField(SF2 bedrock , SF2 sand) : base(bedrock)
        {

        }
        LayeredField(List<SF2> sflist) : base(sflist[0])
        {
            if(sflist.Count <= 0)
            {
                return;
            }
            //base.//(sflist[0]);
        }


    }
}
