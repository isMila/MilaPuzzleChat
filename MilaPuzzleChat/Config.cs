using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MilaPuzzleChat
{
    public class Config : IRocketPluginConfiguration
    {
        public bool Enabled { get; set; }
        public int GameIntervalSeconds { get; set; }
        public int RoundTimeLimitSeconds { get; set; }
        public uint ExperienceReward { get; set; }
        public string Words { get; set; }

        [XmlArrayItem(ElementName = "Item")]
        public List<RewardItem> Rewards { get; set; }

        public void LoadDefaults()
        {
            Enabled = true;
            GameIntervalSeconds = 300;
            RoundTimeLimitSeconds = 60;
            ExperienceReward = 50;
            Words = "unturned,milahosting,zombie,survival,apocalypse,crafting,server,plugin";
            Rewards = new List<RewardItem>
            {
                new RewardItem(15, 1),
                new RewardItem(81, 30)
            };
        }
    }

    public class RewardItem
    {
        [XmlAttribute("ID")]
        public ushort Id { get; set; }
        [XmlAttribute("Amount")]
        public byte Amount { get; set; }

        public RewardItem() { }
        public RewardItem(ushort id, byte amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}