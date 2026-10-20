using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Explosion;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.Ebstral.Commands;

[AdminCommand(AdminFlags.Spawn)]
public sealed class BSAHitCommand : IConsoleCommand
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IEntitySystemManager _systems = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Command => "bsahit";
    public string Description => Loc.GetString("bsa-hit-description");
    public string Help => Loc.GetString("bsa-hit-help");

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;

        if (player?.AttachedEntity == null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        if (!_entManager.TryGetComponent(player.AttachedEntity, out TransformComponent? xform))
        {
            shell.WriteError(Loc.GetString("bsa-hit-coords-error"));
            return;
        }

        // CS0246: убираем лишний TransformSystem — xform.Coordinates достаточно
        // CS0618: xform.MapPosition устарел — используем xform.Coordinates (EntityCoordinates)
        var coords = xform.Coordinates;

        var filter = Filter.Broadcast();
        var audioParams = AudioParams.Default.WithVolume(0);

        // SharedAudioSystem — EntitySystem, получаем через GetEntitySystem, не через [Dependency]
        var audio = _systems.GetEntitySystem<SharedAudioSystem>();
        var resolved = audio.ResolveSound(new SoundPathSpecifier("/Audio/Corvax/Adminbuse/artillery.ogg"));
        _entManager.System<ServerGlobalSoundSystem>().PlayAdminGlobal(
            filter,
            resolved,
            audioParams,
            true);

        _systems.GetEntitySystem<ChatSystem>().DispatchGlobalAnnouncement(
            Loc.GetString("bsa-hit-announcement"),
            Loc.GetString("bsa-hit-announcer"),
            playSound: false,
            colorOverride: Color.Gold);

        // Захватываем EntityUid до входа в лямбду
        // CS1503: QueueExplosion теперь принимает EntityUid, не EntityCoordinates
        var sourceUid = player.AttachedEntity.Value;

        Timer.Spawn(4500, () =>
        {
            if (!_prototypeManager.TryIndex<ExplosionPrototype>("Cryo", out var explosion))
                return;

            _systems.GetEntitySystem<ExplosionSystem>().QueueExplosion(
                sourceUid,
                explosion.ID,
                20000,
                5,
                50);
        });

        _adminLogger.Add(
            LogType.Action,
            LogImpact.High,
            $"{player} used BSA. Coords - {coords}");
    }
}