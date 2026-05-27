namespace NotificationService.Realtime
{
    public static class RealtimeGroups
    {
        public static string User(Guid userId) => $"user:{userId:D}";
        public static string Shipper(Guid shipperId) => $"shipper:{shipperId:D}";
    }
}
