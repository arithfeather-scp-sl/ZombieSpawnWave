using System;
using System.Collections.Generic;
using ArithFeather.AriToolKit;
using ArithFeather.AriToolKit.PointEditor;
using Exiled.API.Features;
using SpawnPoint = ArithFeather.AriToolKit.PointEditor.FixedPoint;

namespace ArithFeather.ZombieSpawnWave {
	public class ZombieSpawnWave : Plugin<Config> {
		private const string SpawnPointFIleName = "ZombieSpawns";

		public override string Author => "Arith";
		public override Version Version => new Version("2.0");

		public override void OnEnabled() {
			base.OnEnabled();

			Exiled.Events.Handlers.Server.ReloadedConfigs += Server_ReloadedConfigs;
			Exiled.Events.Handlers.Server.WaitingForPlayers += Server_WaitingForPlayers;
			Exiled.Events.Handlers.Server.RespawningTeam += Server_RespawningTeam;
			Exiled.Events.Handlers.Warhead.Detonated += Warhead_Detonated;
		}

		public override void OnDisabled() {
			Exiled.Events.Handlers.Server.ReloadedConfigs -= Server_ReloadedConfigs;
			Exiled.Events.Handlers.Server.WaitingForPlayers -= Server_WaitingForPlayers;
			Exiled.Events.Handlers.Server.RespawningTeam -= Server_RespawningTeam;
			Exiled.Events.Handlers.Warhead.Detonated -= Warhead_Detonated;

			base.OnDisabled();
		}

		private void Server_ReloadedConfigs() => _spawnPointFile = PointAPI.GetPointList(SpawnPointFIleName);

		private PointList _spawnPointFile = PointAPI.GetPointList(SpawnPointFIleName);
		private bool _useZombies;

		private List<SpawnPoint> LoadedZombieSpawns => _spawnPointFile.FixedPoints;

		private void Server_WaitingForPlayers() {
			_useZombies = LoadedZombieSpawns.Count > 0;
			if (!_useZombies) Log.Warn("There are no zombie spawn points.");
			Log.Error(_useZombies);
		}

		private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev) {
			if (_useZombies && UnityEngine.Random.Range(0, 101) <= Config.PercentChanceToSpawnZombies) {
				Log.Error("Spawn zombs");
				if (!string.IsNullOrWhiteSpace(Config.ZombieAnnounceMessage))
					Cassie.Message(Config.ZombieAnnounceMessage);

				LoadedZombieSpawns.UnityShuffle();

				var index = 0;
				var LoadedSpawnsCount = LoadedZombieSpawns.Count;

				var spawns = ev.Players;
				var spawnCount = spawns.Count;
				for (int i = 0; i < spawnCount; i++) {
					var spawn = spawns[i];
					spawn.SetRole(RoleType.Scp0492);
					//spawn.Position = LoadedZombieSpawns[index].Position;

					index++;
					if (index == LoadedSpawnsCount) {
						index = 0;
					}
				}

				spawns.Clear();
			}
		}

		private void Warhead_Detonated() => _useZombies = false;
	}
}
