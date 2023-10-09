using Flunt.Notifications;

namespace TodoApp.Data.Extensions;

public static class NotificationErrorsFormatterExtension
{
    public static Dictionary<string, string[]> ConvertToErrorsDictionary(this IReadOnlyCollection<Notification> notifications)
    {
        var errorMessages = ExtractDistinctNotificationMessages(notifications).ToArray();
        var errors = new Dictionary<string, string[]> { {"Error", errorMessages } };
        return errors;

    }

    public static Dictionary<string, string[]> ConvertToErrorsDictionary(this Exception exception)
    {
        return new Dictionary<string, string[]> { { "Error", new string[] { exception.Message } } };
    }

    static IEnumerable<string> ExtractDistinctNotificationMessages(IReadOnlyCollection<Notification> notifications)
    {
        var ExtractMessages = (IGrouping<string, Notification> g) => g.Select(n => n.Message);
        return notifications
            .GroupBy(n => n.Key)
            .SelectMany(g => ExtractMessages(g))
            .Distinct();
    }
}
