using System;

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