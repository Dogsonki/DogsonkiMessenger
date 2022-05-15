using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Pages
{

    public class AfterLoginPageFlyoutMenuItem
    {
        public AfterLoginPageFlyoutMenuItem()
        {
            TargetType = typeof(AfterLoginPageFlyoutMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}