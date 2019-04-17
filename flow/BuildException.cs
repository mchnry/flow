using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    public class BuilderException: Exception
    {
        public BuilderException(string name): base()
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
