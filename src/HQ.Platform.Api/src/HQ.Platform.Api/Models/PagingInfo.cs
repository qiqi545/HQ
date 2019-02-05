using System.Runtime.Serialization;

namespace HQ.Platform.Api.Models
{
    [DataContract]
    public struct PagingInfo
    {
        [DataMember]
        public long TotalCount;

        [DataMember]
        public long TotalPages;

        [DataMember]
        public string FirstPage;

        [DataMember]
        public string NextPage;

        [DataMember]
        public string PreviousPage;

        [DataMember]
        public string LastPage;
    }
}