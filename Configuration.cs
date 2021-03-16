using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EthminerGUI
{
    public class Configuration
    {
        string localMachineName;
        public string LocalMachineName
        {
            get => localMachineName;
            set
            {
                localMachineName = Regex.Replace(value, @"[^a-zA-Z0-9-_]", string.Empty).Trim();
            }
        }
        public int SelectedIndex { get; set; }

        Miner[] miners;
        public Miner CurrentMiner
        {
            get => miners[SelectedIndex];
            set => miners[SelectedIndex] = value;
        }

        string path;

        public Configuration(string path)
        {
            this.path = path;

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var jobj = JObject.Parse(json);

                LocalMachineName = (string)jobj["machineName"];
                SelectedIndex = (int)jobj["index"];

                var miners = (JArray)jobj["miners"];
                this.miners = new Miner[miners.Count];

                for (int i = 0; i < miners.Count; i++)
                {
                    this.miners[i] = JsonConvert.DeserializeObject<Miner>(miners[i].ToString());
                }
            }
            else
            {
                LocalMachineName = Environment.MachineName;
                SelectedIndex = 0;
                miners = new Miner[1];
                miners[0] = new Miner();

                Save();
            }
        }

        public string[] GetMinerNames()
        {
            string[] names = new string[miners.Length];
            for (int i = 0; i < miners.Length; i++)
            {
                names[i] = miners[i].name.ToString();
            }
            return names;
        }

        public void Save()
        {
            var jobj = new JObject();
            jobj["machineName"] = LocalMachineName;
            jobj["index"] = SelectedIndex;
            var miners = new JArray();
            foreach (var miner in this.miners)
            {
                miners.Add(JObject.Parse(JsonConvert.SerializeObject(miner)));
            }
            jobj["miners"] = miners;

            File.WriteAllText(path, jobj.ToString());
        }
    }
}
