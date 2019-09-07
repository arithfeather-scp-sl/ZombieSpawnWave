using ArithFeather.ArithSpawningKit.RandomPlayerSpawning;
using ArithFeather.ArithSpawningKit.SpawnPointTools;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System.Collections.Generic;

namespace ArithFeather.ZombieSpawnWave
{
	[PluginDetails(
		author = "Arith",
		name = "Zombie Spawn Wave",
		description = "",
		id = "ArithFeather.ZombieSpawnWave",
		configPrefix = "zsw",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	public class ZombieSpawnWave : Plugin, IEventHandlerWaitingForPlayers, IEventHandlerTeamRespawn
	{
		public override void Register()
		{
			AddEventHandlers(this);
		}
		public override void OnEnable() => Info("ZombieSpawnWave Enabled");
		public override void OnDisable() => Info("ZombieSpawnWave Disabled");

		[ConfigOption] private readonly bool disablePlugin = false;
		[ConfigOption] private readonly int percentChanceToSpawnZombies = 20;
		[ConfigOption] private readonly string zombieAnnounceMessage = "SCP 0 4 9 2 CONTAINMENT BREACH IN ENTRANCE ZONE";

		private List<SpawnPoint> zombieSpawns;
		public List<SpawnPoint> ZombieSpawns => zombieSpawns ?? (zombieSpawns = new List<SpawnPoint>());

		private List<PlayerSpawnPoint> loadedZombieSpawns;
		public List<PlayerSpawnPoint> LoadedZombieSpawns => loadedZombieSpawns ?? (loadedZombieSpawns = new List<PlayerSpawnPoint>());

		private bool useZombies = false;
		
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (disablePlugin)
			{
				PluginManager.DisablePlugin(this);
				return;
			}

			zombieSpawns = SpawnDataIO.Open("sm_plugins/ZombieSpawns.txt");

			LoadedZombieSpawns.Clear();

			var playerPointCount = ZombieSpawns.Count;
			var rooms = CustomRoomManager.Instance.Rooms;
			var roomCount = rooms.Count;

			// Create player spawn points on map
			for (var i = 0; i < roomCount; i++)
			{
				var r = rooms[i];

				for (var j = 0; j < playerPointCount; j++)
				{
					var p = ZombieSpawns[j];

					if (p.RoomType == r.Name && p.ZoneType == r.Zone)
					{
						LoadedZombieSpawns.Add(new PlayerSpawnPoint(p.RoomType, p.ZoneType,
							Tools.Vec3ToVec(r.Transform.TransformPoint(Tools.VecToVec3(p.Position))) + new Vector(0, 0.3f, 0),
							Tools.Vec3ToVec(r.Transform.TransformDirection(Tools.VecToVec3(p.Rotation)))));
					}
				}
			}

			useZombies = loadedZombieSpawns.Count > 0;
			if (!useZombies) Info("There are no zombie spawn points in this map seed.");
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (useZombies && UnityEngine.Random.Range(0, 101) <= percentChanceToSpawnZombies)
			{
				var spawns = ev.PlayerList;
				var spawnCount = spawns.Count;

				if (spawnCount > 0)
				{
					if (!string.IsNullOrWhiteSpace(zombieAnnounceMessage)) Server.Map.AnnounceCustomMessage(zombieAnnounceMessage);

					LoadedZombieSpawns.Shuffle();

					var index = 0;
					var LoadedSpawnsCount = LoadedZombieSpawns.Count;

					for (int i = 0; i < spawnCount; i++)
					{
						var spawn = spawns[i];
						spawn.ChangeRole(Role.SCP_049_2, true, false);
						spawn.Teleport(LoadedZombieSpawns[index].Position);

						index++;
						if (index == LoadedSpawnsCount)
						{
							index = 0;
						}
					}

					spawns.Clear();
				}
				else
				{
					Error("Tried to spawn players but there were none to spawn. (Possible addon conflict)");
				}
			}
		}
	}
}
