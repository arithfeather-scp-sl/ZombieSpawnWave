using Exiled.API.Interfaces;

namespace ArithFeather.ZombieSpawnWave {
	public class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;

		public int PercentChanceToSpawnZombies { get; set; } = 25;

		public string ZombieAnnounceMessage { get; set; }= "SCP 0 4 9 2 CONTAINMENT BREACH IN ENTRANCE ZONE";
	}
}
