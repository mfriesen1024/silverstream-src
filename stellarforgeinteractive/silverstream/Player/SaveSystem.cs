using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ca.stellarforgeinteractive.silverstream.Player
{
    public static class SaveSystem
    {
        const string path = "./.save";
        const string fishFile = path + "/fish";
        const string powerFile = path + "/power";

        public static int Currency, StaminaLevel;
        public static bool DashUnlocked, WallJumpUnlocked, AirJumpUnlocked,SecondLifeUnlocked;
        
        public static void Save(PlayerStatController psc, CurrencyTracker currencyTracker)
        {
            try
            {
                Directory.CreateDirectory(path);
                File.WriteAllBytes(fishFile, BitConverter.GetBytes(currencyTracker.Currency));
                var list = Array.Empty<byte>().ToList();
                list.AddRange(BitConverter.GetBytes(psc.StaminaLevel));
                list.AddRange(BitConverter.GetBytes(psc.DashUnlocked));
                list.AddRange(BitConverter.GetBytes(psc.WallJumpUnlocked));
                list.AddRange(BitConverter.GetBytes(psc.AirJumpUnlocked));
                list.AddRange(BitConverter.GetBytes(psc.SecondLifeUnlocked));
                File.WriteAllBytes(powerFile, list.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Load()
        {
            try
            {
                var fish = File.ReadAllBytes(fishFile);
                var power = File.ReadAllBytes(powerFile);
                Currency = BitConverter.ToInt32(fish, 0);
                StaminaLevel = BitConverter.ToInt32(power, 0);
                DashUnlocked = BitConverter.ToBoolean(power, 4);
                WallJumpUnlocked = BitConverter.ToBoolean(power, 5);
                AirJumpUnlocked = BitConverter.ToBoolean(power, 6);
                SecondLifeUnlocked = BitConverter.ToBoolean(power, 7);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}