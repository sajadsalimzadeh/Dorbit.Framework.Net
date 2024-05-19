using System.Collections.Generic;

namespace Dorbit.Framework.Contracts;

public class NotificationDto
{
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public string Body { get; set; }
    public string Icon { get; set; }
    public string Badge { get; set; }
    public string Dir { get; set; }
    public string Image { get; set; }
    public string Lang { get; set; }
    public string Tag { get; set; }
    public object Data { get; set; }
    public bool Vibrate { get; set; }
    public bool Silent { get; set; }
    public List<NotificationActionDto> Actions { get; set; }
}

public class NotificationActionDto
{
    public string Action { get; set; }
    public string Title { get; set; }
    public string Icon { get; set; }
}