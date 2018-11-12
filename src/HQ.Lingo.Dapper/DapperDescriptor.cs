using System;
using Dapper;
using table_descriptor;

namespace tuxedo
{
    public class DapperDescriptor : SimpleDescriptor
    {
        protected DapperDescriptor(Type type) : base(type) { }

        public override void OnDescribed()
        {
            SqlMapper.SetTypeMap(Type, new DescriptorColumnMapper(Type, All));
        }
    }
}
