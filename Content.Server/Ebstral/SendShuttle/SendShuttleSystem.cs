using System.Numerics;
using Content.Shared.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Ebstral.ERT.SendShuttlePrototype;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Utility;

namespace Content.Server.Ebstral.ERT.SendShuttleSystem;

public sealed class SendShuttle : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public void SpawnShuttle(string shuttletype, bool playAnnonce)
    {
        var shuttleProto = _prototypeManager.Index<SendShuttlePrototype>(shuttletype);

        var playAnnounce = shuttleProto.ForsedAnnounce
            ? shuttleProto.DefaultIsAnnounce
            : playAnnonce;

        if (shuttleProto.IsLoadGrid)
            SpawnMapAndGrid(shuttleProto);

        if (!playAnnounce)
            return;

        if (shuttleProto.IsSetAlertLevel)
            SetAlertLevel(shuttleProto);

        if (shuttleProto.IsPlayAudio)
            PlayAudioAnnonce(shuttleProto);

        WriteAnnonce(shuttleProto);
    }

    private void SpawnMapAndGrid(SendShuttlePrototype proto)
    {
        // CS0618: IMapManager.CreateMap() устарел → используем SharedMapSystem.CreateMap()
        var mapUid = _mapSystem.CreateMap(out var mapId);
        _metaData.SetEntityName(
            mapUid,
            Loc.GetString("sent-shuttle-map-name")
        );

        var options = new DeserializationOptions();

        _mapLoader.TryLoadGrid(
            mapId,
            new ResPath(proto.GridPath),
            out _,
            options,
            new Vector2(0, 0));
    }

    private void PlayAudioAnnonce(SendShuttlePrototype proto)
    {
        var filter = Filter.Broadcast();
        var audioOption = AudioParams.Default.WithVolume(proto.Volume);

        // CS0144: ResolvedSoundSpecifier абстрактный — резолвим через SharedAudioSystem
        var resolved = _audio.ResolveSound(new SoundPathSpecifier(proto.AudioPath));
        _sound.PlayAdminGlobal(filter, resolved, audioOption);
    }

    private void WriteAnnonce(SendShuttlePrototype proto)
    {
        _chat.DispatchGlobalAnnouncement(
            Loc.GetString(proto.AnnouncementText),
            Loc.GetString(proto.AnnouncerText),
            proto.IsPlayAuidoFromAnnouncement,
            colorOverride: proto.AnnounceColor
        );
    }

    private void SetAlertLevel(SendShuttlePrototype proto)
    {
       if (proto.AlertLevelCode == null)
        return;

    var level = proto.AlertLevelCode.Value;

    foreach (var stationUid in _station.GetStations())
    {
        _alertLevel.SetLevel(
            stationUid,
            level,
            playSound: false,
            announce: true,
            force: true,
            locked: true);
        }
    }
}
