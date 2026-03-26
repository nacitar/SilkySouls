using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Numerics;
using SilkySouls.Models;
using SilkySouls.Properties;

namespace SilkySouls.Utilities
{
    public static class DataLoader
    {
        public static List<Item> GetItemList(string listName)
        {
            List<Item> items = new List<Item>();

            string csvData = Resources.ResourceManager.GetString(listName);
            
            if (string.IsNullOrEmpty(csvData))
            {
                
                return new List<Item>();
            }

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        items.Add(new Item(
                           int.Parse(parts[0]),
                           int.Parse(parts[1]),
                           (UpgradeType)int.Parse(parts[2]),
                           parts[3]
                        ));
                    }
                }
            }
            return items;
        }
        
        public static Dictionary<string, List<WarpLocation>> GetLocationDict()
        {
            Dictionary<string, List<WarpLocation>> warpDict = new Dictionary<string, List<WarpLocation>>();

            string csvData = Resources.WarpLocations;
    
            if (string.IsNullOrWhiteSpace(csvData))
                return warpDict;

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        int id = int.Parse(parts[0]);

                        Vector3? coords = null;
                        float angle = 0f;
                        if (parts[1] != "0")
                        {
                            string[] coordsString = parts[1].Split('|');
    
                            coords = new Vector3(
                                float.Parse(coordsString[0], CultureInfo.InvariantCulture),
                                float.Parse(coordsString[1], CultureInfo.InvariantCulture),
                                float.Parse(coordsString[2], CultureInfo.InvariantCulture)
                            );
    
                            angle = float.Parse(coordsString[3], CultureInfo.InvariantCulture);
                        }

                        string mainArea = parts[2];
                        string name = parts[3];

                        WarpLocation location = new WarpLocation
                        {
                            Id = id,
                            Coords = coords,
                            Angle = angle,
                            MainArea = mainArea,
                            Name = name
                        };

                        if (!warpDict.ContainsKey(mainArea))
                        {
                            warpDict[mainArea] = new List<WarpLocation>();
                        }
                        warpDict[mainArea].Add(location);
                    }
                }
            }
            return warpDict;
        }
        public static Dictionary<TKey, TValue> LoadDict<TKey, TValue>(string resourceName, char separator = ',')
        {
            var dict = new Dictionary<TKey, TValue>();

            string data = Resources.ResourceManager.GetString(resourceName);
            if (string.IsNullOrWhiteSpace(data)) return dict;

            var keyConverter = TypeDescriptor.GetConverter(typeof(TKey));
            var valueConverter = TypeDescriptor.GetConverter(typeof(TValue));

            using (var reader = new StringReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(separator);
                    if (parts.Length < 2) continue;

                    var key = (TKey)keyConverter.ConvertFromInvariantString(parts[0].Trim());
                    var value = (TValue)valueConverter.ConvertFromInvariantString(parts[1].Trim());
                    dict[key] = value;
                }
            }

            return dict;
        }
    }
}