using Robust.Shared.Prototypes;

namespace Content.Shared.Ebstral.ERT.SendShuttlePrototype;

[Prototype("shuttleType")]
public sealed partial class SendShuttlePrototype : IPrototype  // partial — требование анализатора v268
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public bool DefaultIsAnnounce = true;

    [DataField]
    public bool ForsedAnnounce = false;

    [DataField]
    public Color AnnounceColor = Color.Gold;

    [DataField]
    public bool IsPlayAuidoFromAnnouncement = false;

    [DataField]
    public string AnnouncementText = string.Empty;

    [DataField]
    public string AnnouncerText = "shuttle-send-default-announcer";

    [DataField]
    public bool IsPlayAudio = true;

    [DataField]
    public string AudioPath = "/Audio/Corvax/Adminbuse/ertyes.ogg";

    [DataField]
    public int Volume = 0;

    [DataField]
    public bool IsLoadGrid = true;

    [DataField]
    public string GridPath = string.Empty;

    [DataField]
    public bool IsSetAlertLevel = false;

    [DataField]
    public string AlertLevelCode = string.Empty;

    [DataField]
    public string HintText = string.Empty;
}
