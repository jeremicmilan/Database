using System.Collections.Generic;

namespace Database
{
    public class StorageServiceResponseResultGetPage : StorageServiceResponseResult
    {
        public Page Page { get; set; }

        public StorageServiceResponseResultGetPage()
        { }

        public StorageServiceResponseResultGetPage(Page page)
        {
            Page = page;
        }
    }
}
