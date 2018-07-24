namespace Tangy.Utility
{
    public static class StaticDetails
    {
        public const string DefaultFoodImage = "\\images\\default-menu-item-img.jpg";
        public const string AdminEndUser = "Admin";
        public const string CustomerEndUser = "Customer";

        public static class OrderStatus
        {
            public const string Submitted = "Submitted";
            public const string InProcess = "Being Prepared";
            public const string Ready = "Ready for Pickup";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
        }
    }
}
