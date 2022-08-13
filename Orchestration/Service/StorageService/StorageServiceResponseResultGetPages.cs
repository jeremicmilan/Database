using System.Collections.Generic;

namespace Database
{
    public class StorageServiceResponseResultGetPages : StorageServiceResponseResult
    {
        public List<Page> Pages { get; set; }

        public StorageServiceResponseResultGetPages()
        { }

        public StorageServiceResponseResultGetPages(List<Page> pages)
        {
            Pages = pages;
        }
    }
}
