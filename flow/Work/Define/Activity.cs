using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Mchnry.Flow.Work.Define
{
    public class Activity
    {

        public string Id { get; set; }

        public List<Reaction> Reactions { get; set; }
    }

    public class Reaction
    {


        public string Logic { get; set; }
 
        public string Work { get; set; }
    }
}
